using HashidsNet;
using System.Diagnostics;

namespace BookifyTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly IMapper _mapper;
        private readonly IHashids _hashids;
        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, IMapper mapper, IHashids hashids)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
            _hashids = hashids;
        }
        public IActionResult Index()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction(nameof(Index), "Dashboard");

            var newlyAddedBooks = _context.Books.Where(b => !b.IsDeleted).Include(b => b.Authors).OrderByDescending(b => b.Id).Take(10).ToList();
            var viewModel = _mapper.Map<IEnumerable<BookViewModel>>(newlyAddedBooks);
            foreach (BookViewModel bookViewModel in viewModel)
            {
                bookViewModel.Key = _hashids.Encode(bookViewModel.Id);
            }
            return View(viewModel);
        }     
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}