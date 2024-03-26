namespace Bookify.Application.Services
{
    public interface IBookCopyService
    {
        public BookCopy? GetById(int id);
        public Book? GetBookById(int bookId);
        IEnumerable<BookCopy> GetRentalCopies(IEnumerable<int> copies);
        BookCopy? GetActiveCopyBySerialNumber(string serialNumber);
        public BookCopy? Add(int bookId, int editionNumber, bool isAvailableForRental, string createdById);
        public BookCopy? Update(int id, int editionNumber, bool isAvailableForRental, string updatedById);
        public BookCopy? ToggleStatus(int id, string updatedById);
        public IEnumerable<RentalCopy>? GetRentalHistory(int copyId);
        (string errorMessage, ICollection<RentalCopy> copies) CanBeRented(IEnumerable<int> selectedSerials, int subscriberId, int? rentalId = null);
        bool CopyIsInRental(int id);
    }
}
