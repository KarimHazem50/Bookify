namespace BookifyTest.Core.ViewModels
{
    public class BookCopyViewModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string? ImageName { get; set; }
        public string BookTitle { get; set; } = null!;
        public bool IsAvailableForRental { get; set; }
        public bool IsAvailableForRentalForMainBook { get; set; }
        public int EditionNumber { get; set; }
        public int SerialNumber { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
