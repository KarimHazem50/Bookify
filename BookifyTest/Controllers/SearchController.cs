using HashidsNet;

namespace Bookify.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHashids _hashids;
        public SearchController(IApplicationDbContext context, IMapper mapper, IHashids hashids)
        {
            _context = context;
            _mapper = mapper;
            _hashids = hashids;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Find(string value)
        {
            var books = _context.Books.Include(b => b.Authors)
                                                .Where(b => !b.IsDeleted && (b.Title.Contains(value) || b.Authors!.Name.Contains(value)))
                                                .Select(b => new { b.Title, Author = b.Authors!.Name, key = _hashids.Encode(b.Id) })
                                                .ToList();
            return Ok(books);
        }
        public IActionResult Details(string bkey)
        {
            var id = _hashids.Decode(bkey)[0];
            var book = _context.Books.Include(b => b.Authors).Include(b => b.Categories).ThenInclude(c => c.Category).Include(b => b.BookCopies).SingleOrDefault(b => b.Id == id && !b.IsDeleted);

            if (book is null)
                return NotFound();

            var viewModel = _mapper.Map<BookViewModel>(book);

            return View(viewModel);
        }
    }
}
