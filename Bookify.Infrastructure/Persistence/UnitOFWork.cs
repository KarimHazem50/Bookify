using Bookify.Infrastructure.Persistence.Repositories;

namespace Bookify.Infrastructure.Persistence
{
    internal class UnitOFWork : IUnitOFWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOFWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IBaseRepository<Area> Areas => new BaseRepository<Area>(_context);
        public IBaseRepository<Author> Authors => new BaseRepository<Author>(_context);
        public IBookRepository Books => new BookRepository(_context);
        public IBaseRepository<BookCopy> BookCopies => new BaseRepository<BookCopy>(_context);
        public IBaseRepository<Category> Categories => new BaseRepository<Category>(_context);
        public IBaseRepository<RentalCopy> RentalCopies => new BaseRepository<RentalCopy>(_context);
        public IBaseRepository<Rental> Rentals => new BaseRepository<Rental>(_context);
        public IBaseRepository<Subscriber> Subscribers => new BaseRepository<Subscriber>(_context);
        public IBaseRepository<Governorate> Governorates => new BaseRepository<Governorate>(_context);
        public int Complete()
        {
            return _context.SaveChanges();
        }
    }
}
