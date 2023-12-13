namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class CategoriesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IValidator<CategoryFormViewModel> _validator;
        private readonly ICategoryService _categoryService;
        private readonly string currentCulture = Thread.CurrentThread.CurrentCulture.Name;

        public CategoriesController(IMapper mapper, IValidator<CategoryFormViewModel> validator, ICategoryService categoryService)
        {
            _mapper = mapper;
            _validator = validator;
            _categoryService = categoryService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var categories = _categoryService.GetAll(currentCulture);

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

            var name = new List<LocalizationDto>
            {
                new LocalizationDto(AppCultures.English, model.NameInEnglish),
                new LocalizationDto(AppCultures.Arabic, model.NameInArabic)
            };

            var category = _categoryService.Add(name, User.GetUserId());
            var viewModel = _mapper.Map<CategoryViewModel>(category);
            //viewModel.Name = currentCulture == AppCultures.Arabic ? model.NameInArabic : model.NameInEnglish;

            return PartialView("_CategoryRow", viewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var category = _categoryService.GetById(id);

            if (category is null)
                return NotFound();

            var viewModel = new CategoryFormViewModel
            {
                Id = id,
                NameInEnglish = category.Name.Localizations.SingleOrDefault(c => c.CultureCode == AppCultures.English)?.Value!,
                NameInArabic = category.Name.Localizations.SingleOrDefault(c => c.CultureCode == AppCultures.Arabic)?.Value!,
            };

            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        public IActionResult Edit(CategoryFormViewModel model)
        {
            var validationResult = _validator.Validate(model);

            if (!validationResult.IsValid)
                return BadRequest();

            var name = new List<LocalizationDto>
            {
                new LocalizationDto(AppCultures.English, model.NameInEnglish),
                new LocalizationDto(AppCultures.Arabic, model.NameInArabic)
            };

            var category = _categoryService.Update(model.Id, name, User.GetUserId());

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

        public IActionResult AllowEnglishItem(CategoryFormViewModel model)
        {
            return Json(_categoryService.AllowCategory(model.Id, model.NameInEnglish, AppCultures.English));
        }

        public IActionResult AllowArabicItem(CategoryFormViewModel model)
        {
            return Json(_categoryService.AllowCategory(model.Id, model.NameInArabic, AppCultures.Arabic));
        }
    }
}