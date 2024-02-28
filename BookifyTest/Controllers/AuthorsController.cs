namespace BookifyTest.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class AuthorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AuthorsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            var authors = _context.Authors.AsNoTracking().ToList();
            var viewModel = _mapper.Map<IEnumerable<AuthorViewModel>>(authors);
            return View(viewModel);
        }
        [AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }
        [HttpPost]
        public IActionResult Create(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var author = _mapper.Map<Author>(model);
            author.CreatedById = User.GetUserId();
            _context.Authors.Add(author);
            _context.SaveChanges();

            var viewModel = _mapper.Map<AuthorViewModel>(author);
            return PartialView("_AuthorRow", viewModel);
        }
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var author = _context.Authors.Find(id);
            if (author is null)
                return NotFound();
            var viewModel = _mapper.Map<AuthorFormViewModel>(author);
            return PartialView("_Form", viewModel);
        }
        [HttpPost]
        public IActionResult Edit(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var author = _context.Authors.Find(model.Id);
            if (author is null)
                return NotFound();

            _mapper.Map(model, author);
            author.LastUpdatedOn = DateTime.Now;
            author.LastUpdatedById = User.GetUserId();
            _context.SaveChanges();
            var viewModel = _mapper.Map<AuthorViewModel>(author);
            return PartialView("_AuthorRow", viewModel);
        }
        public IActionResult AllowedItem(AuthorFormViewModel model)
        {
            var author = _context.Authors.FirstOrDefault(a => a.Name == model.Name);
            var IsAllowed = author is null || author.Id.Equals(model.Id);
            return Json(IsAllowed);
        }
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var author = _context.Authors.Find(id);
            if (author is null)
                return NotFound();

            author.IsDeleted = !author.IsDeleted;
            author.LastUpdatedOn = DateTime.Now;
            author.LastUpdatedById = User.GetUserId();
            _context.SaveChanges();

            var viewModel = _mapper.Map<AuthorViewModel>(author);
            return PartialView("_AuthorRow", viewModel);
        }
    }
}
