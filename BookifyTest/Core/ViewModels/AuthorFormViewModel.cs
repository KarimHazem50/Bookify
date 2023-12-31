﻿namespace BookifyTest.Core.ViewModels
{
    public class AuthorFormViewModel
    {
        public int Id { get; set; }
        [MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Author"), RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
        [Remote("AllowedItem", "Authors", AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        public string Name { get; set; } = null!;
    }
}
