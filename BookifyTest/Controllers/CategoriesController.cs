namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class CategoriesController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CategoriesController(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            var categories = _context.Categories.AsNoTracking().ToList();

            var viewModel = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);

            return View(viewModel);
        }
        [AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }
        [HttpPost]
        public IActionResult Create(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var category = _mapper.Map<Category>(model);
            category.CreatedById = User.GetUserId();
            _context.Categories.Add(category);
            _context.SaveChanges();

            var viewModel = _mapper.Map<CategoryViewModel>(category);
            return PartialView("_CategoryRow", viewModel);
        }
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);
            if (category is null)
                return NotFound();

            var viewModel = _mapper.Map<CategoryFormViewModel>(category);
            return PartialView("_Form", viewModel);
        }
        [HttpPost]
        public IActionResult Edit(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var category = _context.Categories.Find(model.Id);
            if (category is null)
                return NotFound();
            _mapper.Map(model, category);
            category.LastUpdatedOn = DateTime.Now;
            category.LastUpdatedById = User.GetUserId();
            _context.SaveChanges();
            var viewModel = _mapper.Map<CategoryViewModel>(category);
            return PartialView("_CategoryRow", viewModel);
        }
        public IActionResult AllowedItems(CategoryFormViewModel model)
        {
            var category = _context.Categories.FirstOrDefault(c => c.Name == model.Name);
            var IsAllowed = category is null || category.Id.Equals(model.Id);
            return Json(IsAllowed);
        }
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var category = _context.Categories.Find(id);
            if (category is null)
                return NotFound();
            category.IsDeleted = !category.IsDeleted;
            category.LastUpdatedOn = DateTime.Now;
            category.LastUpdatedById = User.GetUserId();
            _context.SaveChanges();
            var viewModel = _mapper.Map<CategoryViewModel>(category);
            return PartialView("_CategoryRow", viewModel);
        }
    }
}
