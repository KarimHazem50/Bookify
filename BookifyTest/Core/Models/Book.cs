namespace BookifyTest.Core.Models
{
    [Index(nameof(Title), nameof(AuthorId), IsUnique = true)]
    public class Book : BaseModule
    {
        public int Id { get; set; }

        [MaxLength(500)]
        public string Title { get; set; } = null!;
        public int AuthorId { get; set; }
        public Author? Authors { get; set; }
        public string? ImageName { get; set; }
        public string? ImageThumbnailUrl { get; set; }

        [MaxLength(200)]
        public string Publisher { get; set; } = null!;
        public DateTime PublishingDate { get; set; }
        public bool IsAvailableForRental { get; set; }

        [MaxLength(50)]
        public string Hall { get; set; } = null!;

        public string Description { get; set; } = null!;
        public ICollection<BookCategory> Categories { get; set; } = new List<BookCategory>();
        public ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();
    }
}
