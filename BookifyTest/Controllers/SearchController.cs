using HashidsNet;

namespace Bookify.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IMapper _mapper;
        private readonly IHashids _hashids;
        public SearchController(IBookService bookService, IMapper mapper, IHashids hashids)
        {
            _bookService = bookService;
            _mapper = mapper;
            _hashids = hashids;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Find(string value)
        {
            var books = _bookService.Search(value);

            var data = _mapper.ProjectTo<BookSearchResultViewModel>(books).ToList();

            foreach (var book in data)
            {
                book.Key = _hashids.Encode(book.Id);
            }

            return Ok(data);
        }
        public IActionResult Details(string bkey)
        {
            var id = _hashids.Decode(bkey)[0];
            var query = _bookService.GetDetails();

            var viewModel = _mapper.ProjectTo<BookViewModel>(query).SingleOrDefault(b => b.Id == id && !b.IsDeleted);

            if (viewModel is null)
                return NotFound();

            return View(viewModel);
        }
    }
}
