namespace Bookify.Application.Services
{
    public interface IRentalService
    {
        Rental? GetDetails(int id);
        IQueryable<Rental?> GetQueryableDetails(int id);
        int GetNumberOfCopies(int id);
        IEnumerable<KeyValuePairDto> GetRentalsPerDay(DateTime? startDate, DateTime? endDate);
        Rental Add(int subscriberId, ICollection<RentalCopy> copies, string createdById);
        Rental Update(int id, ICollection<RentalCopy> copies, string updatedById);
        void Return(Rental rental, IList<ReturnCopyDto> copies, bool penaltyPaid, string updatedById);
        bool AllowExtend(Rental rental, Subscriber subscriber);
        Rental? MarkAsDeleted(int id, string deletedById);
        string? ValidateExtendedCopies(Rental rental, Subscriber subscriber);
    }
}
