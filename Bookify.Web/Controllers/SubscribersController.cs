using FluentValidation.AspNetCore;
using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class SubscribersController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDataProtector _dataProtector;
        private readonly IMapper _mapper;
        private readonly IValidator<SubscriberFormViewModel> _validator;
        private readonly IWhatsAppClient _whatsAppClient;
        private readonly ISubscriberService _subscriberService;
        private readonly IAreaService _areaService;
        private readonly IGovernorateService _governorateService;

        private readonly IImageService _imageService;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;

        public SubscribersController(
            IWebHostEnvironment webHostEnvironment,
            IDataProtectionProvider dataProtector,
            IMapper mapper,
            IValidator<SubscriberFormViewModel> validator,
            IWhatsAppClient whatsAppClient,
            ISubscriberService subscriberService,
            IAreaService areaService,
            IGovernorateService governorateService,
            IImageService imageService,
            IEmailBodyBuilder emailBodyBuilder,
            IEmailSender emailSender)
        {
            _webHostEnvironment = webHostEnvironment;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
            _mapper = mapper;
            _validator = validator;
            _whatsAppClient = whatsAppClient;
            _subscriberService = subscriberService;
            _areaService = areaService;
            _governorateService = governorateService;
            _imageService = imageService;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
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

            var query = _subscriberService.GetQueryable();

            var subscriber = _mapper.ProjectTo<SubscriberSearchResultViewModel>(query)
                                    .SingleOrDefault(s =>
                                                       s.Email == model.Value
                                                    || s.NationalId == model.Value
                                                    || s.MobileNumber == model.Value);

            if (subscriber is not null)
                subscriber.Key = _dataProtector.Protect(subscriber.Id.ToString());

            return PartialView("_Result", subscriber);
        }

        public IActionResult Details(string id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));

            var query = _subscriberService.GetQueryableDetails();

            var viewModel = _mapper.ProjectTo<SubscriberViewModel>(query).SingleOrDefault(s => s.Id == subscriberId);

            if (viewModel is null)
                return NotFound();

            viewModel.Key = id;

            return View(viewModel);
        }

        public IActionResult Create()
        {
            var viewModel = PopulateViewModel();
            return View("Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SubscriberFormViewModel model)
        {
            var validationResult = _validator.Validate(model);

            if (!validationResult.IsValid)
                validationResult.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var subscriber = _mapper.Map<Subscriber>(model);

            var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image!.FileName)}";
            var imagePath = "/images/Subscribers";

            var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, imagePath, hasThumbnail: true);

            if (!isUploaded)
            {
                ModelState.AddModelError("Image", errorMessage!);
                return View("Form", PopulateViewModel(model));
            }

            subscriber = _subscriberService.Add(subscriber, imagePath, imageName, User.GetUserId());

            //Send welcome email
            var placeholders = new Dictionary<string, string>()
            {
                { "imageUrl", "https://res.cloudinary.com/devcreed/image/upload/v1668739431/icon-positive-vote-2_jcxdww.svg" },
                { "header", $"Welcome {model.FirstName}," },
                { "body", "thanks for joining Bookify 🤩" }
            };

            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(
                model.Email,
                "Welcome to Bookify", body));

            //Send welcome message using WhatsApp
            //You can logic to  whatsApp service
            if (model.HasWhatsApp)
            {
                var components = new List<WhatsAppComponent>()
                {
                    new WhatsAppComponent
                    {
                        Type = "body",
                        Parameters = new List<object>()
                        {
                            new WhatsAppTextParameter { Text = model.FirstName }
                        }
                    }
                };

                var mobileNumber = _webHostEnvironment.IsDevelopment() ? "Add Your Number" : model.MobileNumber;

                //Change 2 with your country code
                BackgroundJob.Enqueue(() => _whatsAppClient
                    .SendMessage($"2{mobileNumber}", WhatsAppLanguageCode.English_US,
                    WhatsAppTemplates.WelcomeMessage, components));
            }

            var subscriberId = _dataProtector.Protect(subscriber.Id.ToString());

            return RedirectToAction(nameof(Details), new { id = subscriberId });
        }

        public IActionResult Edit(string id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));
            var subscriber = _subscriberService.GetById(subscriberId);

            if (subscriber is null)
                return NotFound();

            var model = _mapper.Map<SubscriberFormViewModel>(subscriber);
            var viewModel = PopulateViewModel(model);
            viewModel.Key = id;

            return View("Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SubscriberFormViewModel model)
        {
            var validationResult = _validator.Validate(model);

            if (!validationResult.IsValid)
                validationResult.AddToModelState(ModelState);

            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.Key!));

            var subscriber = _subscriberService.GetById(subscriberId);

            if (subscriber is null)
                return NotFound();

            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(subscriber.ImageUrl))
                    _imageService.Delete(subscriber.ImageUrl, subscriber.ImageThumbnailUrl);

                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
                var imagePath = "/images/Subscribers";

                var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, imagePath, hasThumbnail: true);

                if (!isUploaded)
                {
                    ModelState.AddModelError("Image", errorMessage!);
                    return View("Form", PopulateViewModel(model));
                }

                model.ImageUrl = $"{imagePath}/{imageName}";
                model.ImageThumbnailUrl = $"{imagePath}/thumb/{imageName}";
            }

            else if (!string.IsNullOrEmpty(subscriber.ImageUrl))
            {
                model.ImageUrl = subscriber.ImageUrl;
                model.ImageThumbnailUrl = subscriber.ImageThumbnailUrl;
            }

            subscriber = _mapper.Map(model, subscriber);
            _subscriberService.Update(subscriber, User.GetUserId());

            return RedirectToAction(nameof(Details), new { id = model.Key });
        }

        [HttpPost]
        public IActionResult RenewSubscription(string sKey)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(sKey));

            var subscriber = _subscriberService.GetSubscriberWithSubscriptions(subscriberId);

            if (subscriber is null)
                return NotFound();

            if (subscriber.IsBlackListed)
                return BadRequest();

            var lastSubscription = subscriber.Subscriptions.Last();

            var startDate = lastSubscription.EndDate < DateTime.Today
                            ? DateTime.Today
                            : lastSubscription.EndDate.AddDays(1);

            var newSubscription = _subscriberService.RenewSubscription(subscriberId, startDate, User.GetUserId());

            //Send email and WhatsApp Message
            var placeholders = new Dictionary<string, string>()
            {
                { "imageUrl", "https://res.cloudinary.com/devcreed/image/upload/v1668739431/icon-positive-vote-2_jcxdww.svg" },
                { "header", $"Hello {subscriber.FirstName}," },
                { "body", $"your subscription has been renewed through {newSubscription.EndDate.ToString("d MMM, yyyy")} 🎉🎉" }
            };

            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(
                subscriber.Email,
                "Bookify Subscription Renewal", body));

            if (subscriber.HasWhatsApp)
            {
                var components = new List<WhatsAppComponent>()
                {
                    new WhatsAppComponent
                    {
                        Type = "body",
                        Parameters = new List<object>()
                        {
                            new WhatsAppTextParameter { Text = subscriber.FirstName },
                            new WhatsAppTextParameter { Text = newSubscription.EndDate.ToString("d MMM, yyyy") },
                        }
                    }
                };

                var mobileNumber = _webHostEnvironment.IsDevelopment() ? "Add Your Number" : subscriber.MobileNumber;

                //Change 2 with your country code
                BackgroundJob.Enqueue(() => _whatsAppClient
                    .SendMessage($"2{mobileNumber}", WhatsAppLanguageCode.English,
                    WhatsAppTemplates.SubscriptionRenew, components));
            }

            var viewModel = _mapper.Map<SubscriptionViewModel>(newSubscription);

            return PartialView("_SubscriptionRow", viewModel);
        }

        [AjaxOnly]
        public IActionResult GetAreas(int governorateId)
        {
            var areas = _areaService.GetActiveAreasByGovernorateId(governorateId);

            return Ok(_mapper.Map<IEnumerable<SelectListItem>>(areas));
        }

        public IActionResult AllowNationalId(SubscriberFormViewModel model)
        {
            var subscriberId = 0;

            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            return Json(_subscriberService.AllowNationalId(subscriberId, model.NationalId));
        }

        public IActionResult AllowMobileNumber(SubscriberFormViewModel model)
        {
            var subscriberId = 0;

            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            return Json(_subscriberService.AllowMobileNumber(subscriberId, model.MobileNumber));
        }

        public IActionResult AllowEmail(SubscriberFormViewModel model)
        {
            var subscriberId = 0;

            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            return Json(_subscriberService.AllowEmail(subscriberId, model.Email));
        }

        private SubscriberFormViewModel PopulateViewModel(SubscriberFormViewModel? model = null)
        {
            SubscriberFormViewModel viewModel = model is null ? new SubscriberFormViewModel() : model;

            var governorates = _governorateService.GetActiveGovernorates();
            viewModel.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(governorates);

            if (model?.GovernorateId > 0)
            {
                var areas = _areaService.GetActiveAreasByGovernorateId(model.GovernorateId);

                viewModel.Areas = _mapper.Map<IEnumerable<SelectListItem>>(areas);
            }

            return viewModel;
        }
    }
}