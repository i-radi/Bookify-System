using HashidsNet;

namespace Bookify.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IHashids _hashids;
        private readonly IBookService _bookService;

        public SearchController(IMapper mapper, IHashids hashids, IBookService bookService)
        {
            _mapper = mapper;
            _hashids = hashids;
            _bookService = bookService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Find(string query)
        {
            var books = _bookService.Search(query);

            var data = _mapper.ProjectTo<BookSearchResultViewModel>(books).ToList();

            data.ForEach(book =>
            {
                book.Key = _hashids.EncodeHex(book.Id.ToString());
            });

            return Ok(data);
        }

        public IActionResult Details(string bKey)
        {
            var bookId = _hashids.DecodeHex(bKey);

            var query = _bookService.GetDetails();

            var viewModel = _mapper.ProjectTo<BookViewModel>(query)
                .SingleOrDefault(b => b.Id == int.Parse(bookId));

            if (viewModel is null)
                return NotFound();

            return View(viewModel);
        }
    }
}