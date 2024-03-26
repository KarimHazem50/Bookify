namespace Bookify.Application.Services
{
    public interface ISubscriberService
    {
        Subscriber? GetById(int id);
        Subscriber? GetByIdWithSubscriptions(int id);
        Subscriber? GetByIdWithRentals(int id);
        Subscriber? GetSubscriberByMobileOrEmailOrNationalId(string value);
        int GetActiveSubscribersCount();
        IEnumerable<KeyValuePairDto> GetSubscribersPerCity();
        Subscriber Add(Subscriber subscriber, string createdById);
        Subscriber Update(Subscriber subscriber, string lastupdatedById);
        bool AllowedEmail(string email, int id);
        bool AllowedMobileNumber(string mobileNumber, int id);
        bool AllowedNationlID(string nationlID, int id);
        IQueryable<Subscriber> GetQueryableDetails();
        Subscription RenewSubscription(Subscriber subscriber, string createdById);
        (string errorMessage, int? maxAllowedCopies)? CanRent(int id, int? rentalId = null);
    }
}
