using Bookify.Web.Core.Utilities;

namespace Bookify.Web.Core.ViewModels
{
    public class DelayedRentalsReportViewModel
    {
        public PaginatedList<RentalCopy> DelayedRentals { get; set; } = null!;
    }
}
