using Bookify.Domain.Dtos;
using Microsoft.AspNetCore.DataProtection;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class RentalsController : Controller
    {
        private readonly IRentalService _rentalService;
        private readonly IBookCopyService _bookCopyService;
        private readonly ISubscriberService _subscriberService;
        private readonly IDataProtector _dataProtector;
        private readonly IMapper _mapper;

        public RentalsController(ISubscriberService subscriberService, IBookCopyService bookCopyService, IRentalService rentalService, IDataProtectionProvider dataProtector, IMapper mapper)
        {
            _subscriberService = subscriberService;
            _bookCopyService = bookCopyService;
            _rentalService = rentalService;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
            _mapper = mapper;
        }

        public IActionResult Create(string sKey)
        {
            var id = int.Parse(_dataProtector.Unprotect(sKey));
            var subscriber = _subscriberService.CanRent(id);

            if (subscriber is null)
                return NotFound();

            if (!string.IsNullOrEmpty(subscriber.Value.errorMessage))
                return View("NotAllowedRental", subscriber.Value.errorMessage);

            var viewModel = new RentalFormViewModel
            {
                SubscriberKey = sKey,
                MaxAllowedCopies = subscriber.Value.maxAllowedCopies
            };
            return View("Form", viewModel);
        }

        [HttpPost]
        public IActionResult Create(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));

            var subscriber = _subscriberService.CanRent(subscriberId);

            if (subscriber is null)
                return NotFound();

            if (!string.IsNullOrEmpty(subscriber.Value.errorMessage))
                return View("NotAllowedRental", subscriber.Value.errorMessage);

            var (rentalsError, copies) = _bookCopyService.CanBeRented(model.SelectedCopies, subscriberId);

            if (!string.IsNullOrEmpty(rentalsError))
                return View("NotAllowedRental", rentalsError);

            var rental = _rentalService.Add(subscriberId, copies, User.GetUserId());

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }

        public IActionResult Edit(int id)
        {
            var rental = _rentalService.GetDetails(id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            var subscriber = _subscriberService.CanRent(rental.SubScriberId, rental.Id);

            if (subscriber is null)
                return NotFound();

            if (!string.IsNullOrEmpty(subscriber.Value.errorMessage))
                return View("NotAllowedRental", subscriber.Value.errorMessage);

            var currentCopiesIds = rental.RentalCopies.Select(c => c.BookCopyId).ToList();

            var currentCopies = _bookCopyService.GetRentalCopies(currentCopiesIds);

            var viewModel = new RentalFormViewModel
            {
                SubscriberKey = _dataProtector.Protect(rental.SubScriberId.ToString()),
                MaxAllowedCopies = subscriber.Value.maxAllowedCopies,
                CurrentCopies = _mapper.Map<IEnumerable<BookCopyViewModel>>(currentCopies)
            };

            return View("Form", viewModel);
        }

        [HttpPost]
        public IActionResult Edit(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            var rental = _rentalService.GetDetails(model.Id ?? 0);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            var subscriber = _subscriberService.CanRent(rental.SubScriberId, rental.Id);

            if (subscriber is null)
                return NotFound();

            if (!string.IsNullOrEmpty(subscriber.Value.errorMessage))
                return View("NotAllowedRental", subscriber.Value.errorMessage);

            var (rentalsError, copies) = _bookCopyService.CanBeRented(model.SelectedCopies, rental.SubScriberId, rental.Id);

            if (!string.IsNullOrEmpty(rentalsError))
                return View("NotAllowedRental", rentalsError);

            _rentalService.Update(rental.Id, copies, User.GetUserId());

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }

        public IActionResult Return(int id)
        {
            var rental = _rentalService.GetDetails(id);

            if (rental is null || rental.CreatedOn.Date == DateTime.Today)
                return NotFound();

            var subscriber = _subscriberService.GetByIdWithSubscriptions(rental.SubScriberId);

            var viewModel = new RentalReturnFormViewModel
            {
                Id = id,
                Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies.Where(c => !c.ReturnDate.HasValue).ToList()),
                SelectedCopies = rental.RentalCopies.Where(c => !c.ReturnDate.HasValue).Select(c => new ReturnCopyViewModel { Id = c.BookCopyId, IsReturned = c.ExtendedOn.HasValue ? false : null }).ToList(),
                AllowExtend = _rentalService.AllowExtend(rental, subscriber!)
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Return(RentalReturnFormViewModel model)
        {
            var rental = _rentalService.GetDetails(model.Id);

            if (rental is null || rental.CreatedOn.Date == DateTime.Today)
                return NotFound();

            var copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies).Where(c => !c.ReturnDate.HasValue).ToList();

            if (!ModelState.IsValid)
            {
                model.Copies = copies;
                return View(model);
            }

            var subscriber = _subscriberService.GetByIdWithSubscriptions(rental.SubScriberId);

            if (model.SelectedCopies.Any(c => c.IsReturned.HasValue && !c.IsReturned.Value))
            {
                var error = _rentalService.ValidateExtendedCopies(rental, subscriber!);

                if (!string.IsNullOrEmpty(error))
                {
                    model.Copies = copies;
                    ModelState.AddModelError("", error);
                    return View(model);
                }
            }

            var copiesDto = _mapper.Map<IList<ReturnCopyDto>>(model.SelectedCopies);

            _rentalService.Return(rental, copiesDto, model.PenaltyPaid, User.GetUserId());

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }

        [HttpPost]
        public IActionResult GetCopyDetails(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var copy = _bookCopyService.GetActiveCopyBySerialNumber(model.Value);

            if (copy is null)
                return NotFound(Errors.InvalidSerialNumber);

            if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                return BadRequest(Errors.NotAvailableForRental);

            //check that copy is not in rental
            var copyIsInRental = _bookCopyService.CopyIsInRental(copy.Id);

            if (copyIsInRental)
                return BadRequest(Errors.CopyIsInRental);

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);
            return PartialView("_CopyDetails", viewModel);
        }

        [HttpPost]
        public IActionResult MarkedAsDeleted(int id)
        {
            var rental = _rentalService.MarkAsDeleted(id, User.GetUserId());

            if (rental is null)
                return NotFound();

            var copiesCount = _rentalService.GetNumberOfCopies(id);

            return Ok(copiesCount);
        }

        public IActionResult Details(int id)
        {
            var query = _rentalService.GetQueryableDetails(id);

            var viewModel = _mapper.ProjectTo<RentalViewModel>(query).SingleOrDefault(r => r.Id == id);

            return viewModel is null ? NotFound() : View(viewModel);
        }
    }
}