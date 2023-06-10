namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class CategoriesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IValidator<CategoryFormViewModel> _validator;
        private readonly ICategoryService _categoryService;

        public CategoriesController(IMapper mapper, IValidator<CategoryFormViewModel> validator, ICategoryService categoryService)
        {
            _mapper = mapper;
            _validator = validator;
            _categoryService = categoryService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var categories = _categoryService.GetAll();

            return View(_mapper.Map<IEnumerable<CategoryViewModel>>(categories));
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }

        [HttpPost]
        public IActionResult Create(CategoryFormViewModel model)
        {
            var validationResult = _validator.Validate(model);

            if (!validationResult.IsValid)
                return BadRequest();

            var category = _categoryService.Add(model.Name, User.GetUserId());

            return PartialView("_CategoryRow", _mapper.Map<CategoryViewModel>(category));
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var category = _categoryService.GetById(id);

            if (category is null)
                return NotFound();

            return PartialView("_Form", _mapper.Map<CategoryFormViewModel>(category));
        }

        [HttpPost]
        public IActionResult Edit(CategoryFormViewModel model)
        {
            var validationResult = _validator.Validate(model);

            if (!validationResult.IsValid)
                return BadRequest();

            var category = _categoryService.Update(model.Id, model.Name, User.GetUserId());

            //You split code as Authors Controller
            return category is null
                ? NotFound()
                : PartialView("_CategoryRow", _mapper.Map<CategoryViewModel>(category));
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var category = _categoryService.ToggleStatus(id, User.GetUserId());

            return category is null ? NotFound() : Ok(category.LastUpdatedOn.ToString());
        }

        public IActionResult AllowItem(CategoryFormViewModel model)
        {
            return Json(_categoryService.AllowCategory(model.Id, model.Name));
        }
    }
}