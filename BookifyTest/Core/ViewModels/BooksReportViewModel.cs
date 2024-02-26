using BookifyTest.Core.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookifyTest.Core.ViewModels
{
    public class BooksReportViewModel
    {
        public IEnumerable<SelectListItem> DisplayAuthors { get; set; } = new List<SelectListItem>();

        [Display(Name = "Authors")]
        public IList<int> SelectedAuthors { get; set; } = new List<int>();

        public IEnumerable<SelectListItem> DisplayCategories { get; set; } = new List<SelectListItem>();

        [Display(Name = "Categories")]
        public IList<int> SelectedCategories { get; set; } = new List<int>();


        public PaginatedList<Book>? Books { get; set; }
    }
}
