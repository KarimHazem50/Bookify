using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModels
{
    public class RentalsReportViewModel
    {
        [RequiredIf("Rentals == null", ErrorMessage = Errors.RequiredFiled)]
        public DateTime? Duration { get; set; }
        public PaginatedList<RentalCopy>? Rentals { get; set; }

    }
}
