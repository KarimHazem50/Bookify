using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IImageService _imageService;

        public BooksController(IMapper mapper, IWebHostEnvironment webHostEnvironment, IImageService imageService, IBookService bookService)
        {
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _imageService = imageService;
            _bookService = bookService;
        }
        private BookFormViewModel PopulateData(BookFormViewModel? model = null)
        {
            var authors = _bookService.GetAuthors();
            var categories = _bookService.GetCategories();

            var viewModel = model is null ? new BookFormViewModel() : model;

            viewModel.DisplayAuthors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
            viewModel.DisplayCategories = _mapper.Map<IEnumerable<SelectListItem>>(categories);

            return viewModel;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost, IgnoreAntiforgeryToken]
        public IActionResult GetBooks()
        {
            var filteredDto = Request.Form.GetFilters();

            var (books, recordsTotal) = _bookService.GetFiltered(filteredDto);

            var mappedData = _mapper.ProjectTo<BookRowViewModel>(books).ToList();

            var jsonData = new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mappedData };

            return Ok(jsonData);
        }
        public IActionResult Details(int id)
        {
            var books = _bookService.GetDetails();

            var viewModel = _mapper.ProjectTo<BookViewModel>(books).SingleOrDefault(b => b.Id == id);

            if (viewModel is null)
                return NotFound();

            return View(viewModel);
        }
        public IActionResult Create()
        {
            return View("Form", PopulateData());
        }

        [HttpPost]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateData(model));

            var book = _mapper.Map<Book>(model);

            if (model.Image is not null)
            {
                var extension = Path.GetExtension(model.Image.FileName);
                var imageName = $"{Guid.NewGuid()}{extension}";

                var result = await _imageService.UploadAsync(model.Image, imageName, "/Images/books", hasThumbnail: true);

                if (result.isUploaded)
                {
                    book.ImageName = imageName;
                }
                else
                {
                    ModelState.AddModelError(nameof(model.Image), result.errorMessage!);
                    return View("Form", PopulateData(model));
                }
            }

            _bookService.Add(book, model.SelectedCategories, User.GetUserId());

            return RedirectToAction(nameof(Details), new { id = book.Id });
        }

        public IActionResult Edit(int id)
        {
            var book = _bookService.GetWithCategories(id);

            if (book is null)
                return NotFound();

            var model = _mapper.Map<BookFormViewModel>(book);
            var viewModel = PopulateData(model);

            viewModel.SelectedCategories = book.Categories.Select(c => c.CategoryId).ToList();

            return View("Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateData(model));

            var book = _bookService.GetWithCategoriesAndCopies(model.Id);

            if (book is null)
                return NotFound();

            if (model.Image is not null)
            {
                var extension = Path.GetExtension(model.Image.FileName);
                var imageName = $"{Guid.NewGuid()}{extension}";

                var result = await _imageService.UploadAsync(model.Image, imageName, "/Images/books", hasThumbnail: true);

                if (result.isUploaded)
                {
                    if (!string.IsNullOrEmpty(book.ImageName))
                    {
                        _imageService.Delete(book.ImageName, "/Images/books", HasThumbnailPath: true);
                    }
                    model.ImageName = imageName;
                }
                else
                {
                    ModelState.AddModelError(nameof(model.Image), result.errorMessage!);
                    if (!string.IsNullOrEmpty(book.ImageName))
                        model.ImageName = book.ImageName;
                    return View("Form", PopulateData(model));
                }
            }
            else if (!string.IsNullOrEmpty(book.ImageName) && model.Image is null)
            {
                model.ImageName = book.ImageName;
            }

            book = _mapper.Map(model, book);

            _bookService.Update(book, model.SelectedCategories, User.GetUserId());

            return RedirectToAction(nameof(Details), new { id = book.Id });
        }

        public IActionResult AllowedItem(BookFormViewModel model)
        {
            return Json(_bookService.AllowedItem(model.Title, model.Id, model.AuthorId));
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var book = _bookService.ToggleStatus(id, User.GetUserId());

            if (book is null)
                return NotFound();

            var viewModel = _mapper.Map<BookViewModel>(book);
            return Ok(viewModel);
        }
    }
}