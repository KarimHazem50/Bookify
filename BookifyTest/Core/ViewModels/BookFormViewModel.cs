using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace BookifyTest.Core.ViewModels
{
    public class BookFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(500, ErrorMessage = Errors.MaxLength)]
        [Remote("AllowedItem", "Books", AdditionalFields = "Id, AuthorId", ErrorMessage = Errors.DuplicatedBook)]
        public string Title { get; set; } = null!;

        [Display(Name = "Author")]
        [Remote("AllowedItem", "Books", AdditionalFields = "Id, Title", ErrorMessage = Errors.DuplicatedBook)]
        public int AuthorId { get; set; }
        public IEnumerable<SelectListItem>? DisplayAuthors { get; set; }
        public IFormFile? Image { get; set; }
        public string? ImageName { get; set; }

        [MaxLength(200, ErrorMessage = Errors.MaxLength)]
        public string Publisher { get; set; } = null!;

        [Display(Name = "Publishing Date")]

        [AssertThat("PublishingDate <= Today()")]
        public DateTime PublishingDate { get; set; } = DateTime.Now;

        [Display(Name = "Is available for rental?")]
        public bool IsAvailableForRental { get; set; }

        [MaxLength(50, ErrorMessage = Errors.MaxLength)]
        public string Hall { get; set; } = null!;
        public string Description { get; set; } = null!;

        [Display(Name = "Categories")]
        public IList<int> SelectedCategories { get; set; } = new List<int>();
        public IEnumerable<SelectListItem>? DisplayCategories { get; set; }
    }
}
