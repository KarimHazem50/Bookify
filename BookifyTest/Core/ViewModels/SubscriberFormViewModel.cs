using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace BookifyTest.Core.ViewModels
{
    public class SubscriberFormViewModel
    {
        public string? Key { get; set; }

        [MaxLength(30, ErrorMessage = Errors.MaxLength), Display(Name = "First Name"), 
            RegularExpression(RegexPatterns.DenySpecialCharacters, ErrorMessage = Errors.DenySpecialCharacters)]
        public string FirstName { get; set; } = null!;

        [MaxLength(30, ErrorMessage = Errors.MaxLength), Display(Name = "Last Name"), 
            RegularExpression(RegexPatterns.DenySpecialCharacters, ErrorMessage = Errors.DenySpecialCharacters)]
        public string LastName { get; set; } = null!;

        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [MaxLength(14, ErrorMessage = Errors.MaxLength), Display(Name = "National ID"), 
            RegularExpression(RegexPatterns.NationalNumber, ErrorMessage = Errors.InvalidNationalNumber)]
        [Remote("AllowedNationlID", "Subscribers", AdditionalFields = "Key", ErrorMessage = Errors.Duplicated)]
        public string NationalId { get; set; } = null!;

        [MaxLength(11, ErrorMessage = Errors.MaxLength), Display(Name = "Mobile Number"), RegularExpression(RegexPatterns.MobileNumber, ErrorMessage = Errors.InvalidMobileNumber)]
        [Remote("AllowedMobileNumber", "Subscribers", AdditionalFields = "Key", ErrorMessage = Errors.Duplicated)]
        public string MobileNumber { get; set; } = null!;

        [Display(Name = "Has WhatsApp?")]
        public bool HasWhatsApp { get; set; }

        [MaxLength(60, ErrorMessage = Errors.MaxLength), EmailAddress]
        [Remote("AllowedEmail", "Subscribers", AdditionalFields = "Key", ErrorMessage = Errors.Duplicated)]
        public string Email { get; set; } = null!;

        [MaxLength(500, ErrorMessage = Errors.MaxLength)]
        public string? ImageName { get; set; }

        [RequiredIf("Key == ''", ErrorMessage = Errors.EmptyImage)]
        public IFormFile? Image { get; set; }


        public int AreaId { get; set; }
        public IEnumerable<SelectListItem>? DisplayAreas { get; set; }
        public int GovernorateId { get; set; }
        public IEnumerable<SelectListItem>? DisplayGovernorates { get; set; }

        [MaxLength(500, ErrorMessage = Errors.MaxLength), 
            RegularExpression(RegexPatterns.DenySpecialCharacters, ErrorMessage = Errors.DenySpecialCharacters)]
        public string Address { get; set; } = null!;
    }
}