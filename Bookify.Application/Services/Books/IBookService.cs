namespace Bookify.Application.Services
{
    public interface IBookService
    {
        Book? GetById(int id);
        Book? GetWithCategories(int id);
        Book? GetWithCategoriesAndCopies(int id);
        IEnumerable<Book> GetLastAddedBooks(int numberOfBooks);
        IEnumerable<Author> GetAuthors();
        IEnumerable<Category> GetCategories();
        (IQueryable<Book> books, int count) GetFiltered(FiltrationDto dto);
        IQueryable<Book> GetDetails();
        IEnumerable<BookDto> GetTopBooks(int numberOfBooks);
        Book Add(Book book, IEnumerable<int> selectedCategories, string createdById);
        Book Update(Book book, IEnumerable<int> selectedCategories, string updatedById);
        bool AllowedItem(string title, int id, int authorId);
        Book? ToggleStatus(int id, string lastUpdatedById);
        IQueryable<Book> Search(string value);
        int GetActiveBooksCount();
    }
}
