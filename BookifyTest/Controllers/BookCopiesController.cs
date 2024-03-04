namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BookCopiesController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BookCopiesController(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [AjaxOnly]
        public IActionResult Create(int bookId)
        {
            var book = _context.Books.Find(bookId);
            if (book is null)
                return NotFound();
            var viewModel = new BookCopyFormViewModel
            {
                ShowRentalInput = book.IsAvailableForRental
            };
            return PartialView("Form", viewModel);
        }
        [HttpPost]
        public IActionResult Create(BookCopyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var book = _context.Books.Find(model.BookId);
            if (book is null)
                return NotFound();

            var copy = new BookCopy
            {
                EditionNumber = model.EditionNumber,
                IsAvailableForRental = book.IsAvailableForRental ? model.IsAvailableForRental : false,
                CreatedById = User.GetUserId()
            };
            book.BookCopies.Add(copy);
            _context.SaveChanges();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);
            return PartialView("_BookCopyRow", viewModel);
        }

        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var copy = _context.BookCopies.Include(c => c.Book).SingleOrDefault(c => c.Id == id);
            if (copy is null)
                return NotFound();

            var viewModel = _mapper.Map<BookCopyFormViewModel>(copy);
            return PartialView("Form", viewModel);
        }

        [HttpPost]
        public IActionResult Edit(BookCopyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var copy = _context.BookCopies.Include(c => c.Book).SingleOrDefault(c => c.Id == model.Id);
            if (copy is null)
                return NotFound();

            copy.EditionNumber = model.EditionNumber;
            copy.IsAvailableForRental = copy.Book!.IsAvailableForRental ? model.IsAvailableForRental : false;
            copy.LastUpdatedOn = DateTime.Now;
            copy.LastUpdatedById = User.GetUserId();
            _context.SaveChanges();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);
            return PartialView("_BookCopyRow", viewModel);
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var copy = _context.BookCopies.Include(c => c.Book).SingleOrDefault(c => c.Id == id);
            if (copy is null)
                return NotFound();

            copy.IsDeleted = !copy.IsDeleted;
            copy.LastUpdatedOn = DateTime.Now;
            copy.LastUpdatedById = User.GetUserId();
            _context.SaveChanges();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);
            return PartialView("_BookCopyRow", viewModel);
        }
        public IActionResult RentalHistory(int copyId)
        {
            var copyHistory = _context.RentalCopies.Include(c => c.Rental).ThenInclude(r => r!.Subscriber).Where(c => c.BookCopyId == copyId).OrderByDescending(c => c.RentalDate).ToList();

            var viewModel = _mapper.Map<IEnumerable<CopyHistoryViewModel>>(copyHistory);
            return View(viewModel);
        }
    }
}
