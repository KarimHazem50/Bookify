namespace Bookify.Application.Common.Interfaces
{
    public interface IBookRepository : IBaseRepository<Book>
    {
        IQueryable<Book> GetBooks();
        IQueryable<Book> GetDetails();
    }
}
