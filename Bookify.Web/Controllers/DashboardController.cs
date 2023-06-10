namespace Bookify.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IBookService _bookService;
        private readonly ISubscriberService _subscriberService;
        private readonly IRentalService _rentalService;

        public DashboardController(IMapper mapper,
                IBookService bookService,
                ISubscriberService subscriberService,
                IRentalService rentalService)
        {
            _mapper = mapper;
            _bookService = bookService;
            _subscriberService = subscriberService;
            _rentalService = rentalService;
        }


        public IActionResult Index()
        {
            var numberOfCopies = _bookService.GetActiveBooksCount();

            numberOfCopies = numberOfCopies <= 10 ? numberOfCopies : numberOfCopies / 10 * 10;

            var numberOfsubscribers = _subscriberService.GetActiveSubscribersCount();
            var lastAddedBooks = _bookService.GetLastAddedBooks(8);
            var topBooks = _bookService.GetTopBooks(6);

            var viewModel = new DashboardViewModel
            {
                NumberOfCopies = numberOfCopies,
                NumberOfSubscribers = numberOfsubscribers,
                LastAddedBooks = _mapper.Map<IEnumerable<BookViewModel>>(lastAddedBooks),
                TopBooks = _mapper.Map<IEnumerable<BookViewModel>>(topBooks)
            };

            return View(viewModel);
        }

        [AjaxOnly]
        public IActionResult GetRentalsPerDay(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.Today.AddDays(-29);
            endDate ??= DateTime.Today;

            var data = _rentalService.GetRentalsPerDay(startDate, endDate);

            return Ok(_mapper.Map<IEnumerable<ChartItemViewModel>>(data));
        }

        [AjaxOnly]
        public IActionResult GetSubscribersPerCity()
        {
            var data = _subscriberService.GetSubscribersPerCity();

            return Ok(_mapper.Map<IEnumerable<ChartItemViewModel>>(data));
        }
    }
}