namespace Bookify.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DashboardController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            var numberOfCopies = _context.BookCopies.Count(c => !c.IsDeleted);
            numberOfCopies = numberOfCopies <= 10 ? numberOfCopies : numberOfCopies / 10 * 10;

            var numberOfSubscribers = _context.Subscribers.Count(s => !s.IsDeleted);

            var lastAddBooks = _context.Books.Where(b => !b.IsDeleted).Include(b => b.Authors).OrderByDescending(b => b.Id).Take(8).ToList();

            var topBooks = _context.RentalCopies.Include(rc => rc.BookCopy).ThenInclude(c => c!.Book).ThenInclude(b => b!.Authors)
                                            .GroupBy(rc => new
                                            {
                                                rc.BookCopy!.BookId,
                                                rc.BookCopy.Book!.Title,
                                                rc.BookCopy.Book.ImageName,
                                                AuthorName = rc.BookCopy.Book.Authors!.Name
                                            })
                                            .Select(b => new
                                            {
                                                b.Key.BookId,
                                                b.Key.Title,
                                                b.Key.ImageName,
                                                b.Key.AuthorName,
                                                Count = b.Count()
                                            })
                                            .OrderByDescending(b => b.Count)
                                            .Take(6)
                                            .Select(b => new BookViewModel
                                            {
                                                Id = b.BookId,
                                                Title = b.Title,
                                                ImageName = b.ImageName,
                                                AuthorName = b.AuthorName
                                            })
                                            .ToList();

            var viewModel = new DashboardViewModel
            {
                NumberOfCopies = numberOfCopies,
                NumberOfSubscribers = numberOfSubscribers,
                LastAddBooks = _mapper.Map<IEnumerable<BookViewModel>>(lastAddBooks),
                TopBooks = topBooks
            };
            return View(viewModel);
        }


        [AjaxOnly]
        public IActionResult GetRentalsPerDay(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.Today.AddDays(-29);
            endDate ??= DateTime.Today;

            var data = _context.RentalCopies.Where(rc => rc.RentalDate >= startDate && rc.RentalDate <= endDate)
                                    .GroupBy(rc => new
                                    {
                                        rc.RentalDate,
                                    })
                                    .Select(g => new ChartItemViewModel
                                    {
                                        Label = g.Key.RentalDate.ToString("d MMM"),
                                        Value = g.Count().ToString(),
                                    })
                                    .ToList();

            for (var day = startDate; day <= endDate; day = day.Value.AddDays(1))
            {
                if (!data.Any(d => d.Label == day.Value.ToString("d MMM")))
                {
                    data.Add(new ChartItemViewModel
                    {
                        Label = day.Value.ToString("d MMM"),
                        Value = "0",
                    });
                }
            }


            var orderedData = data.OrderBy(d => int.Parse(d.Label!.Split(" ")[0]));
            return Ok(orderedData);
        }

        [AjaxOnly]
        public IActionResult GetSubscribersByCity()
        {
            var data = _context.Subscribers.Where(s => !s.IsDeleted).Include(s => s.Governorate)
                                          .GroupBy(s => new
                                          {
                                              City = s.Governorate!.Name
                                          })
                                          .Select(g => new ChartItemViewModel
                                          {
                                              Label = g.Key.City,
                                              Value = g.Count().ToString()
                                          })
                                          .ToList();

            return Ok(data);
        }
    }
}
