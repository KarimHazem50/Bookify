namespace Bookify.Application.Common.Interfaces
{
    public interface IUnitOFWork
    {
        IBaseRepository<Area> Areas { get; }
        IBaseRepository<Author> Authors { get; }
        IBookRepository Books { get; }
        IBaseRepository<BookCopy> BookCopies { get; }
        IBaseRepository<Category> Categories { get; }
        IBaseRepository<RentalCopy> RentalCopies { get; }
        IBaseRepository<Rental> Rentals { get; }
        IBaseRepository<Subscriber> Subscribers { get; }
        IBaseRepository<Governorate> Governorates { get; }
        int Complete();
    }
}
