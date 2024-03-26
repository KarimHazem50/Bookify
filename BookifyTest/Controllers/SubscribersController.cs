using Hangfire;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhatsAppCloudApi;
using WhatsAppCloudApi.Services;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class SubscribersController : Controller
    {
        private readonly IGovernorateService _governorateService;
        private readonly ISubscriberService _subscriberService;
        private readonly IAreaService _areaService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IImageService _imageService;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;
        private readonly IWhatsAppClient _whatsAppClient;
        private readonly IDataProtector _dataProtector;
        public SubscribersController(IWebHostEnvironment webHostEnvironment, IMapper mapper, IImageService imageService, IEmailBodyBuilder emailBodyBuilder, IEmailSender emailSender, IWhatsAppClient whatsAppClient, IDataProtectionProvider dataProtector, IGovernorateService governorateService, IAreaService areaService, ISubscriberService subscriberService)
        {
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
            _imageService = imageService;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
            _whatsAppClient = whatsAppClient;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
            _governorateService = governorateService;
            _areaService = areaService;
            _subscriberService = subscriberService;
        }
        private SubscriberFormViewModel PopulateData(SubscriberFormViewModel? model = null)
        {
            var Governorates = _governorateService.GetActiveGovernorates();

            var viewModel = model is null ? new SubscriberFormViewModel() : model;

            viewModel.DisplayGovernorates = _mapper.Map<IEnumerable<SelectListItem>>(Governorates);

            if (viewModel.GovernorateId > 0)
            {
                var areas = _areaService.GetActiveAreasByGovernorateId(viewModel.GovernorateId);
                viewModel.DisplayAreas = _mapper.Map<IEnumerable<SelectListItem>>(areas);
            }

            return viewModel;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Search(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var subscriber = _subscriberService.GetSubscriberByMobileOrEmailOrNationalId(model.Value);

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
            var areas = _areaService.GetActiveAreasByGovernorateId(id).Select(a => new { a.Id, a.Name }).ToList();
            return Ok(areas);
        }

        [HttpPost]
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

            _subscriberService.Add(subscriber, User.GetUserId());

            // Send Mail Message
            var placehoders = new Dictionary<string, string>()
            {
                {"imageUrl", "https://previews.123rf.com/images/johan2011/johan20111309/johan2011130900008/21934214-ok-the-dude-giving-thumb-up-next-to-a-green-check-mark.jpg"},
                {"header", $"Welcome {subscriber.FirstName}"},
                {"body", "thanks for joining Bookify 😍"},
            };
            var body = _emailBodyBuilder.GetEmailBody(template: EmailTemplates.Notification, placehoders: placehoders);
            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(subscriber.Email, "Welcome to Bookify", body));


            // Send WhatsApp Message
            if (subscriber.HasWhatsApp)
            {
                var components = new List<WhatsAppComponent>()
                    {
                        new WhatsAppComponent
                        {
                            Type = "body",
                            Parameters = new List<object>()
                            {
                                new WhatsAppTextParameter
                                {
                                    Text = subscriber.FirstName
                                }
                            }
                        }
                    };
                BackgroundJob.Enqueue(() => _whatsAppClient.SendMessage(_webHostEnvironment.IsDevelopment() ? "201027453613" : $"2{subscriber.MobileNumber}", WhatsAppLanguageCode.English_US, WhatsAppTemplates.WelcomeMessage, components));
            }

            var subscriberId = _dataProtector.Protect(subscriber.Id.ToString());
            return RedirectToAction(nameof(Details), new { id = subscriberId });
        }
        public IActionResult AllowedEmail(SubscriberFormViewModel model)
        {
            var id = 0;
            if (!string.IsNullOrEmpty(model.Key))
            {
                id = int.Parse(_dataProtector.Unprotect(model.Key));
            }
            return Json(_subscriberService.AllowedEmail(model.Email, id));
        }
        public IActionResult AllowedMobileNumber(SubscriberFormViewModel model)
        {
            var id = 0;
            if (!string.IsNullOrEmpty(model.Key))
            {
                id = int.Parse(_dataProtector.Unprotect(model.Key));
            }
            return Json(_subscriberService.AllowedMobileNumber(model.MobileNumber, id));
        }
        public IActionResult AllowedNationlID(SubscriberFormViewModel model)
        {
            var id = 0;
            if (!string.IsNullOrEmpty(model.Key))
            {
                id = int.Parse(_dataProtector.Unprotect(model.Key));
            }
            return Json(_subscriberService.AllowedNationlID(model.NationalId, id));
        }
        public IActionResult Edit(string id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));

            var subscriber = _subscriberService.GetById(subscriberId);
            if (subscriber is null)
                return NotFound();

            var model = _mapper.Map<SubscriberFormViewModel>(subscriber);
            var viewModel = PopulateData(model);

            viewModel.Key = id;
            return View("Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateData(model));

            var id = int.Parse(_dataProtector.Unprotect(model.Key!));
            var subscriber = _subscriberService.GetById(id);

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

            _subscriberService.Update(subscriber, User.GetUserId());

            return RedirectToAction(nameof(Details), new { id = model.Key });
        }
        public IActionResult Details(string id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));

            var query = _subscriberService.GetQueryableDetails();

            var viewModel = _mapper.ProjectTo<SubscriberDetailsViewModel>(query).SingleOrDefault(s => s.Id == subscriberId);

            if (viewModel is null)
                return NotFound();

            viewModel.Key = id;
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult RenewSubscription(string sKey)
        {
            var id = int.Parse(_dataProtector.Unprotect(sKey));
            var subscriber = _subscriberService.GetByIdWithSubscriptions(id);

            if (subscriber is null)
                return NotFound();
            if (subscriber.IsBlackListed)
                return BadRequest();

            var newSubscription = _subscriberService.RenewSubscription(subscriber, User.GetUserId());

            // Send Mail Messaga
            var placehoders = new Dictionary<string, string>()
            {
                {"imageUrl", "https://previews.123rf.com/images/johan2011/johan20111309/johan2011130900008/21934214-ok-the-dude-giving-thumb-up-next-to-a-green-check-mark.jpg"},
                {"header", $"Hello {subscriber.FirstName}"},
                {"body", $"Your subscription has been renewed {newSubscription.EndDate.ToString("d MMM yyyy")} 🥳🥳"},
            };
            var body = _emailBodyBuilder.GetEmailBody(template: EmailTemplates.Notification, placehoders: placehoders);
            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(subscriber.Email, "Bookify Subscription Renewal", body));


            // Send WhatsApp Message
            if (subscriber.HasWhatsApp)
            {
                var components = new List<WhatsAppComponent>()
                    {
                        new WhatsAppComponent
                        {
                            Type = "body",
                            Parameters = new List<object>()
                            {
                                new WhatsAppTextParameter
                                {
                                    Text = subscriber.FirstName
                                },
                                 new WhatsAppTextParameter
                                {
                                    Text = newSubscription.EndDate.ToString("d MMM yyyy")
                                },
                            }
                        }
                    };
                BackgroundJob.Enqueue(() => _whatsAppClient.SendMessage(_webHostEnvironment.IsDevelopment() ? "201027453613" : $"2{subscriber.MobileNumber}", WhatsAppLanguageCode.English_US, WhatsAppTemplates.SubscriptionRenew, components));
            }


            var viewModel = _mapper.Map<SubscriptionViewModel>(newSubscription);
            return PartialView("_SubscriptionRow", viewModel);
        }
    }
}