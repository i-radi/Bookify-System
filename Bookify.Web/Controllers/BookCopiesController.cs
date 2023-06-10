namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BookCopiesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IValidator<BookCopyFormViewModel> _validator;
        private readonly IBookService _bookService;
        private readonly IBookCopyService _bookCopyService;
        private readonly IRentalService _rentalService;

        public BookCopiesController(
            IMapper mapper,
            IValidator<BookCopyFormViewModel> validator,
            IBookService bookService,
            IBookCopyService bookCopyService,
            IRentalService rentalService)
        {
            _mapper = mapper;
            _validator = validator;
            _bookService = bookService;
            _bookCopyService = bookCopyService;
            _rentalService = rentalService;
        }

        [AjaxOnly]
        public IActionResult Create(int bookId)
        {
            var book = _bookService.GetById(bookId);

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

            var copy = _bookCopyService.Add(model.BookId, model.EditionNumber, model.IsAvailableForRental, User.GetUserId());

            if (copy is null)
                return NotFound();

            return PartialView("_BookCopyRow", _mapper.Map<BookCopyViewModel>(copy));
        }

        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var copy = _bookCopyService.GetDetails(id);

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

            var copy = _bookCopyService.Update(model.Id, model.EditionNumber, model.IsAvailableForRental, User.GetUserId());

            if (copy is null)
                return NotFound();

            return PartialView("_BookCopyRow", _mapper.Map<BookCopyViewModel>(copy));
        }

        public IActionResult RentalHistory(int id)
        {
            var copyHistory = _rentalService.GetAllByCopyId(id);

            return View(_mapper.Map<IEnumerable<CopyHistoryViewModel>>(copyHistory));
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var copy = _bookCopyService.ToggleStatus(id, User.GetUserId());

            return copy is null ? NotFound() : Ok();
        }
    }
}