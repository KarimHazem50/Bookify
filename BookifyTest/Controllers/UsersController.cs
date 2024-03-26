using Bookify.Domain.Dtos;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using System.Data;
using System.Text;
using System.Text.Encodings.Web;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;


        public UsersController(IAuthService authService, IMapper mapper, IEmailSender emailSender, IEmailBodyBuilder emailBodyBuilder)
        {
            _authService = authService;
            _mapper = mapper;
            _emailSender = emailSender;
            _emailBodyBuilder = emailBodyBuilder;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _authService.GetUsersAsync();
            var viewModel = _mapper.Map<IEnumerable<UsersViewModel>>(users);
            return View(viewModel);
        }

        [AjaxOnly]
        public async Task<IActionResult> Create()
        {
            var roles = await _authService.GetRolesAsync();
            var viewModel = new UserFormViewModel
            {
                DisplayRoles = roles.Select(r => new SelectListItem { Text = r.Name, Value = r.Name })
            };
            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
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

                var placehoders = new Dictionary<string, string>()
                {
                    {"imageUrl", "https://previews.123rf.com/images/johan2011/johan20111309/johan2011130900008/21934214-ok-the-dude-giving-thumb-up-next-to-a-green-check-mark.jpg"},
                    {"header", $"Hey {user.FullName} , thanks for joining us!"},
                    {"body", "please confirm your email"},
                    {"url", HtmlEncoder.Default.Encode(callbackUrl!)},
                    {"linkTitle", "Active Account!"}
                };

                var body = _emailBodyBuilder.GetEmailBody(template: EmailTemplates.Email, placehoders: placehoders);

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email", body);

                var viewModel = _mapper.Map<UsersViewModel>(user);
                return PartialView("_UserRow", viewModel);
            }

            return BadRequest(string.Join(",", result.Errors!));
        }
        public async Task<IActionResult> AllowEmail(UserFormViewModel model)
        {
            return Json(await _authService.AllowEmailAsync(model.Id, model.Email));
        }
        public async Task<IActionResult> AllowUserName(UserFormViewModel model)
        {
            return Json(await _authService.AllowUserNameAsync(model.Id, model.UserName));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _authService.ToggleStatusAsync(id, User.GetUserId());

            if (user is null)
                return NotFound();

            var viewModel = _mapper.Map<UsersViewModel>(user);
            return PartialView("_UserRow", viewModel);
        }

        [AjaxOnly]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _authService.GetByIdAsync(id);
            if (user is null)
                return NotFound();
            return PartialView("_FormResetPassword");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(UserResetPasswordFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _authService.GetByIdAsync(model.Id);
            if (user is null)
                return NotFound();

            var result = await _authService.ResetPasswordAsync(user, model.Password, User.GetUserId());

            if (result.IsSucceeded)
            {
                var viewModel = _mapper.Map<UsersViewModel>(user);
                return PartialView("_UserRow", viewModel);
            }
            return BadRequest(string.Join(",", result.Errors!));
        }

        [AjaxOnly]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _authService.GetByIdAsync(id);
            if (user == null)
                return NotFound();


            var roles = await _authService.GetRolesAsync();
            var s = roles.Select(r => new SelectListItem { Text = r.Name, Value = r.Name });
            UserFormViewModel viewModel = new()
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName,
                Email = user.Email,
                DisplayRoles = (await _authService.GetRolesAsync()).Select(r => new SelectListItem { Text = r.Name, Value = r.Name }),
                SelectedRoles = await _authService.GetRolesAsync(user),
            };
            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _authService.GetByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            _mapper.Map(model, user);

            var result = await _authService.UpdateUserAsync(user, model.SelectedRoles, User.GetUserId());

            if (result.IsSucceeded)
            {
                var viewModel = _mapper.Map<UsersViewModel>(result.User);
                return PartialView("_UserRow", viewModel);
            }

            return BadRequest(string.Join(",", result.Errors!));
        }

        [HttpPost]
        public async Task<IActionResult> UnLock(string id)
        {
            var user = await _authService.UnLockAsync(id, User.GetUserId());
            if (user == null)
                return NotFound();

            var viewModel = _mapper.Map<UsersViewModel>(user);
            return PartialView("_UserRow", viewModel);
        }
    }
}