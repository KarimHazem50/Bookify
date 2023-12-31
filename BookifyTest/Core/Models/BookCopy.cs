﻿namespace BookifyTest.Core.Models
{
    public class BookCopy : BaseModule
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public Book? Book { get; set; }
        public bool IsAvailableForRental { get; set; }
        public int EditionNumber { get; set; }
        public int SerialNumber { get; set; }
    }
}
