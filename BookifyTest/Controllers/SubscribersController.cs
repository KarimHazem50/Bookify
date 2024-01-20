using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookifyTest.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class SubscribersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;
        private readonly IDataProtector _dataProtector;
        public SubscribersController(ApplicationDbContext context, IMapper mapper, IImageService imageService, IEmailBodyBuilder emailBodyBuilder, IEmailSender emailSender, IDataProtectionProvider dataProtector)
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
        }
        private SubscriberFormViewModel PopulateData(SubscriberFormViewModel? model = null)
        {
            var Governorates = _context.Governorates.Where(g => !g.IsDeleted).OrderBy(g => g.Name).ToList();

            var viewModel = model is null ? new SubscriberFormViewModel() : model;

            viewModel.DisplayGovernorates = _mapper.Map<IEnumerable<SelectListItem>>(Governorates);

            if (viewModel.GovernorateId > 0)
            {
                var areas = _context.Areas.Where(a => !a.IsDeleted && a.GovernorateId == viewModel.GovernorateId).OrderBy(g => g.Name).ToList();
                viewModel.DisplayAreas = _mapper.Map<IEnumerable<SelectListItem>>(areas);
            }

            return viewModel;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var subscriber = _context.Subscribers.SingleOrDefault(s => !s.IsDeleted &&
            (s.MobileNumber == model.Value || s.Email == model.Value || s.NationalId == model.Value));

            var viewModel = _mapper.Map<SubscriberSearchResultViewModel>(subscriber);

            if (subscriber is not null)
                viewModel.Key = _dataProtector.Protect(subscriber.Id.ToString());

            return PartialView("_Result", viewModel);
        }
        public IActionResult Create()
        {
            return View("Form", PopulateData());
        }

        [AjaxOnly]
        public IActionResult GetAreas(int id)
        {
            var areas = _context.Areas.Where(a => !a.IsDeleted && a.GovernorateId == id).Select(a => new { a.Id, a.Name }).OrderBy(a => a.Name).ToList();
            return Ok(areas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateData(model));


            var extension = Path.GetExtension(model.Image!.FileName);
            var imageName = $"{Guid.NewGuid()}{extension}";

            var result = await _imageService.UploadAsync(model.Image, imageName, "/Images/subscribers", hasThumbnail: true);

            if (!result.isUploaded)
            {
                ModelState.AddModelError(nameof(model.Image), result.errorMessage!);
                return View("Form", PopulateData(model));
            }

            model.ImageName = imageName;

            var subscriber = _mapper.Map<Subscriber>(model);
            subscriber.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            Subscription subscription = new()
            {
                CreatedById = subscriber.CreatedById,
                CreatedOn = subscriber.CreatedOn,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1)
            };

            subscriber.Subscriptions.Add(subscription);

            _context.Subscribers.Add(subscriber);
            _context.SaveChanges();


            var placehoders = new Dictionary<string, string>()
            {
                {"imageUrl", "https://previews.123rf.com/images/johan2011/johan20111309/johan2011130900008/21934214-ok-the-dude-giving-thumb-up-next-to-a-green-check-mark.jpg"},
                {"header", $"Welcome {subscriber.FirstName}"},
                {"body", "thanks for joining Bookify 😍"},
            };
            var body = _emailBodyBuilder.GetEmailBody(template: EmailTemplates.Notification, placehoders: placehoders);
            await _emailSender.SendEmailAsync(subscriber.Email, "Welcome to Bookify", body);


            var subscriberId = _dataProtector.Protect(subscriber.Id.ToString());
            return RedirectToAction(nameof(Details), new { id = subscriberId });
        }
        public IActionResult AllowedEmail(SubscriberFormViewModel model)
        {
            var subscriber = _context.Subscribers.SingleOrDefault(s => s.Email == model.Email);

            var id = 0;
            if (!string.IsNullOrEmpty(model.Key))
            {
                id = int.Parse(_dataProtector.Unprotect(model.Key));
            }

            var isAllowed = subscriber is null || id == subscriber.Id;

            return Json(isAllowed);
        }
        public IActionResult AllowedMobileNumber(SubscriberFormViewModel model)
        {
            var subscriber = _context.Subscribers.SingleOrDefault(s => s.MobileNumber == model.MobileNumber);

            var id = 0;
            if (!string.IsNullOrEmpty(model.Key))
            {
                id = int.Parse(_dataProtector.Unprotect(model.Key));
            }

            var isAllowed = subscriber is null || id == subscriber.Id;

            return Json(isAllowed);
        }
        public IActionResult AllowedNationlID(SubscriberFormViewModel model)
        {
            var subscriber = _context.Subscribers.SingleOrDefault(s => s.NationalId == model.NationalId);

            var id = 0;
            if (!string.IsNullOrEmpty(model.Key))
            {
                id = int.Parse(_dataProtector.Unprotect(model.Key));
            }

            var isAllowed = subscriber is null || id == subscriber.Id;

            return Json(isAllowed);
        }
        public IActionResult Edit(string id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));

            var subscriber = _context.Subscribers.Find(subscriberId);
            if (subscriber is null)
                return NotFound();

            var model = _mapper.Map<SubscriberFormViewModel>(subscriber);
            var viewModel = PopulateData(model);

            viewModel.Key = id;
            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateData(model));

            var id = int.Parse(_dataProtector.Unprotect(model.Key!));
            var subscriber = _context.Subscribers.Find(id);

            if (subscriber is null)
                return NotFound();

            if (model.Image is not null)
            {
                var extension = Path.GetExtension(model.Image.FileName);
                var imageName = $"{Guid.NewGuid()}{extension}";
                var result = await _imageService.UploadAsync(model.Image, imageName, "/Images/subscribers", hasThumbnail: true);

                if (result.isUploaded)
                {
                    _imageService.Delete(subscriber.ImageName, "/Images/subscribers", HasThumbnailPath: true);
                    model.ImageName = imageName;
                }
                else
                {
                    ModelState.AddModelError(nameof(model.Image), result.errorMessage!);
                    model.ImageName = subscriber.ImageName;
                    return View("Form", PopulateData(model));
                }
            }
            else
            {
                model.ImageName = subscriber.ImageName;
            }

            subscriber = _mapper.Map(model, subscriber);
            subscriber.LastUpdatedOn = DateTime.Now;
            subscriber.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = model.Key });
        }
        public IActionResult Details(string id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));

            var subsriber = _context.Subscribers.Include(s => s.Area).Include(s => s.Governorate).Include(s => s.Subscriptions).SingleOrDefault(s => s.Id == subscriberId);
            if (subsriber is null)
                return NotFound();

            var viewModel = _mapper.Map<SubscriberDetailsViewModel>(subsriber);
            viewModel.Key = id;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenewSubscriptionAsync(string sKey)
        {
            var id = int.Parse(_dataProtector.Unprotect(sKey));
            var subscriber = _context.Subscribers.Include(s => s.Subscriptions).SingleOrDefault(s => s.Id == id);
            if (subscriber is null)
                return NotFound();
            if (subscriber.IsBlackListed)
                return BadRequest();

            var lastSubscription = subscriber.Subscriptions.Last();
            var startDate = lastSubscription.EndDate < DateTime.Today ? DateTime.Today : lastSubscription.EndDate.AddDays(1);

            Subscription newSubscription = new()
            {
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                CreatedOn = DateTime.Now,
                StartDate = startDate,
                EndDate = startDate.AddYears(1)
            };

            subscriber.Subscriptions.Add(newSubscription);

            _context.SaveChanges();


            var placehoders = new Dictionary<string, string>()
            {
                {"imageUrl", "https://previews.123rf.com/images/johan2011/johan20111309/johan2011130900008/21934214-ok-the-dude-giving-thumb-up-next-to-a-green-check-mark.jpg"},
                {"header", $"Hello {subscriber.FirstName}"},
                {"body", $"Your subscription has been renewed {subscriber.Subscriptions.Last().EndDate.ToString("d MMM yyyy")} 😍"},
            };
            var body = _emailBodyBuilder.GetEmailBody(template: EmailTemplates.Notification, placehoders: placehoders);
            await _emailSender.SendEmailAsync(subscriber.Email, "Welcome to Bookify", body);

            var viewModel = _mapper.Map<SubscriptionViewModel>(newSubscription);
            return PartialView("_SubscriptionRow", viewModel);
        }
    }
}
