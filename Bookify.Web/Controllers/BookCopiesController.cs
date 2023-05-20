using FluentValidation;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BookCopiesController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<BookCopyFormViewModel> _validator;

        public BookCopiesController(IApplicationDbContext context, 
            IMapper mapper, 
            IValidator<BookCopyFormViewModel> validator)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
        }

        [AjaxOnly]
        public IActionResult Create(int bookId)
        {
            var book = _context.Books.Find(bookId);

            if (book is null)
                return NotFound();

            var viewModel = new BookCopyFormViewModel
            {
                BookId = bookId,
                ShowRentalInput = book.IsAvailableForRental
            };

            return PartialView("Form", viewModel);
        }

        [HttpPost]
        public IActionResult Create(BookCopyFormViewModel model)
        {
            var validationResult = _validator.Validate(model);

            if (!validationResult.IsValid)
                return BadRequest();

            var book = _context.Books.Find(model.BookId);

            if (book is null)
                return NotFound();

            BookCopy copy = new()
            {
                EditionNumber = model.EditionNumber,
                IsAvailableForRental = book.IsAvailableForRental && model.IsAvailableForRental,
                CreatedById = User.GetUserId()
            };

            book.Copies.Add(copy);
            _context.SaveChanges();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow", viewModel);
        }

        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var copy = _context.BookCopies.Include(c => c.Book).SingleOrDefault(c => c.Id == id);

            if (copy is null)
                return NotFound();

            var viewModel = _mapper.Map<BookCopyFormViewModel>(copy);
            viewModel.ShowRentalInput = copy.Book!.IsAvailableForRental;

            return PartialView("Form", viewModel);
        }

        [HttpPost]
        public IActionResult Edit(BookCopyFormViewModel model)
        {
            var validationResult = _validator.Validate(model);

            if (!validationResult.IsValid)
                return BadRequest();

            var copy = _context.BookCopies.Include(c => c.Book).SingleOrDefault(c => c.Id == model.Id);

            if (copy is null)
                return NotFound();

            copy.EditionNumber = model.EditionNumber;
            copy.IsAvailableForRental = copy.Book!.IsAvailableForRental && model.IsAvailableForRental;
            copy.LastUpdatedById = User.GetUserId();
            copy.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow", viewModel);
        }

        public IActionResult RentalHistory(int id)
        {
            var copyHistory = _context.RentalCopies
                .Include(c => c.Rental)
                .ThenInclude(r => r!.Subscriber)
                .Where(c => c.BookCopyId == id)
                .OrderByDescending(c => c.RentalDate)
                .ToList();

            var viewModel = _mapper.Map<IEnumerable<CopyHistoryViewModel>>(copyHistory);

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var copy = _context.BookCopies.Find(id);

            if (copy is null)
                return NotFound();

            copy.IsDeleted = !copy.IsDeleted;
            copy.LastUpdatedById = User.GetUserId();
            copy.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();

            return Ok();
        }
    }
}