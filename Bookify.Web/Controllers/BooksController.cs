using CloudinaryDotNet;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BooksController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;
        private readonly IValidator<BookFormViewModel> _validator;
        private readonly Cloudinary _cloudinary;
        private readonly IImageService _imageService;
        private readonly IBookService _bookService;
        private readonly IAuthorService _authorService;
        private readonly ICategoryService _categoryService;

        private List<string> _allowedExtensions = new() { ".jpg", ".jpeg", ".png" };
        private int _maxAllowedSize = 2097152;

        public BooksController(IWebHostEnvironment webHostEnvironment,
            IMapper mapper,
            IValidator<BookFormViewModel> validator,
            IOptions<CloudinarySettings> cloudinary,
            IImageService imageService,
            IBookService bookService,
            IAuthorService authorService,
            ICategoryService categoryService)
        {
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
            _validator = validator;

            Account account = new()
            {
                Cloud = cloudinary.Value.Cloud,
                ApiKey = cloudinary.Value.ApiKey,
                ApiSecret = cloudinary.Value.ApiSecret
            };

            _cloudinary = new Cloudinary(account);
            _imageService = imageService;
            _bookService = bookService;
            _authorService = authorService;
            _categoryService = categoryService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost, IgnoreAntiforgeryToken]
        public IActionResult GetBooks()
        {
            var filterDto = Request.Form.GetFilters();

            var (books, recordsTotal) = _bookService.GetFiltered(filterDto);

            var mappedData = _mapper.ProjectTo<BookRowViewModel>(books).ToList();

            var jsonData = new { recordsFiltered = recordsTotal, recordsTotal, data = mappedData };

            return Ok(jsonData);
        }

        public IActionResult Details(int id)
        {
            var query = _bookService.GetDetails();

            var viewModel = _mapper.ProjectTo<BookViewModel>(query)
                .SingleOrDefault(b => b.Id == id);

            if (viewModel is null)
                return NotFound();

            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View("Form", PopulateViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            var validationResult = _validator.Validate(model);

            if (!validationResult.IsValid)
                validationResult.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var book = _mapper.Map<Book>(model);

            if (model.Image is not null)
            {
                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
                var imagePath = "/images/books";

                var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, imagePath, hasThumbnail: true);

                if (!isUploaded)
                {
                    ModelState.AddModelError(nameof(Image), errorMessage!);
                    return View("Form", PopulateViewModel(model));

                }

                book.ImageUrl = $"{imagePath}/{imageName}";
                book.ImageThumbnailUrl = $"{imagePath}/thumb/{imageName}";

                //using var straem = model.Image.OpenReadStream();

                //var imageParams = new ImageUploadParams
                //{
                //    File = new FileDescription(imageName, straem),
                //    UseFilename = true
                //};

                //var result = await _cloudinary.UploadAsync(imageParams);

                //book.ImageUrl = result.SecureUrl.ToString();
                //book.ImageThumbnailUrl = GetThumbnailUrl(book.ImageUrl);
                //book.ImagePublicId = result.PublicId;
            }

            book = _bookService.Add(book, model.SelectedCategories, User.GetUserId());

            return RedirectToAction(nameof(Details), new { id = book.Id });
        }

        public IActionResult Edit(int id)
        {
            var book = _bookService.GetWithCategories(id);

            if (book is null)
                return NotFound();

            var model = _mapper.Map<BookFormViewModel>(book);
            var viewModel = PopulateViewModel(model);

            viewModel.SelectedCategories = book.Categories.Select(c => c.CategoryId).ToList();

            return View("Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {
            var validationResult = _validator.Validate(model);

            if (!validationResult.IsValid)
                validationResult.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var book = _bookService.GetWithCategories(model.Id);

            if (book is null)
                return NotFound();

            //string imagePublicId = null;

            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(book.ImageUrl))
                {
                    _imageService.Delete(book.ImageUrl, book.ImageThumbnailUrl);

                    //await _cloudinary.DeleteResourcesAsync(book.ImagePublicId);
                }

                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
                var imagePath = "/images/books";

                var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, imagePath, hasThumbnail: true);

                if (!isUploaded)
                {
                    ModelState.AddModelError(nameof(Image), errorMessage!);
                    return View("Form", PopulateViewModel(model));
                }

                model.ImageUrl = $"{imagePath}/{imageName}";
                model.ImageThumbnailUrl = $"{imagePath}/thumb/{imageName}";

                //using var straem = model.Image.OpenReadStream();

                //var imageParams = new ImageUploadParams
                //{
                //    File = new FileDescription(imageName, straem),
                //    UseFilename = true
                //};

                //var result = await _cloudinary.UploadAsync(imageParams);

                //model.ImageUrl = result.SecureUrl.ToString();
                //imagePublicId = result.PublicId;
            }

            else if (!string.IsNullOrEmpty(book.ImageUrl))
            {
                model.ImageUrl = book.ImageUrl;
                model.ImageThumbnailUrl = book.ImageThumbnailUrl;
            }

            book = _mapper.Map(model, book);

            book = _bookService.Update(book, model.SelectedCategories, User.GetUserId());

            return RedirectToAction(nameof(Details), new { id = book.Id });
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var book = _bookService.ToggleStatus(id, User.GetUserId());

            return book is null ? NotFound() : Ok();
        }

        public IActionResult AllowItem(BookFormViewModel model)
        {
            return Json(_bookService.AllowTitle(model.Id, model.Title, model.AuthorId));
        }

        private BookFormViewModel PopulateViewModel(BookFormViewModel? model = null)
        {
            BookFormViewModel viewModel = model is null ? new BookFormViewModel() : model;

            var authors = _authorService.GetActiveAuthors();
            var categories = _categoryService.GetActiveCategories();

            viewModel.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
            viewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);

            return viewModel;
        }

        private string GetThumbnailUrl(string url)
        {
            var separator = "image/upload/";
            var urlParts = url.Split(separator);

            var thumbnailUrl = $"{urlParts[0]}{separator}c_thumb,w_200,g_face/{urlParts[1]}";

            return thumbnailUrl;
        }
    }
}