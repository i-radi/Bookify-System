using Bookify.Domain.Entities;
using Microsoft.AspNetCore.DataProtection;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class RentalsController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataProtector _dataProtector;
        private readonly IMapper _mapper;

        public RentalsController(IApplicationDbContext context,
            IDataProtectionProvider dataProtector,
            IMapper mapper)
        {
            _context = context;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
            _mapper = mapper;
        }

        public IActionResult Details(int id)
        {
            var rental = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .ThenInclude(c => c!.Book)
                .SingleOrDefault(r => r.Id == id);

            if (rental is null)
                return NotFound();

            var viewModel = _mapper.Map<RentalViewModel>(rental);

            return View(viewModel);
        }

        public IActionResult Create(string sKey)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(sKey));

            var subscriber = _context.Subscribers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var viewModel = new RentalFormViewModel
            {
                SubscriberKey = sKey,
                MaxAllowedCopies = maxAllowedCopies
            };

            return View("Form", viewModel);
        }

        [HttpPost]
        public IActionResult Create(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));

            var subscriber = _context.Subscribers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var (rentalsError, copies) = ValidateCopies(model.SelectedCopies, subscriberId);

            if (!string.IsNullOrEmpty(rentalsError))
                return View("NotAllowedRental", rentalsError);

            Rental rental = new()
            {
                RentalCopies = copies,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };

            subscriber.Rentals.Add(rental);
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }

        public IActionResult Edit(int id)
        {
            var rental = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .SingleOrDefault(r => r.Id == id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            var subscriber = _context.Subscribers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(s => s.Id == rental.SubscriberId);

            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber!, rental.Id);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var currentCopiesIds = rental.RentalCopies.Select(c => c.BookCopyId).ToList();

            var currentCopies = _context.BookCopies
                .Where(c => currentCopiesIds.Contains(c.Id))
                .Include(c => c.Book)
                .ToList();

            var viewModel = new RentalFormViewModel
            {
                SubscriberKey = _dataProtector.Protect(subscriber!.Id.ToString()),
                MaxAllowedCopies = maxAllowedCopies,
                CurrentCopies = _mapper.Map<IEnumerable<BookCopyViewModel>>(currentCopies)
            };

            return View("Form", viewModel);
        }

        [HttpPost]
        public IActionResult Edit(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            var rental = _context.Rentals
                .Include(r => r.RentalCopies)
                .SingleOrDefault(r => r.Id == model.Id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));

            var subscriber = _context.Subscribers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(s => s.Id == subscriberId);

            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber!, model.Id);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var (rentalsError, copies) = ValidateCopies(model.SelectedCopies, subscriberId, rental.Id);

            if (!string.IsNullOrEmpty(rentalsError))
                return View("NotAllowedRental", rentalsError);

            rental.RentalCopies = copies;
            rental.LastUpdatedById = User.GetUserId();
            rental.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }

        public IActionResult Return(int id)
        {
            var rental = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .ThenInclude(c => c!.Book)
                .SingleOrDefault(r => r.Id == id);

            if (rental is null || rental.CreatedOn.Date == DateTime.Today)
                return NotFound();

            var subscriber = _context.Subscribers
                .Include(s => s.Subscriptions)
                .SingleOrDefault(s => s.Id == rental.SubscriberId);

            var viewModel = new RentalReturnFormViewModel
            {
                Id = id,
                Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies.Where(c => !c.ReturnDate.HasValue).ToList()),
                SelectedCopies = rental.RentalCopies.Where(c => !c.ReturnDate.HasValue).Select(c => new ReturnCopyViewModel { Id = c.BookCopyId, IsReturned = c.ExtendedOn.HasValue ? false : null }).ToList(),
                AllowExtend = !subscriber!.IsBlackListed
                    && subscriber!.Subscriptions.Last().EndDate >= rental.StartDate.AddDays((int)RentalsConfigurations.MaxRentalDuration)
                    && rental.StartDate.AddDays((int)RentalsConfigurations.RentalDuration) >= DateTime.Today
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Return(RentalReturnFormViewModel model)
        {
            var rental = _context.Rentals
               .Include(r => r.RentalCopies)
               .ThenInclude(c => c.BookCopy)
               .ThenInclude(c => c!.Book)
               .SingleOrDefault(r => r.Id == model.Id);

            if (rental is null || rental.CreatedOn.Date == DateTime.Today)
                return NotFound();

            var copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies).Where(c => !c.ReturnDate.HasValue).ToList();

            if (!ModelState.IsValid)
            {
                model.Copies = copies;
                return View(model);
            }

            var subscriber = _context.Subscribers
                .Include(s => s.Subscriptions)
                .SingleOrDefault(s => s.Id == rental.SubscriberId);

            if (model.SelectedCopies.Any(c => c.IsReturned.HasValue && !c.IsReturned.Value))
            {
                string error = string.Empty;

                if (subscriber!.IsBlackListed)
                    error = Errors.RentalNotAllowedForBlacklisted;

                else if (subscriber!.Subscriptions.Last().EndDate < rental.StartDate.AddDays((int)RentalsConfigurations.MaxRentalDuration))
                    error = Errors.RentalNotAllowedForInactive;

                else if (rental.StartDate.AddDays((int)RentalsConfigurations.RentalDuration) < DateTime.Today)
                    error = Errors.ExtendNotAllowed;

                if (!string.IsNullOrEmpty(error))
                {
                    model.Copies = copies;
                    ModelState.AddModelError("", error);
                    return View(model);
                }
            }

            var isUpdated = false;

            foreach (var copy in model.SelectedCopies)
            {
                if (!copy.IsReturned.HasValue) continue;

                var currentCopy = rental.RentalCopies.SingleOrDefault(c => c.BookCopyId == copy.Id);

                if (currentCopy is null) continue;

                if (copy.IsReturned.HasValue && copy.IsReturned.Value)
                {
                    if (currentCopy.ReturnDate.HasValue) continue;

                    currentCopy.ReturnDate = DateTime.Now;
                    isUpdated = true;
                }

                if (copy.IsReturned.HasValue && !copy.IsReturned.Value)
                {
                    if (currentCopy.ExtendedOn.HasValue) continue;

                    currentCopy.ExtendedOn = DateTime.Now;
                    currentCopy.EndDate = currentCopy.RentalDate.AddDays((int)RentalsConfigurations.MaxRentalDuration);
                    isUpdated = true;
                }
            }

            if (isUpdated)
            {
                rental.LastUpdatedOn = DateTime.Now;
                rental.LastUpdatedById = User.GetUserId();
                rental.PenaltyPaid = model.PenaltyPaid;

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }

        [HttpPost]
        public IActionResult GetCopyDetails(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var copy = _context.BookCopies
                .Include(c => c.Book)
                .SingleOrDefault(c => c.SerialNumber.ToString() == model.Value && !c.IsDeleted && !c.Book!.IsDeleted);

            if (copy is null)
                return NotFound(Errors.InvalidSerialNumber);

            if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                return BadRequest(Errors.NotAvilableRental);

            //check that copy is not in rental
            var copyIsInRental = _context.RentalCopies.Any(c => c.BookCopyId == copy.Id && !c.ReturnDate.HasValue);

            if (copyIsInRental)
                return BadRequest(Errors.CopyIsInRental);

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_CopyDetails", viewModel);
        }

        [HttpPost]
        public IActionResult MarkAsDeleted(int id)
        {
            var rental = _context.Rentals.Find(id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            rental.IsDeleted = true;
            rental.LastUpdatedOn = DateTime.Now;
            rental.LastUpdatedById = User.GetUserId();

            _context.SaveChanges();

            var copiesCount = _context.RentalCopies.Count(r => r.RentalId == id);

            return Ok(copiesCount);
        }

        private (string errorMessage, int? maxAllowedCopies) ValidateSubscriber(Subscriber subscriber, int? rentalId = null)
        {
            if (subscriber.IsBlackListed)
                return (errorMessage: Errors.BlackListedSubscriber, maxAllowedCopies: null);

            if (subscriber.Subscriptions.Last().EndDate < DateTime.Today.AddDays((int)RentalsConfigurations.RentalDuration))
                return (errorMessage: Errors.InactiveSubscriber, maxAllowedCopies: null);

            var currentRentals = subscriber.Rentals
                .Where(r => rentalId == null || r.Id != rentalId)
                .SelectMany(r => r.RentalCopies)
                .Count(c => !c.ReturnDate.HasValue);

            var availableCopiesCount = (int)RentalsConfigurations.MaxAllowedCopies - currentRentals;

            if (availableCopiesCount.Equals(0))
                return (errorMessage: Errors.MaxCopiesReached, maxAllowedCopies: null);

            return (errorMessage: string.Empty, maxAllowedCopies: availableCopiesCount);
        }

        private (string errorMessage, ICollection<RentalCopy> copies) ValidateCopies(IEnumerable<int> selectedSerials, int subscriberId, int? rentalId = null)
        {
            var selectedCopies = _context.BookCopies
                .Include(c => c.Book)
                .Include(c => c.Rentals)
                .Where(c => selectedSerials.Contains(c.SerialNumber))
                .ToList();

            var currentSubscriberRentals = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .Where(r => r.SubscriberId == subscriberId && (rentalId == null || r.Id != rentalId))
                .SelectMany(r => r.RentalCopies)
                .Where(c => !c.ReturnDate.HasValue)
                .Select(c => c.BookCopy!.BookId)
                .ToList();

            List<RentalCopy> copies = new();

            foreach (var copy in selectedCopies)
            {
                if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                    return (errorMessage: Errors.NotAvilableRental, copies);

                if (copy.Rentals.Any(c => !c.ReturnDate.HasValue && (rentalId == null || c.RentalId != rentalId)))
                    return (errorMessage: Errors.CopyIsInRental, copies);

                if (currentSubscriberRentals.Any(bookId => bookId == copy.BookId))
                    return (errorMessage: $"This subscriber already has a copy for '{copy.Book.Title}' Book", copies);

                copies.Add(new RentalCopy { BookCopyId = copy.Id });
            }

            return (errorMessage: string.Empty, copies);
        }
    }
}