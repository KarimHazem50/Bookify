namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BookCopiesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IBookCopyService _bookCopyService;

        public BookCopiesController(IMapper mapper, IBookCopyService bookCopyService)
        {
            _mapper = mapper;
            _bookCopyService = bookCopyService;
        }

        [AjaxOnly]
        public IActionResult Create(int bookId)
        {
            var book = _bookCopyService.GetBookById(bookId);

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

            var copy = _bookCopyService.Add(model.BookId, model.EditionNumber, model.IsAvailableForRental, User.GetUserId());

            if (copy is null)
                return NotFound();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);
            return PartialView("_BookCopyRow", viewModel);
        }

        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var copy = _bookCopyService.GetById(id);

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

            var copy = _bookCopyService.Update(model.Id, model.EditionNumber, model.IsAvailableForRental, User.GetUserId());

            if (copy is null)
                return NotFound();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);
            return PartialView("_BookCopyRow", viewModel);
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var copy = _bookCopyService.ToggleStatus(id, User.GetUserId());

            if (copy is null)
                return NotFound();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);
            return PartialView("_BookCopyRow", viewModel);
        }
        public IActionResult RentalHistory(int copyId)
        {
            var copyHistory = _bookCopyService.GetRentalHistory(copyId);

            var viewModel = _mapper.Map<IEnumerable<CopyHistoryViewModel>>(copyHistory);
            return View(viewModel);
        }
    }
}
