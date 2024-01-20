using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

namespace BookifyTest.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;

        public ResendEmailConfirmationModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender, IEmailBodyBuilder emailBodyBuilder)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _emailBodyBuilder = emailBodyBuilder;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public string Username { get; set; } = null!;
        }

        public void OnGet(string username)
        {
            Input = new()
            {
                Username = username,
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userName = Input.Username.ToUpper();
            var user = await _userManager.Users.SingleOrDefaultAsync(u => (u.NormalizedUserName == userName || u.NormalizedEmail == userName));
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
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

            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            return Page();
        }
    }
}
