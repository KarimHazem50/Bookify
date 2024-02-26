using BookifyTest.Core.Utilities;

namespace BookifyTest.Core.ViewModels
{
    public class DelayedRentalsReportViewModel
    {
        public PaginatedList<RentalCopy> DelayedRentals { get; set; } = null!;
    }
}
