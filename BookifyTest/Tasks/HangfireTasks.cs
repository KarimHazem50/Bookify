using Microsoft.AspNetCore.Identity.UI.Services;
using WhatsAppCloudApi;
using WhatsAppCloudApi.Services;

namespace Bookify.Web.Tasks
{
    public class HangfireTasks
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;
        private readonly IWhatsAppClient _whatsAppClient;
        public HangfireTasks(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IEmailBodyBuilder emailBodyBuilder, IEmailSender emailSender, IWhatsAppClient whatsAppClient)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
            _whatsAppClient = whatsAppClient;
        }
        public async Task PrepareExpirationAlert()
        {
            var subscribers = _context.Subscribers
                                                .Include(s => s.Subscriptions)
                                                .Where(s => !s.IsBlackListed && s.Subscriptions.OrderByDescending(x => x.EndDate).First().EndDate == DateTime.Today.AddDays(5)).ToList();

            foreach (var subscriber in subscribers)
            {
                var endDate = subscriber.Subscriptions.Last().EndDate.ToString("d MMM yyyy");
                // Send Mail Messaga
                var placehoders = new Dictionary<string, string>()
                {
                    {"imageUrl", "https://res.cloudinary.com/devcreed/image/upload/v1671062674/calendar_zfohjc.png"},
                    {"header", $"Hello {subscriber.FirstName}"},
                    {"body", $"Your subscription will be expired by {endDate} 😔"},
                };
                var body = _emailBodyBuilder.GetEmailBody(template: EmailTemplates.Notification, placehoders: placehoders);
                await _emailSender.SendEmailAsync(subscriber.Email, "Bookify Subscription Expiration", body);


                // Send WhatsApp Message
                if (subscriber.HasWhatsApp)
                {
                    var components = new List<WhatsAppComponent>()
                    {
                        new WhatsAppComponent
                        {
                            Type = "body",
                            Parameters = new List<object>()
                            {
                                new WhatsAppTextParameter
                                {
                                    Text = subscriber.FirstName
                                },
                                new WhatsAppTextParameter
                                {
                                    Text = endDate
                                },
                            }
                        }
                    };
                    await _whatsAppClient.SendMessage(_webHostEnvironment.IsDevelopment() ? "201027453613" : $"2{subscriber.MobileNumber}", WhatsAppLanguageCode.English_US, WhatsAppTemplates.SubscriptionExpiration, components);
                }
            }
        }


        public async Task RentalExpirationAlert()
        {
            var tomorrow = DateTime.Today.AddDays(1);
            var rentals = _context.Rentals
                                        .Include(r => r.RentalCopies).ThenInclude(rc => rc.BookCopy).ThenInclude(c => c!.Book)
                                        .Include(r => r.Subscriber)
                                        .Where(r => r.RentalCopies.Any(rc => rc.EndDate == tomorrow && !rc.ReturnDate.HasValue))
                                        .ToList();
            foreach (var rental in rentals)
            {
                var copiesWillBeExpired = rental.RentalCopies.Where(rc => rc.EndDate == tomorrow && !rc.ReturnDate.HasValue).Select(rc => rc.BookCopy!.Book!.Title).ToArray();
                var copiesExpired = string.Join(",\n", copiesWillBeExpired);
                // Send Mail Messaga
                var placehoders = new Dictionary<string, string>()
                {
                    {"imageUrl", "https://res.cloudinary.com/devcreed/image/upload/v1671062674/calendar_zfohjc.png"},
                    {"header", $"Hello {rental.Subscriber!.FirstName}"},
                    {"body", $"Your rental for this Book(s) \n" +
                        $"{copiesExpired}" +
                        $" will be expired by {tomorrow.ToString("d MMM yyyy")} 😔"},
                };
                var body = _emailBodyBuilder.GetEmailBody(template: EmailTemplates.Notification, placehoders: placehoders);
                await _emailSender.SendEmailAsync(rental.Subscriber.Email, "Bookify Rental Expiration", body);


                // Send WhatsApp Message
                if (rental.Subscriber.HasWhatsApp)
                {
                    var components = new List<WhatsAppComponent>()
                    {
                        new WhatsAppComponent
                        {
                            Type = "body",
                            Parameters = new List<object>()
                            {
                                new WhatsAppTextParameter
                                {
                                    Text = rental.Subscriber.FirstName
                                },
                                new WhatsAppTextParameter
                                {
                                    Text = copiesExpired
                                },
                                new WhatsAppTextParameter
                                {
                                    Text = tomorrow.ToString("d MMM yyyy")
                                },
                            }
                        }
                    };
                    await _whatsAppClient.SendMessage(_webHostEnvironment.IsDevelopment() ? "201027453613" : $"2{rental.Subscriber.MobileNumber}", WhatsAppLanguageCode.English_US, WhatsAppTemplates.RentalExpiration, components);
                }
            }
        }
    }
}