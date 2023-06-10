using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IValidator<UserFormViewModel> _validator;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;

        private readonly IEmailBodyBuilder _emailBodyBuilder;

        public UsersController(
            IAuthService authService,
            IValidator<UserFormViewModel> validator,
            IEmailSender emailSender,
            IMapper mapper,
            IEmailBodyBuilder emailBodyBuilder)
        {
            _authService = authService;
            _validator = validator;
            _emailSender = emailSender;
            _mapper = mapper;
            _emailBodyBuilder = emailBodyBuilder;
        }


        public async Task<IActionResult> Index()
        {
            var users = await _authService.GetUsersAsync();
            return View(_mapper.Map<IEnumerable<UserViewModel>>(users));
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Create()
        {
            var roles = await _authService.GetRolesAsync();

            var viewModel = new UserFormViewModel
            {
                Roles = roles.Select(r => new SelectListItem { Text = r.Name, Value = r.Name })
            };

            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            var validationResult = _validator.Validate(model);

            if (!validationResult.IsValid)
                return BadRequest();

            var dto = _mapper.Map<CreateUserDto>(model);

            var result = await _authService.AddUserAsync(dto, User.GetUserId());

            if (result.IsSucceeded)
            {
                var user = result.User;

                var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(result.VerificationCode!));

                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user!.Id, code },
                    protocol: Request.Scheme);

                var placeholders = new Dictionary<string, string>()
                {
                    { "imageUrl", "https://res.cloudinary.com/devcreed/image/upload/v1668732314/icon-positive-vote-1_rdexez.svg" },
                    { "header", $"Hey {user.FullName}, thanks for joining us!" },
                    { "body", "please confirm your email" },
                    { "url", $"{HtmlEncoder.Default.Encode(callbackUrl!)}" },
                    { "linkTitle", "Active Account!" }
                };

                var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Email, placeholders);

                await _emailSender.SendEmailAsync(user.Email!, "Confirm your email", body);

                var viewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", viewModel);
            }

            return BadRequest(string.Join(',', result.Errors!));
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _authService.GetUsersByIdAsync(id);

            if (user is null)
                return NotFound();

            var roles = await _authService.GetRolesAsync();

            var viewModel = _mapper.Map<UserFormViewModel>(user);

            viewModel.SelectedRoles = await _authService.GetUsersRolesAsync(user);
            viewModel.Roles = roles.Select(r => new SelectListItem { Text = r.Name, Value = r.Name });

            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserFormViewModel model)
        {
            var validationResult = _validator.Validate(model);

            if (!validationResult.IsValid)
                return BadRequest();

            var user = await _authService.GetUsersByIdAsync(model.Id!);

            if (user is null)
                return NotFound();

            user = _mapper.Map(model, user);

            var result = await _authService.UpdateUserAsync(user, model.SelectedRoles, User.GetUserId());

            if (result.IsSucceeded)
            {
                var viewModel = _mapper.Map<UserViewModel>(result.User);
                return PartialView("_UserRow", viewModel);
            }

            return BadRequest(string.Join(',', result.Errors!));
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _authService.GetUsersByIdAsync(id);

            if (user is null)
                return NotFound();

            var viewModel = new ResetPasswordFormViewModel { Id = user.Id };

            return PartialView("_ResetPasswordForm", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _authService.GetUsersByIdAsync(model.Id);

            if (user is null)
                return NotFound();

            var result = await _authService.ResetPasswordAsync(user, model.Password, User.GetUserId());

            if (result.IsSucceeded)
            {
                var viewModel = _mapper.Map<UserViewModel>(result.User);
                return PartialView("_UserRow", viewModel);
            }

            return BadRequest(string.Join(',', result.Errors!));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _authService.ToggleUserStatusAsync(id, User.GetUserId());

            return user is null ? NotFound() : Ok(user.LastUpdatedOn.ToString());
        }

        [HttpPost]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _authService.UnlockUserAsync(id);

            return user is null ? NotFound() : Ok();
        }

        public async Task<IActionResult> AllowUserName(UserFormViewModel model)
        {
            return Json(await _authService.AllowUserNameAsync(model.Id, model.UserName));
        }

        public async Task<IActionResult> AllowEmail(UserFormViewModel model)
        {
            return Json(await _authService.AllowEmailAsync(model.Id, model.Email));
        }
    }
}