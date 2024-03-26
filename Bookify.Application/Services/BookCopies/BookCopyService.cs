using Bookify.Domain.Consts;

namespace Bookify.Application.Services
{
    internal class BookCopyService : IBookCopyService
    {
        private readonly IUnitOFWork _unitOFWork;

        public BookCopyService(IUnitOFWork unitOFWork)
        {
            _unitOFWork = unitOFWork;
        }

        public BookCopy? GetById(int id)
        {
            return _unitOFWork.BookCopies.Find(c => c.Id == id, include: c => c.Include(x => x.Book)!);
        }
        public Book? GetBookById(int bookId)
        {
            var book = _unitOFWork.Books.GetById(bookId);

            if (book is null)
                return null;
            else
                return book;
        }
        public IEnumerable<BookCopy> GetRentalCopies(IEnumerable<int> copies)
        {
            return _unitOFWork.BookCopies.FindAll(
                    predicate: c => copies.Contains(c.Id),
                    include: c => c.Include(x => x.Book)!
                );
        }
        public BookCopy? GetActiveCopyBySerialNumber(string serialNumber)
        {
            return _unitOFWork.BookCopies
                        .Find(predicate: c => c.SerialNumber.ToString() == serialNumber && !c.IsDeleted && !c.Book!.IsDeleted,
                              include: c => c.Include(x => x.Book)!);
        }
        public BookCopy? Add(int bookId, int editionNumber, bool isAvailableForRental, string createdById)
        {
            var book = GetBookById(bookId);

            if (book is null)
                return null;

            var copy = new BookCopy
            {
                EditionNumber = editionNumber,
                IsAvailableForRental = book.IsAvailableForRental ? isAvailableForRental : false,
                CreatedById = createdById,
            };

            book.BookCopies.Add(copy);
            _unitOFWork.Complete();

            return copy;
        }
        public BookCopy? Update(int id, int editionNumber, bool isAvailableForRental, string updatedById)
        {
            var copy = GetById(id);

            if (copy is null)
                return null;

            copy.EditionNumber = editionNumber;
            copy.IsAvailableForRental = copy.Book!.IsAvailableForRental ? isAvailableForRental : false;
            copy.LastUpdatedOn = DateTime.Now;
            copy.LastUpdatedById = updatedById;
            _unitOFWork.Complete();

            return copy;
        }
        public BookCopy? ToggleStatus(int id, string updatedById)
        {
            var copy = GetById(id);

            if (copy is null)
                return null;

            copy.IsDeleted = !copy.IsDeleted;
            copy.LastUpdatedOn = DateTime.Now;
            copy.LastUpdatedById = updatedById;
            _unitOFWork.Complete();

            return copy;
        }
        public IEnumerable<RentalCopy> GetRentalHistory(int copyId)
        {
            return _unitOFWork.RentalCopies.FindAll(predicate: c => c.BookCopyId == copyId,
                                                     include: c => c.Include(x => x.Rental)!.ThenInclude(r => r!.Subscriber)!,
                                                     orderBy: c => c.RentalDate,
                                                     orderByDirection: OrderBy.Descending);
        }
        public (string errorMessage, ICollection<RentalCopy> copies) CanBeRented(IEnumerable<int> selectedSerials, int subscriberId, int? rentalId = null)
        {
            var selectedCopies = _unitOFWork.BookCopies.FindAll(
                                                        predicate: c => selectedSerials.Contains(c.SerialNumber),
                                                        include: c => c.Include(c => c.Book).Include(c => c.RentalCopies));

            var query = _unitOFWork.Rentals.GetQueryable();

            var currentSubscriberRentals = query
                                               .Include(r => r.RentalCopies)
                                               .ThenInclude(c => c.BookCopy)
                                               .Where(r => r.SubScriberId == subscriberId && (rentalId == null || r.Id != rentalId))
                                               .SelectMany(r => r.RentalCopies)
                                               .Where(c => !c.ReturnDate.HasValue)
                                               .Select(c => c.BookCopy!.BookId)
                                               .ToList();

            List<RentalCopy> copies = new();

            foreach (var copy in selectedCopies)
            {
                if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                    return (errorMessage: Errors.NotAvailableForRental, copies);

                if (copy.RentalCopies.Any(c => !c.ReturnDate.HasValue && (rentalId == null || c.RentalId != rentalId)))
                    return (errorMessage: Errors.CopyIsInRental, copies);

                if (currentSubscriberRentals.Any(bookId => bookId == copy.BookId))
                    return (errorMessage: $"This subscriber already has a copy for '{copy.Book.Title}' Book", copies);

                copies.Add(new RentalCopy { BookCopyId = copy.Id });
            }

            return (errorMessage: string.Empty, copies);
        }
        public bool CopyIsInRental(int id)
        {
            return _unitOFWork.RentalCopies.IsExists(c => c.BookCopyId == id && !c.ReturnDate.HasValue);
        }
    }
}
