using Bookify.Domain.Consts;
using Bookify.Domain.Enums;

namespace Bookify.Application.Services
{
    internal class SubscriberService : ISubscriberService
    {
        private readonly IUnitOFWork _unitOFWork;

        public SubscriberService(IUnitOFWork unitOFWork)
        {
            _unitOFWork = unitOFWork;
        }

        public Subscriber? GetById(int id)
        {
            return _unitOFWork.Subscribers.GetById(id);
        }
        public Subscriber? GetByIdWithSubscriptions(int id)
        {
            return _unitOFWork.Subscribers.Find(predicate: s => s.Id == id, include: s => s.Include(x => x.Subscriptions));
        }
        public Subscriber? GetByIdWithRentals(int id)
        {
            return _unitOFWork.Subscribers.Find(
                                             predicate: s => s.Id == id,
                                             include: x => x.Include(s => s.Subscriptions).Include(s => s.Rentals).ThenInclude(r => r.RentalCopies));
        }
        public Subscriber? GetSubscriberByMobileOrEmailOrNationalId(string value)
        {
            return _unitOFWork.Subscribers.Find(predicate: s => !s.IsDeleted && (s.MobileNumber == value || s.Email == value || s.NationalId == value));
        }
        public int GetActiveSubscribersCount()
        {
            return _unitOFWork.Subscribers.Count(c => !c.IsDeleted);
        }
        public IEnumerable<KeyValuePairDto> GetSubscribersPerCity()
        {
            return _unitOFWork.Subscribers.GetQueryable()
                .Include(s => s.Governorate)
                    .Where(s => !s.IsDeleted)
                    .GroupBy(s => new { GovernorateName = s.Governorate!.Name })
                    .Select(g => new KeyValuePairDto(
                        g.Key.GovernorateName,
                        g.Count().ToString()
                    ))
                    .ToList();
        }
        public Subscriber Add(Subscriber subscriber, string createdById)
        {
            subscriber.CreatedById = createdById;

            Subscription subscription = new()
            {
                CreatedById = subscriber.CreatedById,
                CreatedOn = subscriber.CreatedOn,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1)
            };

            subscriber.Subscriptions.Add(subscription);

            _unitOFWork.Subscribers.Add(subscriber);
            _unitOFWork.Complete();

            return subscriber;
        }
        public Subscriber Update(Subscriber subscriber, string lastupdatedById)
        {
            subscriber.LastUpdatedOn = DateTime.Now;
            subscriber.LastUpdatedById = lastupdatedById;
            _unitOFWork.Complete();

            return subscriber;
        }
        public bool AllowedEmail(string email, int id)
        {
            var subscriber = _unitOFWork.Subscribers.Find(predicate: s => s.Email == email);
            return subscriber is null || id == subscriber.Id;
        }
        public bool AllowedMobileNumber(string mobileNumber, int id)
        {
            var subscriber = _unitOFWork.Subscribers.Find(predicate: s => s.MobileNumber == mobileNumber);
            return subscriber is null || id == subscriber.Id;
        }
        public bool AllowedNationlID(string nationlID, int id)
        {
            var subscriber = _unitOFWork.Subscribers.Find(predicate: s => s.NationalId == nationlID);
            return subscriber is null || id == subscriber.Id;
        }
        public IQueryable<Subscriber> GetQueryableDetails()
        {
            return _unitOFWork.Subscribers.GetQueryable()
                .Include(s => s.Area)
                .Include(s => s.Governorate)
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies);
        }
        public Subscription RenewSubscription(Subscriber subscriber, string createdById)
        {
            var lastSubscription = subscriber.Subscriptions.Last();
            var startDate = lastSubscription.EndDate < DateTime.Today ? DateTime.Today : lastSubscription.EndDate.AddDays(1);

            Subscription newSubscription = new()
            {
                CreatedById = createdById,
                CreatedOn = DateTime.Now,
                StartDate = startDate,
                EndDate = startDate.AddYears(1)
            };

            subscriber.Subscriptions.Add(newSubscription);
            _unitOFWork.Complete();

            return newSubscription;
        }
        public (string errorMessage, int? maxAllowedCopies)? CanRent(int id, int? rentalId = null)
        {
            var subscriber = GetByIdWithRentals(id);

            if (subscriber is null)
                return null;

            if (subscriber.IsBlackListed)
                return (errorMessage: Errors.BlackListedSubscriber, maxAllowedCopies: null);

            if (subscriber.Subscriptions.Last().EndDate < DateTime.Today.AddDays((int)RentalsConfigurations.RentalDuration))
                return (errorMessage: Errors.InActiveSubscriber, maxAllowedCopies: null);

            var currentRentals = subscriber.Rentals.Where(r => rentalId == null || r.Id != rentalId).SelectMany(r => r.RentalCopies).Count(c => !c.ReturnDate.HasValue);

            var allowedRentals = (int)RentalsConfigurations.MaxAllowedCopies - currentRentals;

            if (allowedRentals.Equals(0))
                return (errorMessage: Errors.MaxCopiesReached, maxAllowedCopies: null);


            return (errorMessage: string.Empty, maxAllowedCopies: allowedRentals);
        }

    }
}
