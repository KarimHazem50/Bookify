namespace Bookify.Infrastructure.Persistence.Repositories
{
    internal class BookRepository : BaseRepository<Book>, IBookRepository
    {
        public BookRepository(ApplicationDbContext context) : base(context)
        {
        }

        public IQueryable<Book> GetBooks()
        {
            return _context.Books.Include(b => b.Authors).Include(b => b.Categories).ThenInclude(c => c.Category);
        }

        public IQueryable<Book> GetDetails()
        {
            return _context.Books.Include(b => b.Authors).Include(b => b.BookCopies).Include(b => b.Categories).ThenInclude(c => c.Category);
        }
    }
}
