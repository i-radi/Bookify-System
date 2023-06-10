namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class AuthorsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IValidator<AuthorFormViewModel> _validator;
        private readonly IAuthorService _authorService;

        public AuthorsController(
            IMapper mapper,
            IValidator<AuthorFormViewModel> validator,
            IAuthorService authorService)
        {
            _mapper = mapper;
            _validator = validator;
            _authorService = authorService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var authors = _authorService.GetAll();

            var viewModel = _mapper.Map<IEnumerable<AuthorViewModel>>(authors);

            return View(viewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }

        [HttpPost]
        public IActionResult Create(AuthorFormViewModel model)
        {
            var validationResult = _validator.Validate(model);

            if (!validationResult.IsValid)
                return BadRequest();

            var author = _authorService.Add(model.Name, User.GetUserId());

            return PartialView("_AuthorRow", _mapper.Map<AuthorViewModel>(author));
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var author = _authorService.GetById(id);

            if (author is null)
                return NotFound();

            var viewModel = _mapper.Map<AuthorFormViewModel>(author);

            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        public IActionResult Edit(AuthorFormViewModel model)
        {
            var validationResult = _validator.Validate(model);

            if (!validationResult.IsValid)
                return BadRequest();

            var author = _authorService.Update(model.Id, model.Name, User.GetUserId());

            if (author is null)
                return NotFound();

            return PartialView("_AuthorRow", _mapper.Map<AuthorViewModel>(author));
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var author = _authorService.ToggleStatus(id, User.GetUserId());

            if (author is null)
                return NotFound();

            return Ok(author.LastUpdatedOn.ToString());
        }

        public IActionResult AllowItem(AuthorFormViewModel model)
        {
            return Json(_authorService.AllowAuthor(model.Id, model.Name));
        }
    }
}