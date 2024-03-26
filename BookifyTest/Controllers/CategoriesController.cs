namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class CategoriesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ICategoryService _categoryService;

        public CategoriesController(IMapper mapper, ICategoryService categoryService)
        {
            _mapper = mapper;
            _categoryService = categoryService;
        }

        public IActionResult Index()
        {
            var categories = _categoryService.GetAllWithNoTracking();

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

            _categoryService.Add(category, User.GetUserId());

            var viewModel = _mapper.Map<CategoryViewModel>(category);
            return PartialView("_CategoryRow", viewModel);
        }

        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var category = _categoryService.GetById(id);

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

            var category = _categoryService.Update(model.Id, User.GetUserId());

            if (category is null)
                return NotFound();

            _mapper.Map(model, category);

            var viewModel = _mapper.Map<CategoryViewModel>(category);
            return PartialView("_CategoryRow", viewModel);
        }
        public IActionResult AllowedItems(CategoryFormViewModel model)
        {
            return Json(_categoryService.AllowedItem(model.Id, model.Name));
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var category = _categoryService.ToggleStatus(id, User.GetUserId());
            var viewModel = _mapper.Map<CategoryViewModel>(category);
            return PartialView("_CategoryRow", viewModel);
        }
    }
}