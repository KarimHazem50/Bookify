using Microsoft.AspNetCore.DataProtection;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class RentalsController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataProtector _dataProtector;
        private readonly IMapper _mapper;

        public RentalsController(IApplicationDbContext context, IDataProtectionProvider dataProtector, IMapper mapper)
        {
            _context = context;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
            _mapper = mapper;
        }

        public IActionResult Create(string sKey)
        {
            var id = int.Parse(_dataProtector.Unprotect(sKey));
            var subscriber = _context.Subscribers.Include(s => s.Subscriptions).Include(s => s.Rentals).ThenInclude(r => r.RentalCopies).SingleOrDefault(s => s.Id == id);

            if (subscriber is null)
                return NotFound();

            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber);
            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var viewModel = new RentalFormViewModel
            {
                SubscriberKey = sKey,
                MaxAllowedCopies = maxAllowedCopies
            };
            return View("Form", viewModel);
        }

        [HttpPost]
        public IActionResult Create(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            var (subscriber, copies, errorMessage) = GenrateCopies(model);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                return View("NotAllowedRental", errorMessage);
            }


            var rental = new Rental()
            {
                RentalCopies = copies!,
                CreatedById = User.GetUserId(),
            };

            subscriber!.Rentals.Add(rental);
            _context.SaveChanges();
            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }


        public IActionResult Edit(int id)
        {
            var rental = _context.Rentals.Include(r => r.RentalCopies).ThenInclude(rc => rc.BookCopy).ThenInclude(bc => bc!.Book).SingleOrDefault(r => r.Id == id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            var subscriber = _context.Subscribers.Include(s => s!.Subscriptions).Include(s => s.Rentals).ThenInclude(r => r.RentalCopies).SingleOrDefault(s => s.Id == rental.SubScriberId);

            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber!, rental.Id);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);


            var currentCopies = rental.RentalCopies.Select(rc => rc.BookCopy);

            var viewModel = new RentalFormViewModel
            {
                SubscriberKey = _dataProtector.Protect(rental.SubScriberId.ToString()),
                CurrentCopies = _mapper.Map<IEnumerable<BookCopyViewModel>>(currentCopies),
                MaxAllowedCopies = maxAllowedCopies
            };

            return View("Form", viewModel);
        }



        [HttpPost]
        public IActionResult Edit(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            var rental = _context.Rentals.Include(r => r.RentalCopies).SingleOrDefault(r => r.Id == model.Id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();


            var (subscriber, copies, errorMessage) = GenrateCopies(model, rental.Id);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                return View("NotAllowedRental", errorMessage);
            }

            rental.RentalCopies = copies!;
            rental.LastUpdatedById = User.GetUserId();
            rental.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();
            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }


        public IActionResult Return(int id)
        {
            var rental = _context.Rentals.Include(r => r.RentalCopies).ThenInclude(rc => rc.BookCopy).ThenInclude(bc => bc!.Book).SingleOrDefault(r => r.Id == id);

            if (rental is null || rental.CreatedOn.Date == DateTime.Today)
                return NotFound();

            var subscriber = _context.Subscribers.Include(s => s.Subscriptions).SingleOrDefault(s => s.Id == rental.SubScriberId);

            var viewModel = new RentalReturnFormViewModel()
            {
                Id = id,
                Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies.Where(rc => !rc.ReturnDate.HasValue)),
                SelectedCopies = rental.RentalCopies.Where(rc => !rc.ReturnDate.HasValue).Select(c => new ReturnCopyViewModel { Id = c.BookCopyId, IsReturned = (c.ExtendedOn.HasValue ? false : null) }).ToList(),
                AllowExtend = !subscriber!.IsBlackListed && subscriber.Subscriptions.Last().EndDate >= rental.StartDate.AddDays((int)RentalsConfigurations.RentalDuration * 2) && rental.StartDate.AddDays((int)RentalsConfigurations.RentalDuration) >= DateTime.Today,
            };

            return View(viewModel);
        }



        [HttpPost]
        public IActionResult Return(RentalReturnFormViewModel model)
        {
            var rental = _context.Rentals.Include(r => r.RentalCopies).ThenInclude(rc => rc.BookCopy).ThenInclude(bc => bc!.Book).SingleOrDefault(r => r.Id == model.Id);

            if (rental is null || rental.CreatedOn.Date == DateTime.Today)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies.Where(rc => !rc.ReturnDate.HasValue));
                return View(model);
            }

            var subscriber = _context.Subscribers.Include(s => s.Subscriptions).SingleOrDefault(s => s.Id == rental.SubScriberId);

            if (model.SelectedCopies.Any(c => c.IsReturned.HasValue && !c.IsReturned.Value))
            {
                string error = string.Empty;

                if (subscriber!.IsBlackListed)
                    error = Errors.RentalNotAllowedForBlackListed;
                else if (subscriber.Subscriptions.Last().EndDate < rental.StartDate.AddDays((int)RentalsConfigurations.RentalDuration * 2))
                    error = Errors.RentalNotAllowedForInactive;
                else if (rental.StartDate.AddDays((int)RentalsConfigurations.RentalDuration) < DateTime.Today)
                    error = Errors.ExtendNotAllowed;

                if (!string.IsNullOrEmpty(error))
                {
                    model.Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies.Where(rc => !rc.ReturnDate.HasValue));
                    ModelState.AddModelError("", error);
                    return View(model);
                }
            }


            var isUpdated = false;

            foreach (var copy in model.SelectedCopies)
            {
                if (!copy.IsReturned.HasValue)
                    continue;

                var currentCopy = rental.RentalCopies.SingleOrDefault(c => c.BookCopyId == copy.Id);

                if (currentCopy is null)
                    continue;

                if (copy.IsReturned.HasValue && copy.IsReturned.Value)
                {
                    if (currentCopy.ReturnDate.HasValue)
                        continue;

                    currentCopy.ReturnDate = DateTime.Now;
                    isUpdated = true;
                }
                if (copy.IsReturned.HasValue && !copy.IsReturned.Value)
                {
                    if (currentCopy.ExtendedOn.HasValue)
                        continue;

                    currentCopy.ExtendedOn = DateTime.Now;
                    currentCopy.EndDate = currentCopy.RentalDate.AddDays((int)RentalsConfigurations.RentalDuration * 2);
                    isUpdated = true;
                }
            }

            if (isUpdated)
            {
                rental.LastUpdatedOn = DateTime.Now;
                rental.LastUpdatedById = User.GetUserId();
                rental.PenaltyPaid = model.PenaltyPaid;
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }


        [HttpPost]
        public IActionResult GetCopyDetails(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var copy = _context.BookCopies.Include(c => c.Book).SingleOrDefault(c => c.SerialNumber.ToString() == model.Value && !c.IsDeleted && !c.Book!.IsDeleted);

            if (copy is null)
                return NotFound(Errors.InvalidSerialNumber);

            if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                return BadRequest(Errors.NotAvailableForRental);


            //check that copy is not in rental
            var copyIsInRental = _context.RentalCopies.Any(c => c.BookCopyId == copy.Id && !c.ReturnDate.HasValue);
            if (copyIsInRental)
                return BadRequest(Errors.CopyIsInRental);

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);
            return PartialView("_CopyDetails", viewModel);
        }



        [HttpPost]
        public IActionResult MarkedAsDeleted(int id)
        {
            var rental = _context.Rentals.Include(r => r.RentalCopies).SingleOrDefault(r => r.Id == id);
            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            rental.IsDeleted = true;
            rental.LastUpdatedOn = DateTime.Now;
            rental.LastUpdatedById = User.GetUserId();

            _context.SaveChanges();

            return Ok();
        }


        public IActionResult Details(int id)
        {
            var rental = _context.Rentals.Include(r => r.RentalCopies).ThenInclude(rc => rc.BookCopy).ThenInclude(bc => bc!.Book).SingleOrDefault(r => r.Id == id);
            if (rental is null)
                return NotFound();

            var viewModel = _mapper.Map<RentalViewModel>(rental);
            return View(viewModel);
        }



        private (Subscriber? subscriber, List<RentalCopy>? Copies, string errorMessage) GenrateCopies(RentalFormViewModel model, int? RentalId = null)
        {
            var id = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));
            var subscriber = _context.Subscribers.Include(s => s.Subscriptions).Include(s => s.Rentals).ThenInclude(r => r.RentalCopies).ThenInclude(rc => rc.BookCopy).SingleOrDefault(s => s.Id == id);

            if (subscriber is null)
                return (subscriber: null, Copies: null, "This subscriber not found!");


            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber, RentalId);
            if (!string.IsNullOrEmpty(errorMessage))
                return (subscriber: subscriber, Copies: null, errorMessage);

            var selectedCopies = _context.BookCopies.Include(c => c.Book).Include(c => c.RentalCopies).Where(c => model.SelectedCopies.Contains(c.SerialNumber)).ToList();

            var currentSubscriberRentals = subscriber.Rentals.Where(r => RentalId == null || r.Id != RentalId).SelectMany(r => r.RentalCopies).Where(rc => !rc.ReturnDate.HasValue).Select(rc => rc.BookCopy!.BookId).ToList();


            var copies = new List<RentalCopy>();

            foreach (var copy in selectedCopies)
            {
                if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                    return (subscriber: subscriber, Copies: null, Errors.NotAvailableForRental);

                //check that copy is not in rental
                if (copy.RentalCopies.Any(c => !c.ReturnDate.HasValue && (RentalId == null || c.RentalId != RentalId)))
                    return (subscriber: subscriber, Copies: null, Errors.CopyIsInRental);

                if (currentSubscriberRentals.Any(bookId => bookId == copy.BookId))
                    return (subscriber: subscriber, Copies: null, $"this subscriber already has a copy for this book '{copy.Book.Title}'");

                copies.Add(new RentalCopy { BookCopyId = copy.Id });
            }

            return (subscriber: subscriber, Copies: copies, errorMessage: string.Empty);
        }

        private (string errorMessage, int? maxAllowedCopies) ValidateSubscriber(Subscriber subscriber, int? rentalId = null)
        {
            if (subscriber.IsBlackListed)
                return (errorMessage: Errors.BlackListedSubscriber, maxAllowedCopies: null);

            if (subscriber.Subscriptions.Last().EndDate < DateTime.Today.AddDays((int)RentalsConfigurations.RentalDuration))
                return (errorMessage: Errors.InActiveSubscriber, maxAllowedCopies: null);

            var currentRentals = subscriber.Rentals.Where(r => rentalId == null || r.Id != rentalId).SelectMany(r => r.RentalCopies).Count(c => !c.ReturnDate.HasValue);

            var allowedRentals = (int)RentalsConfigurations.MaxAllowedCopies - currentRentals;

            if (allowedRentals.Equals(0))
                return (errorMessage: Errors.MaxCopiesReached, maxAllowedCopies: null);


            return (errorMessage: string.Empty, maxAllowedCopies: allowedRentals);
        }
    }
}
