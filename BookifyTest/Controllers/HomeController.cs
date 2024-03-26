using HashidsNet;
using Microsoft.AspNetCore.WebUtilities;

namespace Bookify.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBookService _bookService;
        private readonly ILogger<HomeController> _logger;
        private readonly IMapper _mapper;
        private readonly IHashids _hashids;
        public HomeController(ILogger<HomeController> logger, IMapper mapper, IHashids hashids, IBookService bookService)
        {
            _logger = logger;
            _mapper = mapper;
            _hashids = hashids;
            _bookService = bookService;
        }
        public IActionResult Index()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction(nameof(Index), "Dashboard");

            var newlyAddedBooks = _bookService.GetLastAddedBooks(10);
            var viewModel = _mapper.Map<IEnumerable<BookViewModel>>(newlyAddedBooks);
            foreach (BookViewModel bookViewModel in viewModel)
            {
                bookViewModel.Key = _hashids.Encode(bookViewModel.Id);
            }
            return View(viewModel);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int id = 500)
        {
            return View(new ErrorViewModel { ErrorCode = id, ErrorDescription = ReasonPhrases.GetReasonPhrase(id) });
        }
    }
}