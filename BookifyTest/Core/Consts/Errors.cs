namespace Bookify.Web.Core.Consts
{
    public static class Errors
    {
        public const string RequiredFiled = "Required filed";
        public const string MaxLength = "Length cannot more than {1} characters";
        public const string MinLength = "Length cannot less than {1} characters";
        public const string MaxMinLength = "The {0} must be at least {2} and at max {1} characters long.";
        public const string Duplicated = "Another record with the same {0} is already exists!";
        public const string NotAllowedExtension = "only .png, .jpeg, .jpg files are allowed!";
        public const string Maxsize = "size cannot be more than 2 MB!";
        public const string DuplicatedBook = "Book with the same title with the same author is already exists!";
        public const string InvalidRange = "{0} should be between {1} and {2}!";
        public const string InvalidConfirmPassword = "The password and confirmation password do not match.";
        public const string WeakPassword = "Passwords contain an uppercase character, lowercase character, a digit, and a non-alphanumeric character. Passwords must be at least 8 characters long";
        public const string InvalidUsername = "Username can only contain letters or digits.";
        public const string OnlyEnglishLetters = "Only English letters are allowed.";
        public const string OnlyArabicLetters = "Only Arabic letters are allowed.";
        public const string OnlyNumbersAndLetters = "Only Arabic/English letters or digits are allowed.";
        public const string DenySpecialCharacters = "Special characters are not allowed.";
        public const string InvalidMobileNumber = "Invalid mobile number!";
        public const string InvalidNationalNumber = "Invalid national ID!";
        public const string InvalidSerialNumber = "Invalid serial number!";
        public const string NotAvailableForRental = "This book/copy is not available for rental.";
        public const string EmptyImage = "Please select an image.";
        public const string BlackListedSubscriber = "This subscriber is blacklisted.";
        public const string InActiveSubscriber = "This subscriber is inactive.";
        public const string MaxCopiesReached = "This subscriber has reached the max number for rentals.";
        public const string CopyIsInRental = "This copy is already rentaled.";
        public const string RentalNotAllowedForBlackListed = "Rental cannot extended be for blacklisted subscriber.";
        public const string RentalNotAllowedForInactive = "Rental cannot be extended for this subscriber before renwal.";
        public const string ExtendNotAllowed = "Rental cannot be extended.";
        public const string PenaltyShouldBePaid = "Penalty should be paid.";
    }
}
