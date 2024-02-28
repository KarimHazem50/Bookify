using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookifyTest.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IImageService _imageService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IImageService imageService,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _imageService = imageService;
            _webHostEnvironment = webHostEnvironment;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required, MaxLength(30, ErrorMessage = Errors.MaxLength), Display(Name = "Full Name"), RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
            public string FullName { get; set; } = null!;

            [Phone]
            [Display(Name = "Phone Number"), MaxLength(11, ErrorMessage = Errors.MaxLength), RegularExpression(RegexPatterns.MobileNumber, ErrorMessage = Errors.InvalidMobileNumber)]
            public string? PhoneNumber { get; set; }

            public IFormFile? Avatar { get; set; }

            public bool DeletedImage { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            _imageService.Delete($"{user.Id}.png", "/Images/users/removedImages", HasThumbnailPath: false);

            Username = userName;

            Input = new InputModel
            {
                FullName = user.FullName,
                PhoneNumber = phoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (Input.Avatar is not null)
            {
                var sourceImagePath = Path.Combine($"{_webHostEnvironment.WebRootPath}/Images/users", $"{user.Id}.png");
                var destinationImagePath = Path.Combine($"{_webHostEnvironment.WebRootPath}/Images/users/removedImages", $"{user.Id}.png");

                if (System.IO.File.Exists(sourceImagePath))
                {
                    System.IO.File.Copy(sourceImagePath, destinationImagePath, true);
                }


                _imageService.Delete($"{user.Id}.png", "/Images/users", HasThumbnailPath: false);

                var (isUploaded, errorMessage) = await _imageService.UploadAsync(Input.Avatar, $"{user.Id}.png", "/Images/users", hasThumbnail: false);

                if (!isUploaded)
                {
                    if (System.IO.File.Exists(destinationImagePath))
                    {
                        System.IO.File.Copy(destinationImagePath, sourceImagePath, true);
                    }

                    ModelState.AddModelError("Input.Avatar", errorMessage!);
                    await LoadAsync(user);
                    return Page();
                }
            }

            if (Input.Avatar is null && Input.DeletedImage)
            {
                _imageService.Delete($"{user.Id}.png", "/Images/users", HasThumbnailPath: false);
                await LoadAsync(user);
                return Page();
            }


            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (Input.FullName != user.FullName)
            {
                user.FullName = Input.FullName;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set full name.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
