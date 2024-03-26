using Bookify.Domain.Consts;
using Bookify.Domain.Enums;

namespace Bookify.Application.Services
{
    internal class RentalService : IRentalService
    {
        private readonly IUnitOFWork _unitOFWork;

        public RentalService(IUnitOFWork unitOFWork)
        {
            _unitOFWork = unitOFWork;
        }

        public Rental? GetDetails(int id)
        {
            return _unitOFWork.Rentals.GetQueryable().Include(r => r.RentalCopies).ThenInclude(c => c.BookCopy).SingleOrDefault(r => r.Id == id);
        }
        public IQueryable<Rental?> GetQueryableDetails(int id)
        {
            return _unitOFWork.Rentals.GetQueryable()
                    .Include(r => r.RentalCopies)
                    .ThenInclude(c => c.BookCopy)
                    .ThenInclude(c => c!.Book);
        }
        public int GetNumberOfCopies(int id)
        {
            return _unitOFWork.RentalCopies.Count(c => c.RentalId == id);
        }
        public IEnumerable<KeyValuePairDto> GetRentalsPerDay(DateTime? startDate, DateTime? endDate)
        {
            return _unitOFWork.RentalCopies.GetQueryable()
                    .Where(c => c.RentalDate >= startDate && c.RentalDate <= endDate)
                    .GroupBy(c => new { Date = c.RentalDate })
                    .Select(g => new KeyValuePairDto(
                        g.Key.Date.ToString("d MMM"),
                        g.Count().ToString()
                    ))
                    .ToList();
        }
        public Rental Add(int subscriberId, ICollection<RentalCopy> copies, string createdById)
        {
            var rental = new Rental()
            {
                SubScriberId = subscriberId,
                RentalCopies = copies,
                CreatedById = createdById
            };

            _unitOFWork.Rentals.Add(rental);
            _unitOFWork.Complete();

            return rental;
        }
        public Rental Update(int id, ICollection<RentalCopy> copies, string updatedById)
        {
            var rental = _unitOFWork.Rentals.GetById(id);

            rental!.RentalCopies = copies;
            rental.LastUpdatedById = updatedById;
            rental.LastUpdatedOn = DateTime.Now;

            _unitOFWork.Complete();

            return rental;
        }
        public void Return(Rental rental, IList<ReturnCopyDto> copies, bool penaltyPaid, string updatedById)
        {
            var isUpdated = false;

            foreach (var copy in copies)
            {
                if (!copy.IsReturned.HasValue) continue;

                var currentCopy = rental.RentalCopies.SingleOrDefault(c => c.BookCopyId == copy.Id);

                if (currentCopy is null) continue;

                if (copy.IsReturned.HasValue && copy.IsReturned.Value)
                {
                    if (currentCopy.ReturnDate.HasValue) continue;

                    currentCopy.ReturnDate = DateTime.Now;
                    isUpdated = true;
                }

                if (copy.IsReturned.HasValue && !copy.IsReturned.Value)
                {
                    if (currentCopy.ExtendedOn.HasValue) continue;

                    currentCopy.ExtendedOn = DateTime.Now;
                    currentCopy.EndDate = currentCopy.RentalDate.AddDays((int)RentalsConfigurations.RentalDuration * 2);
                    isUpdated = true;
                }
            }

            if (isUpdated)
            {
                rental.LastUpdatedOn = DateTime.Now;
                rental.LastUpdatedById = updatedById;
                rental.PenaltyPaid = penaltyPaid;

                _unitOFWork.Complete();
            }
        }
        public bool AllowExtend(Rental rental, Subscriber subscriber)
        {
            return !subscriber.IsBlackListed
                        && subscriber!.Subscriptions.Last().EndDate >= rental.StartDate.AddDays((int)RentalsConfigurations.RentalDuration)
                        && rental.StartDate.AddDays((int)RentalsConfigurations.RentalDuration * 2) >= DateTime.Today;
        }
        public Rental? MarkAsDeleted(int id, string deletedById)
        {
            var rental = _unitOFWork.Rentals.GetById(id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return null;

            rental.IsDeleted = true;
            rental.LastUpdatedOn = DateTime.Now;
            rental.LastUpdatedById = deletedById;

            _unitOFWork.Complete();

            return rental;
        }
        public string? ValidateExtendedCopies(Rental rental, Subscriber subscriber)
        {
            string error = string.Empty;

            if (subscriber!.IsBlackListed)
                error = Errors.BlackListedSubscriber;

            else if (subscriber!.Subscriptions.Last().EndDate < rental.StartDate.AddDays((int)RentalsConfigurations.RentalDuration * 2))
                error = Errors.RentalNotAllowedForInactive;

            else if (rental.StartDate.AddDays((int)RentalsConfigurations.RentalDuration) < DateTime.Today)
                error = Errors.ExtendNotAllowed;

            return error;
        }

    }
}
