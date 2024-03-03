﻿namespace Bookify.Web.Core.ViewModels
{
    public class SubscriberDetailsViewModel
    {
        public int Id { get; set; }
        public string? Key { get; set; }
        public string FullName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string NationalId { get; set; } = null!;
        public string MobileNumber { get; set; } = null!;
        public bool HasWhatsApp { get; set; }
        public string Email { get; set; } = null!;
        public string ImageName { get; set; } = null!;

        public string AreaName { get; set; } = null!;
        public string GovernorateName { get; set; } = null!;

        public string Address { get; set; } = null!;

        public bool IsBlackListed { get; set; }

        public DateTime CreatedOn { get; set; }

        public IEnumerable<SubscriptionViewModel> Subscriptions { get; set; } = new List<SubscriptionViewModel>();
        public IEnumerable<RentalViewModel> Rentals { get; set; } = new List<RentalViewModel>();
    }
}
