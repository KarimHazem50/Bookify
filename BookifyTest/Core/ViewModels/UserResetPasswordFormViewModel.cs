namespace BookifyTest.Core.ViewModels
{
    public class UserResetPasswordFormViewModel
    {
        public string Id { get; set; } = null!;
        [DataType(DataType.Password), StringLength(100, ErrorMessage = Errors.MaxMinLength, MinimumLength = 8), RegularExpression(RegexPatterns.Password, ErrorMessage = Errors.WeakPassword)]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = Errors.InvalidConfirmPassword), Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
