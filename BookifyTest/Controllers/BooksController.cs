using Microsoft.AspNetCore.Mvc.Rendering;
using SixLabors.ImageSharp;
using System.Linq.Dynamic.Core;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IImageService _imageService;

        public BooksController(ApplicationDbContext context, IMapper mapper, IWebHostEnvironment webHostEnvironment, IImageService imageService)
        {
            _context = context;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _imageService = imageService;
        }
        private BookFormViewModel PopulateData(BookFormViewModel? model = null)
        {
            var authors = _context.Authors.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();
            var categories = _context.Categories.Where(a => !a.IsDeleted).OrderBy(c => c.Name).ToList();

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
            var start = int.Parse(Request.Form["start"]);
            var pageSize = int.Parse(Request.Form["length"]);

            var sortColumnIndex = Request.Form["order[0][column]"];
            var sortColumnDirection = Request.Form["order[0][dir]"];
            var sortColumnName = Request.Form[$"columns[{sortColumnIndex}][name]"];

            var searchValue = Request.Form["search[value]"];

            IQueryable<Book> books = _context.Books.Include(b => b.Authors).Include(b => b.Categories).ThenInclude(c => c.Category);
            if (!string.IsNullOrEmpty(searchValue))
                books = books.Where(b => b.Title.Contains(searchValue) || b.Authors!.Name.Contains(searchValue));
            books = books.OrderBy($"{sortColumnName} {sortColumnDirection}");

            int recordsTotal = books.Count();
            var data = books.Skip(start).Take(pageSize).ToList();
            var mappedData = _mapper.Map<IEnumerable<BookViewModel>>(data);

            var jsonData = new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mappedData };
            return Ok(jsonData);
        }
        public IActionResult Details(int id)
        {
            var book = _context.Books.Include(b => b.Authors).Include(b => b.BookCopies).Include(b => b.Categories).ThenInclude(c => c.Category).SingleOrDefault(b => b.Id == id);
            if (book is null)
                return NotFound();
            var viewModel = _mapper.Map<BookViewModel>(book);
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
            foreach (var category in model.SelectedCategories)
            {
                book.Categories.Add(new BookCategory { CategoryId = category });
            }
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
            book.CreatedById = User.GetUserId();
            _context.Books.Add(book);
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = book.Id });
        }
        public IActionResult Edit(int id)
        {
            var book = _context.Books.Include(b => b.Categories).SingleOrDefault(b => b.Id == id);
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

            var book = _context.Books.Include(b => b.Categories).Include(b => b.BookCopies).SingleOrDefault(b => b.Id == model.Id);
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
            book.LastUpdatedOn = DateTime.Now;
            book.LastUpdatedById = User.GetUserId();

            book.Categories.Clear();
            foreach (var category in model.SelectedCategories)
            {
                book.Categories.Add(new BookCategory { CategoryId = category });
            }

            if (!model.IsAvailableForRental)
            {
                foreach (var copy in book.BookCopies)
                {
                    copy.IsAvailableForRental = false;
                }
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Details), new { id = book.Id });
        }
        public IActionResult AllowedItem(BookFormViewModel model)
        {
            var book = _context.Books.SingleOrDefault(b => b.Title == model.Title && b.AuthorId == model.AuthorId);
            var isAllowed = book is null || book.Id.Equals(model.Id);
            return Json(isAllowed);
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var book = _context.Books.Find(id);
            if (book is null)
                return NotFound();
            book.IsDeleted = !book.IsDeleted;
            book.LastUpdatedOn = DateTime.Now;
            book.LastUpdatedById = User.GetUserId();
            _context.SaveChanges();

            var viewModel = _mapper.Map<BookViewModel>(book);
            return Ok(viewModel);
        }
    }
}
