namespace Bookify.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IBookService _bookService;
        private readonly ISubscriberService _subscriberService;
        private readonly IRentalService _rentalService;
        private readonly IMapper _mapper;

        public DashboardController(IBookService bookService, ISubscriberService subscriberService, IRentalService rentalService, IMapper mapper)
        {
            _bookService = bookService;
            _subscriberService = subscriberService;
            _rentalService = rentalService;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            var numberOfCopies = _bookService.GetActiveBooksCount();
            numberOfCopies = numberOfCopies <= 10 ? numberOfCopies : numberOfCopies / 10 * 10;

            var numberOfSubscribers = _subscriberService.GetActiveSubscribersCount();

            var lastAddedBooks = _bookService.GetLastAddedBooks(8);

            var topBooks = _bookService.GetTopBooks(6);

            var viewModel = new DashboardViewModel
            {
                NumberOfCopies = numberOfCopies,
                NumberOfSubscribers = numberOfSubscribers,
                LastAddBooks = _mapper.Map<IEnumerable<BookViewModel>>(lastAddedBooks),
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
        public IActionResult GetSubscribersByCity()
        {
            var data = _subscriberService.GetSubscribersPerCity();

            return Ok(_mapper.Map<IEnumerable<ChartItemViewModel>>(data));
        }
    }
}