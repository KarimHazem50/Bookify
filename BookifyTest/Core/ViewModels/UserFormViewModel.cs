using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace BookifyTest.Core.ViewModels
{
    public class UserFormViewModel
    {
        public string? Id { get; set; }

        [MaxLength(30, ErrorMessage = Errors.MaxLength), Display(Name = "Full Name"), RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
        public string FullName { get; set; } = null!;

        [MaxLength(20, ErrorMessage = Errors.MaxLength), Display(Name = "User Name"), RegularExpression(RegexPatterns.Username, ErrorMessage = Errors.InvalidUsername)]
        [Remote("AllowUserName", "Users", AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        public string UserName { get; set; } = null!;

        [MaxLength(30, ErrorMessage = Errors.MaxLength), EmailAddress]
        [Remote("AllowEmail", "Users", AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        public string Email { get; set; } = null!;

        [Display(Name = "Roles")]
        public IList<string> SelectedRoles { get; set; } = new List<string>();
        public IEnumerable<SelectListItem>? DisplayRoles { get; set; }

        [DataType(DataType.Password), StringLength(100, ErrorMessage = Errors.MaxMinLength, MinimumLength = 8), RegularExpression(RegexPatterns.Password, ErrorMessage = Errors.WeakPassword)]
        [RequiredIf("Id == null", ErrorMessage = Errors.RequiredFiled)]
        public string? Password { get; set; }

        [DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = Errors.InvalidConfirmPassword), Display(Name = "Confirm password")]
        [RequiredIf("Id == null", ErrorMessage = Errors.RequiredFiled)]
        public string? ConfirmPassword { get; set; }
    }
}
