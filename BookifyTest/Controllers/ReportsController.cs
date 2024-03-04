using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.Rendering;
using OpenHtmlToPdf;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Net.Mime;
using ViewToHTML.Services;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class ReportsController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;
        private readonly IViewRendererService _viewRendererService;

        private readonly string _logoPath;
        private readonly int _sheetStartRow = 5;
        public ReportsController(IApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IMapper mapper, IViewRendererService viewRendererService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
            _viewRendererService = viewRendererService;

            _logoPath = $"{_webHostEnvironment.WebRootPath}/assets/images/logo.png";
        }


        public IActionResult Index()
        {
            return View();
        }


        #region Books
        public IActionResult Books(int? pageNumber, IList<int> selectedAuthors, IList<int> selectedCategories)
        {
            var viewModel = new BooksReportViewModel
            {
                DisplayAuthors = _mapper.Map<IList<SelectListItem>>(_context.Authors.OrderBy(a => a.Name).ToList()),
                DisplayCategories = _mapper.Map<IList<SelectListItem>>(_context.Categories.OrderBy(c => c.Name).ToList()),
            };

            IQueryable<Book> books = _context.Books.Include(b => b.Authors).Include(b => b.Categories).ThenInclude(c => c.Category);

            if (selectedAuthors.Any())
                books = books.Where(b => selectedAuthors.Contains(b.AuthorId));

            if (selectedCategories.Any())
                books = books.Where(b => b.Categories.Any(c => selectedCategories.Contains(c.CategoryId)));


            if (pageNumber is not null)
                viewModel.Books = PaginatedList<Book>.Create(books, pageNumber ?? 0, pageSize: (int)ReportsConfigurations.PageSize);

            return View(viewModel);
        }
        public async Task<IActionResult> ExportsBooksToExcel(string authors, string categories)
        {
            IList<int> selectedAuthors = new List<int>();
            if (!string.IsNullOrEmpty(authors))
            {
                try
                {
                    selectedAuthors = authors.Split(",").Select(int.Parse).ToList();
                }
                catch { }
            }

            IList<int> selectedCategories = new List<int>();
            if (!string.IsNullOrEmpty(categories))
            {
                try
                {
                    selectedCategories = categories.Split(",").Select(int.Parse).ToList();
                }
                catch { }
            }


            IQueryable<Book> data = _context.Books.Include(b => b.Authors).Include(b => b.Categories).ThenInclude(c => c.Category);

            if (selectedAuthors.Any())
                data = data.Where(b => selectedAuthors.Contains(b.AuthorId));

            if (selectedCategories.Any())
                data = data.Where(b => b.Categories.Any(c => selectedCategories.Contains(c.CategoryId)));

            var books = data.ToList();

            using var workBook = new XLWorkbook();

            var sheet = workBook.AddWorksheet("Books");

            sheet.AddToLocalImage(_logoPath);

            var headerCells = new string[] { "Title", "Author", "Categories", "Publisher", "Publishing Date", "Hall", "Availble for rental", "Status" };

            sheet.AddHeader(headerCells);

            int row = _sheetStartRow;
            foreach (var book in books)
            {
                sheet.Cell(row, 1).SetValue(book.Title);
                sheet.Cell(row, 2).SetValue(book.Authors!.Name);
                sheet.Cell(row, 3).SetValue(string.Join(",", book.Categories.Select(b => b.Category!.Name)));
                sheet.Cell(row, 4).SetValue(book.Publisher);
                sheet.Cell(row, 5).SetValue(book.PublishingDate.ToString("dd MMM, yyyy"));
                sheet.Cell(row, 6).SetValue(book.Hall);
                sheet.Cell(row, 7).SetValue(book.IsAvailableForRental ? "Yes" : "No");
                sheet.Cell(row, 8).SetValue(book.IsDeleted ? "Deleted" : "Available");

                row++;
            }

            sheet.AddStyles();

            sheet.AddTable(books.Count(), headerCells.Length);

            await using var stream = new MemoryStream();

            workBook.SaveAs(stream);

            return File(stream.ToArray(), MediaTypeNames.Application.Octet, "Books.xlsx");
        }

        public async Task<IActionResult> ExportsBooksToPdf(string authors, string categories)
        {
            IList<int> selectedAuthors = new List<int>();
            if (!string.IsNullOrEmpty(authors))
            {
                try
                {
                    selectedAuthors = authors.Split(",").Select(int.Parse).ToList();
                }
                catch { }
            }

            IList<int> selectedCategories = new List<int>();
            if (!string.IsNullOrEmpty(categories))
            {
                try
                {
                    selectedCategories = categories.Split(",").Select(int.Parse).ToList();
                }
                catch { }
            }

            IQueryable<Book> books = _context.Books.Include(b => b.Authors).Include(b => b.Categories).ThenInclude(c => c.Category);

            if (selectedAuthors.Any())
                books = books.Where(b => selectedAuthors.Contains(b.AuthorId));

            if (selectedCategories.Any())
                books = books.Where(b => b.Categories.Any(c => selectedCategories.Contains(c.CategoryId)));



            var viewModel = _mapper.Map<IEnumerable<BookViewModel>>(books.ToList());

            var templatePath = "~/Views/Reports/BooksTemplate.cshtml";
            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templatePath, viewModel);

            var pdf = Pdf.From(html).EncodedWith("Utf-8").Content();

            return File(pdf.ToArray(), MediaTypeNames.Application.Octet, "Books.pdf");
        }
        #endregion


        #region Rentals
        public IActionResult Rentals(int pageNumber, string? duration)
        {
            var viewModel = new RentalsReportViewModel();
            if (duration is not null)
            {
                var result = GenerateQuery(duration);
                if (!string.IsNullOrEmpty(result.errorMessage))
                {
                    ModelState.AddModelError($"Duration", result.errorMessage);
                    return View(viewModel);
                }

                viewModel.Rentals = PaginatedList<RentalCopy>.Create(result.rentals!, pageNumber, (int)ReportsConfigurations.PageSize);
            }


            return View(viewModel);
        }

        public async Task<IActionResult> ExportRentalsToExcel(string duration)
        {
            var result = GenerateQuery(duration);

            if (!string.IsNullOrEmpty(result.errorMessage))
            {
                var viewModel = new RentalsReportViewModel();
                ModelState.AddModelError($"Duration", result.errorMessage);
                return View("Rentals", viewModel);
            }

            using var workBook = new XLWorkbook();

            var sheet = workBook.AddWorksheet("Rentals");

            sheet.AddToLocalImage(_logoPath);

            var headerCells = new string[] { "Subscriber ID", "Subscriber Name", "Subscriber Phone", "Book Title", "Book Author", "Rental Date", "End Date", "Return Date", "Extended On" };

            sheet.AddHeader(headerCells);

            var row = _sheetStartRow;
            foreach (var rental in result.rentals!)
            {
                sheet.Cell(row, 1).SetValue(rental.Rental!.SubScriberId);
                sheet.Cell(row, 2).SetValue($"{rental.Rental.Subscriber!.FirstName} {rental.Rental.Subscriber.LastName}");
                sheet.Cell(row, 3).SetValue(rental.Rental.Subscriber.MobileNumber);
                sheet.Cell(row, 4).SetValue(rental.BookCopy!.Book!.Title);
                sheet.Cell(row, 5).SetValue(rental.BookCopy.Book.Authors!.Name);
                sheet.Cell(row, 6).SetValue(rental.RentalDate.ToString("dd MMM, yyyy"));
                sheet.Cell(row, 7).SetValue(rental.EndDate.ToString("dd MMM, yyyy"));
                sheet.Cell(row, 8).SetValue(rental.ReturnDate.HasValue ? rental.ReturnDate?.ToString("dd MMM, yyyy") : "-");
                sheet.Cell(row, 9).SetValue(rental.ExtendedOn.HasValue ? rental.ExtendedOn?.ToString("dd MMM, yyyy") : "-");

                row++;
            }

            sheet.AddStyles();

            sheet.AddTable(result.rentals.Count(), headerCells.Length);

            await using var stream = new MemoryStream();
            workBook.SaveAs(stream);

            return File(stream.ToArray(), MediaTypeNames.Application.Octet, "Rentals.xlsx");
        }

        public async Task<IActionResult> ExportRentalsToPDF(string duration)
        {
            var result = GenerateQuery(duration);

            if (!string.IsNullOrEmpty(result.errorMessage))
            {
                var viewModel = new RentalsReportViewModel();
                ModelState.AddModelError($"Duration", result.errorMessage);
                return View("Rentals", viewModel);
            }

            var templatePath = "~/Views/Reports/RentalsTemplate.cshtml";
            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templatePath, result.rentals!.ToList());
            var pdf = Pdf.From(html).EncodedWith("Utf-8").Content();

            return File(pdf.ToArray(), MediaTypeNames.Application.Octet, "Rentals.pdf");
        }


        private (IQueryable<RentalCopy>? rentals, string? errorMessage) GenerateQuery(string duration)
        {
            DateTime startDate;
            DateTime endDate;
            var dateParts = duration.Split(" - ");
            try
            {
                startDate = DateTime.ParseExact(dateParts[0], "MM/dd/yyyy", CultureInfo.InvariantCulture);
            }
            catch
            {
                return (null, errorMessage: "Invalid start date");
            }
            try
            {
                endDate = DateTime.ParseExact(dateParts[1], "MM/dd/yyyy", CultureInfo.InvariantCulture);
            }
            catch
            {
                return (null, errorMessage: "Invalid end date");
            }

            var rentals = _context.RentalCopies.Where(r => r.RentalDate >= startDate && r.RentalDate <= endDate)
                    .Include(r => r.Rental).ThenInclude(r => r!.Subscriber).Include(r => r.BookCopy).ThenInclude(bc => bc!.Book).ThenInclude(b => b!.Authors)
                    .OrderBy(r => r.RentalDate);

            return (rentals, errorMessage: null);
        }
        #endregion


        #region Delayed Rentals
        public IActionResult DelayedRentals(int? pageNumber)
        {
            IQueryable<RentalCopy> delayedRentals = _context.RentalCopies.Where(r => r.EndDate < DateTime.Now && r.ReturnDate == null)
                .Include(r => r.Rental).ThenInclude(r => r!.Subscriber)
                .Include(r => r.BookCopy).ThenInclude(bc => bc!.Book).ThenInclude(b => b!.Authors)
                .OrderBy(r => r.Rental!.SubScriberId);

            var viewModel = new DelayedRentalsReportViewModel();

            viewModel.DelayedRentals = PaginatedList<RentalCopy>.Create(delayedRentals, pageNumber ?? 1, (int)ReportsConfigurations.PageSize);

            return View(viewModel);
        }

        public async Task<IActionResult> ExportDelayedRentalsToExcel()
        {
            IQueryable<RentalCopy> delayedRentals = _context.RentalCopies.Where(r => r.EndDate < DateTime.Now && r.ReturnDate == null)
                .Include(r => r.Rental).ThenInclude(r => r!.Subscriber)
                .Include(r => r.BookCopy).ThenInclude(bc => bc!.Book).ThenInclude(b => b!.Authors);

            using var workBook = new XLWorkbook();

            var sheet = workBook.AddWorksheet("Delayed Rentals");

            sheet.AddToLocalImage(_logoPath);

            var headerCells = new string[] { "Subscriber ID", "Subscriber Name", "Subscriber Phone", "Book Title", "Book Author", "Book Serial", "Rental Date", "End Date", "Extended On", "Delay In Days" };

            sheet.AddHeader(headerCells);

            var row = _sheetStartRow;
            foreach (var rental in delayedRentals)
            {
                sheet.Cell(row, 1).SetValue(rental.Rental!.SubScriberId);
                sheet.Cell(row, 2).SetValue($"{rental.Rental.Subscriber!.FirstName} {rental.Rental.Subscriber.LastName}");
                sheet.Cell(row, 3).SetValue(rental.Rental.Subscriber.MobileNumber);
                sheet.Cell(row, 4).SetValue(rental.BookCopy!.Book!.Title);
                sheet.Cell(row, 5).SetValue(rental.BookCopy.Book.Authors!.Name);
                sheet.Cell(row, 6).SetValue(rental.BookCopy.SerialNumber);
                sheet.Cell(row, 7).SetValue(rental.RentalDate.ToString("dd MMM, yyyy"));
                sheet.Cell(row, 8).SetValue(rental.EndDate.ToString("dd MMM, yyyy"));
                sheet.Cell(row, 9).SetValue(rental.ExtendedOn.HasValue ? rental.ExtendedOn?.ToString("dd MMM, yyyy") : "-");
                sheet.Cell(row, 10).SetValue((int)(DateTime.Today - rental.EndDate).TotalDays);

                row++;
            }

            sheet.AddStyles();

            sheet.AddTable(delayedRentals.Count(), headerCells.Length);

            await using var stream = new MemoryStream();
            workBook.SaveAs(stream);

            return File(stream.ToArray(), MediaTypeNames.Application.Octet, "DelayedRentals.xlsx");
        }


        public async Task<IActionResult> ExportDelayedRentalsToPdf()
        {
            IQueryable<RentalCopy> delayedRentals = _context.RentalCopies.Where(r => r.EndDate < DateTime.Now && r.ReturnDate == null)
             .Include(r => r.Rental).ThenInclude(r => r!.Subscriber)
             .Include(r => r.BookCopy).ThenInclude(bc => bc!.Book).ThenInclude(b => b!.Authors);


            var templatePath = "~/Views/Reports/DelayedRentalsTemplate.cshtml";
            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templatePath, delayedRentals.ToList());

            var pdf = Pdf.From(html).EncodedWith("Utf-8").Content();

            return File(pdf.ToArray(), MediaTypeNames.Application.Octet, "DelayedRentals.pdf");
        }
        #endregion

    }
}
