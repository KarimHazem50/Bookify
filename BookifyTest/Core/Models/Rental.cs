namespace BookifyTest.Core.Models
{
    public class Rental : BaseModule
    {
        public int Id { get; set; }
        public int SubScriberId { get; set; }
        public Subscriber? Subscriber { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
        public bool PenaltyPaid { get; set; }

        public ICollection<RentalCopy> RentalCopies { get; set; } = new List<RentalCopy>();
    }
}
