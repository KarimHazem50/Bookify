namespace Bookify.Web.Core.ViewModels
{
    public class BookRowViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string AuthorName { get; set; } = null!;
        public string? ImageName { get; set; }
        public IEnumerable<string> CategoriesNames { get; set; } = null!;
        public string Publisher { get; set; } = null!;
        public DateTime PublishingDate { get; set; }
        public string Hall { get; set; } = null!;
        public bool IsAvailableForRental { get; set; }
        public bool IsDeleted { get; set; }
    }
}
