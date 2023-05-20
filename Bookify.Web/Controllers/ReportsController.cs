using Bookify.Domain.Entities;
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

        private readonly string _logoPath;
        private readonly int _sheetStartRow = 5;

        public ReportsController(IApplicationDbContext context, IMapper mapper,
            IWebHostEnvironment webHost, IViewRendererService viewRendererService)
        {
            _context = context;
            _mapper = mapper;
            _webHost = webHost;
            _viewRendererService = viewRendererService;

            _logoPath = $"{_webHost.WebRootPath}/assets/images/Logo.png";
        }


        public IActionResult Index()
        {
            return View();
        }

        #region Books
        public IActionResult Books(IList<int> selectedAuthors, IList<int> selectedCategories,
            int? pageNumber)
        {
            var authors = _context.Authors.OrderBy(a => a.Name).ToList();
            var categories = _context.Categories.OrderBy(a => a.Name).ToList();

            IQueryable<Book> books = _context.Books
                        .Include(b => b.Author)
                        .Include(b => b.Categories)
                        .ThenInclude(c => c.Category)
                        .Where(b => (!selectedAuthors.Any() || selectedAuthors.Contains(b.AuthorId))
                        && (!selectedCategories.Any() || b.Categories.Any(c => selectedCategories.Contains(c.CategoryId))));

            //if (selectedAuthors.Any())
            //    books = books.Where(b => selectedAuthors.Contains(b.AuthorId));

            //if (selectedCategories.Any())
            //    books = books.Where(b => b.Categories.Any(c => selectedCategories.Contains(c.CategoryId)));

            var viewModel = new BooksReportViewModel
            {
                Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors),
                Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories)
            };

            if (pageNumber is not null)
                viewModel.Books = PaginatedList<Book>.Create(books, pageNumber ?? 0, (int)ReportsConfigurations.PageSize);

            return View(viewModel);
        }

        public async Task<IActionResult> ExportBooksToExcel(string authors, string categories)
        {
            var selectedAuthors = authors?.Split(',');
            var selectedCategories = categories?.Split(',');

            var books = _context.Books
                        .Include(b => b.Author)
                        .Include(b => b.Categories)
                        .ThenInclude(c => c.Category)
                        .Where(b => (string.IsNullOrEmpty(authors) || selectedAuthors!.Contains(b.AuthorId.ToString()))
                            && (string.IsNullOrEmpty(categories) || b.Categories.Any(c => selectedCategories!.Contains(c.CategoryId.ToString()))))
                        .ToList();

            using var workbook = new XLWorkbook();

            var sheet = workbook.AddWorksheet("Books");

            sheet.AddLocalImage(_logoPath);

            var headerCells = new string[] { "Title", "Author", "Categories", "Publisher",
                "Publishing Date", "Hall", "Available for rental", "Status" };

            sheet.AddHeader(headerCells);

            for (int i = 0; i < books.Count; i++)
            {
                sheet.Cell(i + _sheetStartRow, 1).SetValue(books[i].Title);
                sheet.Cell(i + _sheetStartRow, 2).SetValue(books[i].Author!.Name);
                sheet.Cell(i + _sheetStartRow, 3).SetValue(string.Join(", ", books[i].Categories!.Select(c => c.Category!.Name)));
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
            var selectedAuthors = authors?.Split(',');
            var selectedCategories = categories?.Split(',');

            var books = _context.Books
                        .Include(b => b.Author)
                        .Include(b => b.Categories)
                        .ThenInclude(c => c.Category)
                        .Where(b => (string.IsNullOrEmpty(authors) || selectedAuthors!.Contains(b.AuthorId.ToString()))
                            && (string.IsNullOrEmpty(categories) || b.Categories.Any(c => selectedCategories!.Contains(c.CategoryId.ToString()))))
                        .ToList();

            //var html = await System.IO.File.ReadAllTextAsync($"{_webHost.WebRootPath}/templates/report.html");

            //html = html.Replace("[Title]", "Books");

            //var body = "<table><thead><tr><th>Title</th><th>Author</th></tr></thead><tbody>";

            //foreach (var book in books)
            //{
            //    body += $"<tr><td>{book.Title}</td><td>{book.Author!.Name}</td></tr>";
            //}

            //body += "</body></table>";

            //html = html.Replace("[body]", body);

            var viewModel = _mapper.Map<IEnumerable<BookViewModel>>(books);

            var templatePath = "~/Views/Reports/BooksTemplate.cshtml";
            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templatePath, viewModel);

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

                IQueryable<RentalCopy> rentals = _context.RentalCopies
                        .Include(c => c.BookCopy)
                        .ThenInclude(r => r!.Book)
                        .ThenInclude(b => b!.Author)
                        .Include(c => c.Rental)
                        .ThenInclude(c => c!.Subscriber)
                        .Where(r => r.RentalDate >= from && r.RentalDate <= to);

                if (pageNumber is not null)
                    viewModel.Rentals = PaginatedList<RentalCopy>.Create(rentals, pageNumber ?? 0, (int)ReportsConfigurations.PageSize);
            }

            ModelState.Clear();

            return View(viewModel);
        }

        public async Task<IActionResult> ExportRentalsToExcel(string duration)
        {
            var from = DateTime.Parse(duration.Split(" - ")[0]);
            var to = DateTime.Parse(duration.Split(" - ")[1]);

            var rentals = _context.RentalCopies
                        .Include(c => c.BookCopy)
                        .ThenInclude(r => r!.Book)
                        .ThenInclude(b => b!.Author)
                        .Include(c => c.Rental)
                        .ThenInclude(c => c!.Subscriber)
                        .Where(r => !string.IsNullOrEmpty(duration) && r.RentalDate >= from && r.RentalDate <= to)
                        .ToList();

            using var workbook = new XLWorkbook();

            var sheet = workbook.AddWorksheet("Rentals");

            sheet.AddLocalImage(_logoPath);

            var headerCells = new string[] { "Subscriber ID", "Subscriber Name", "Subscriber Phone", "Book Title",
                "Book Author", "SerialNumber", "Rental Date", "End Date", "Return Date", "Extended On" };

            sheet.AddHeader(headerCells);

            for (int i = 0; i < rentals.Count; i++)
            {
                sheet.Cell(i + _sheetStartRow, 1).SetValue(rentals[i].Rental!.Subscriber!.Id);
                sheet.Cell(i + _sheetStartRow, 2).SetValue($"{rentals[i].Rental!.Subscriber!.FirstName} {rentals[i].Rental!.Subscriber!.LastName}");
                sheet.Cell(i + _sheetStartRow, 3).SetValue(rentals[i].Rental!.Subscriber!.MobileNumber);
                sheet.Cell(i + _sheetStartRow, 4).SetValue(rentals[i].BookCopy!.Book!.Title);
                sheet.Cell(i + _sheetStartRow, 5).SetValue(rentals[i].BookCopy!.Book!.Author!.Name);
                sheet.Cell(i + _sheetStartRow, 6).SetValue(rentals[i].BookCopy!.SerialNumber);
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
            var from = DateTime.Parse(duration.Split(" - ")[0]);
            var to = DateTime.Parse(duration.Split(" - ")[1]);

            var rentals = _context.RentalCopies
                        .Include(c => c.BookCopy)
                        .ThenInclude(r => r!.Book)
                        .ThenInclude(b => b!.Author)
                        .Include(c => c.Rental)
                        .ThenInclude(c => c!.Subscriber)
                        .Where(r => !string.IsNullOrEmpty(duration) && r.RentalDate >= from && r.RentalDate <= to)
                        .ToList();

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
            var rentals = _context.RentalCopies
                        .Include(c => c.BookCopy)
                        .ThenInclude(r => r!.Book)
                        .Include(c => c.Rental)
                        .ThenInclude(c => c!.Subscriber)
                        .Where(c => !c.ReturnDate.HasValue && c.EndDate < DateTime.Today)
                        .ToList();

            var viewModel = _mapper.Map<IEnumerable<RentalCopyViewModel>>(rentals);

            return View(viewModel);
        }

        public async Task<IActionResult> ExportDelayedRentalsToExcel()
        {
            var rentals = _context.RentalCopies
                           .Include(c => c.BookCopy)
                           .ThenInclude(r => r!.Book)
                           .Include(c => c.Rental)
                           .ThenInclude(c => c!.Subscriber)
                           .Where(c => !c.ReturnDate.HasValue && c.EndDate < DateTime.Today)
                           .ToList();

            var data = _mapper.Map<IList<RentalCopyViewModel>>(rentals);

            using var workbook = new XLWorkbook();

            var sheet = workbook.AddWorksheet("Delayed Rentals");

            sheet.AddLocalImage(_logoPath);

            var headerCells = new string[] { "Subscriber ID", "Subscriber Name", "Subscriber Phone", "Book Title",
                "Book Serial", "Rental Date", "End Date", "Extended On", "Delay in Days" };

            sheet.AddHeader(headerCells);

            for (int i = 0; i < data.Count; i++)
            {
                sheet.Cell(i + _sheetStartRow, 1).SetValue(data[i].Rental!.Subscriber!.Id);
                sheet.Cell(i + _sheetStartRow, 2).SetValue(data[i].Rental!.Subscriber!.FullName);
                sheet.Cell(i + _sheetStartRow, 3).SetValue(data[i].Rental!.Subscriber!.MobileNumber);
                sheet.Cell(i + _sheetStartRow, 4).SetValue(data[i].BookCopy!.BookTitle);
                sheet.Cell(i + _sheetStartRow, 5).SetValue(data[i].BookCopy!.SerialNumber);
                sheet.Cell(i + _sheetStartRow, 6).SetValue(data[i].RentalDate.ToString("d MMM, yyyy"));
                sheet.Cell(i + _sheetStartRow, 7).SetValue(data[i].EndDate.ToString("d MMM, yyyy"));
                sheet.Cell(i + _sheetStartRow, 8).SetValue(data[i].ExtendedOn is null ? "-" : rentals[i].ExtendedOn?.ToString("d MMM, yyyy"));
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

            var rentals = _context.RentalCopies
                           .Include(c => c.BookCopy)
                           .ThenInclude(r => r!.Book)
                           .Include(c => c.Rental)
                           .ThenInclude(c => c!.Subscriber)
                           .Where(c => !c.ReturnDate.HasValue && c.EndDate < DateTime.Today)
                           .ToList();

            var data = _mapper.Map<IEnumerable<RentalCopyViewModel>>(rentals);

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