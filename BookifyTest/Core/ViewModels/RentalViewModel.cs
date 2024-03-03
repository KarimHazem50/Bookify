namespace Bookify.Web.Core.ViewModels
{
    public class RentalViewModel
    {
        public int Id { get; set; }
        public SubscriberDetailsViewModel? Subscriber { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public bool PenaltyPaid { get; set; }

        public IEnumerable<RentalCopyViewModel> RentalCopies { get; set; } = new List<RentalCopyViewModel>();


        public int TotalDelayInDays
        {
            get
            {
                return RentalCopies.Sum(c => c.DelayInDays);
            }
        }
        public int NumbersOfCopies
        {
            get
            {
                return RentalCopies.Count();
            }
        }
    }
}
