using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.Rendering;
using OpenHtmlToPdf;
using System.Net.Mime;
using ViewToHTML.Services;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class ReportsController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHost;
        private readonly IMapper _mapper;
        private readonly IViewRendererService _viewRendererService;
        private readonly IAuthorService _authorService;
        private readonly ICategoryService _categoryService;
        private readonly IBookService _bookService;
        private readonly IRentalService _rentalService;

        private readonly string _logoPath;
        private readonly int _sheetStartRow = 5;

        public ReportsController(IApplicationDbContext context,
            IMapper mapper,
            IWebHostEnvironment webHost,
            IViewRendererService viewRendererService,
            IAuthorService authorService,
            ICategoryService categoryService,
            IBookService bookService,
            IRentalService rentalService)
        {
            _context = context;
            _mapper = mapper;
            _webHost = webHost;
            _viewRendererService = viewRendererService;

            _logoPath = $"{_webHost.WebRootPath}/assets/images/Logo.png";
            _authorService = authorService;
            _categoryService = categoryService;
            _bookService = bookService;
            _rentalService = rentalService;
        }


        public IActionResult Index()
        {
            return View();
        }

        #region Books
        public IActionResult Books(IList<int> selectedAuthors, IList<int> selectedCategories,
            int? pageNumber)
        {
            var authors = _authorService.GetActiveAuthors();
            var categories = _categoryService.GetActiveCategories();

            var viewModel = new BooksReportViewModel
            {
                Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors),
                Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories)
            };

            if (pageNumber is not null)
                viewModel.Books = _bookService.GetPaginatedList(selectedAuthors, selectedCategories, pageNumber ?? 0, (int)ReportsConfigurations.PageSize);

            return View(viewModel);
        }

        public async Task<IActionResult> ExportBooksToExcel(string authors, string categories)
        {
            var query = _bookService.GetQuerbaleRawData(authors, categories);

            var books = _mapper.ProjectTo<BookViewModel>(query).ToList();

            using var workbook = new XLWorkbook();

            var sheet = workbook.AddWorksheet("Books");

            sheet.AddLocalImage(_logoPath);

            var headerCells = new string[] { "Title", "Author", "Categories", "Publisher",
                "Publishing Date", "Hall", "Available for rental", "Status" };

            sheet.AddHeader(headerCells);

            for (int i = 0; i < books.Count; i++)
            {
                sheet.Cell(i + _sheetStartRow, 1).SetValue(books[i].Title);
                sheet.Cell(i + _sheetStartRow, 2).SetValue(books[i].Author);
                sheet.Cell(i + _sheetStartRow, 3).SetValue(string.Join(", ", books[i].Categories));
                sheet.Cell(i + _sheetStartRow, 4).SetValue(books[i].Publisher);
                sheet.Cell(i + _sheetStartRow, 5).SetValue(books[i].PublishingDate.ToString("d MMM, dddd"));
                sheet.Cell(i + _sheetStartRow, 6).SetValue(books[i].Hall);
                sheet.Cell(i + _sheetStartRow, 7).SetValue(books[i].IsAvailableForRental ? "Yes" : "No");
                sheet.Cell(i + _sheetStartRow, 8).SetValue(books[i].IsDeleted ? "Deleted" : "Available");
            }

            sheet.Format();
            sheet.AddTable(books.Count, headerCells.Length);
            sheet.ShowGridLines = false;

            await using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(stream.ToArray(), MediaTypeNames.Application.Octet, "Books.xlsx");
        }

        public async Task<IActionResult> ExportBooksToPDF(string authors, string categories)
        {
            var query = _bookService.GetQuerbaleRawData(authors, categories);

            var books = _mapper.ProjectTo<BookViewModel>(query).ToList();

            var templatePath = "~/Views/Reports/BooksTemplate.cshtml";
            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templatePath, books);

            var pdf = Pdf
                .From(html)
                .EncodedWith("Utf-8")
                .OfSize(PaperSize.A4)
                .WithMargins(1.Centimeters())
                .Landscape()
                .Content();

            return File(pdf.ToArray(), MediaTypeNames.Application.Octet, "Books.pdf");
        }
        #endregion

        #region Rentals
        public IActionResult Rentals(string duration, int? pageNumber)
        {
            var viewModel = new RentalsReportViewModel { Duration = duration };

            if (!string.IsNullOrEmpty(duration))
            {
                if (!DateTime.TryParse(duration.Split(" - ")[0], out DateTime from))
                {
                    ModelState.AddModelError("Duration", Errors.InvalidStartDate);
                    return View(viewModel);
                }

                if (!DateTime.TryParse(duration.Split(" - ")[1], out DateTime to))
                {
                    ModelState.AddModelError("Duration", Errors.InvalidEndDate);
                    return View(viewModel);
                }

                if (pageNumber is not null)
                    viewModel.Rentals = _rentalService.GetPaginatedList(from, to, pageNumber ?? 0, (int)ReportsConfigurations.PageSize);
            }

            ModelState.Clear();

            return View(viewModel);
        }

        public async Task<IActionResult> ExportRentalsToExcel(string duration)
        {
            var query = _rentalService.GetQuerbaleRawData(duration);

            var rentals = _mapper.ProjectTo<RentalCopiesViewModel>(query).ToList();

            using var workbook = new XLWorkbook();

            var sheet = workbook.AddWorksheet("Rentals");

            sheet.AddLocalImage(_logoPath);

            var headerCells = new string[] { "Subscriber ID", "Subscriber Name", "Subscriber Phone", "Book Title",
                "Book Author", "SerialNumber", "Rental Date", "End Date", "Return Date", "Extended On" };

            sheet.AddHeader(headerCells);

            for (int i = 0; i < rentals.Count; i++)
            {
                sheet.Cell(i + _sheetStartRow, 1).SetValue(rentals[i].SubscriberId);
                sheet.Cell(i + _sheetStartRow, 2).SetValue(rentals[i].SubscriberName);
                sheet.Cell(i + _sheetStartRow, 3).SetValue(rentals[i].SubscriberMobile);
                sheet.Cell(i + _sheetStartRow, 4).SetValue(rentals[i].BookTitle);
                sheet.Cell(i + _sheetStartRow, 5).SetValue(rentals[i].BookAuthor);
                sheet.Cell(i + _sheetStartRow, 6).SetValue(rentals[i].CopySerialNumber);
                sheet.Cell(i + _sheetStartRow, 7).SetValue(rentals[i].RentalDate.ToString("d MMM, yyyy"));
                sheet.Cell(i + _sheetStartRow, 8).SetValue(rentals[i].EndDate.ToString("d MMM, yyyy"));
                sheet.Cell(i + _sheetStartRow, 9).SetValue(rentals[i].ReturnDate is null ? "-" : rentals[i].ReturnDate?.ToString("d MMM, yyyy"));
                sheet.Cell(i + _sheetStartRow, 10).SetValue(rentals[i].ExtendedOn is null ? "-" : rentals[i].ExtendedOn?.ToString("d MMM, yyyy"));
            }
            sheet.Format();
            sheet.AddTable(rentals.Count, headerCells.Length);
            sheet.ShowGridLines = false;

            await using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(stream.ToArray(), MediaTypeNames.Application.Octet, "Rentals.xlsx");
        }

        public async Task<IActionResult> ExportRentalsToPDF(string duration)
        {
            var query = _rentalService.GetQuerbaleRawData(duration);

            var rentals = _mapper.ProjectTo<RentalCopiesViewModel>(query).ToList();

            var templatePath = "~/Views/Reports/RentalsTemplate.cshtml";
            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templatePath, rentals);

            var pdf = Pdf
                .From(html)
                .EncodedWith("Utf-8")
                .OfSize(PaperSize.A4)
                .WithMargins(1.Centimeters())
                .Landscape()
                .Content();

            return File(pdf.ToArray(), MediaTypeNames.Application.Octet, "Rentals.pdf");
        }
        #endregion

        #region DelayedRentals
        public IActionResult DelayedRentals()
        {
            var query = _rentalService.GetQuerbaleDelayedRawData();

            return View(_mapper.ProjectTo<RentalCopiesViewModel>(query).ToList());
        }

        public async Task<IActionResult> ExportDelayedRentalsToExcel()
        {
            var query = _rentalService.GetQuerbaleDelayedRawData();

            var data = _mapper.ProjectTo<RentalCopiesViewModel>(query).ToList();

            using var workbook = new XLWorkbook();

            var sheet = workbook.AddWorksheet("Delayed Rentals");

            sheet.AddLocalImage(_logoPath);

            var headerCells = new string[] { "Subscriber ID", "Subscriber Name", "Subscriber Phone", "Book Title",
                "Book Serial", "Rental Date", "End Date", "Extended On", "Delay in Days" };

            sheet.AddHeader(headerCells);

            for (int i = 0; i < data.Count; i++)
            {
                sheet.Cell(i + _sheetStartRow, 1).SetValue(data[i].SubscriberId);
                sheet.Cell(i + _sheetStartRow, 2).SetValue(data[i].SubscriberName);
                sheet.Cell(i + _sheetStartRow, 3).SetValue(data[i].CopySerialNumber);
                sheet.Cell(i + _sheetStartRow, 4).SetValue(data[i].BookTitle);
                sheet.Cell(i + _sheetStartRow, 5).SetValue(data[i].CopySerialNumber);
                sheet.Cell(i + _sheetStartRow, 6).SetValue(data[i].RentalDate.ToString("d MMM, yyyy"));
                sheet.Cell(i + _sheetStartRow, 7).SetValue(data[i].EndDate.ToString("d MMM, yyyy"));
                sheet.Cell(i + _sheetStartRow, 8).SetValue(data[i].ExtendedOn is null ? "-" : data[i].ExtendedOn?.ToString("d MMM, yyyy"));
                sheet.Cell(i + _sheetStartRow, 9).SetValue(data[i].DelayInDays);
            }

            sheet.Format();
            sheet.AddTable(data.Count, headerCells.Length);
            sheet.ShowGridLines = false;

            await using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(stream.ToArray(), MediaTypeNames.Application.Octet, "DelayedRentals.xlsx");
        }

        public async Task<IActionResult> ExportDelayedRentalsToPDF()
        {

            var query = _rentalService.GetQuerbaleDelayedRawData();

            var data = _mapper.ProjectTo<RentalCopiesViewModel>(query).ToList();

            var templatePath = "~/Views/Reports/DelayedRentalsTemplate.cshtml";
            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templatePath, data);

            var pdf = Pdf
                .From(html)
                .EncodedWith("Utf-8")
                .OfSize(PaperSize.A4)
                .WithMargins(1.Centimeters())
                .Landscape()
                .Content();

            return File(pdf.ToArray(), MediaTypeNames.Application.Octet, "DelayedRentals.pdf");
        }
        #endregion
    }
}