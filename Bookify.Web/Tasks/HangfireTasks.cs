using Microsoft.AspNetCore.Identity.UI.Services;


namespace Bookify.Web.Tasks
{
    public class HangfireTasks
    {
        private readonly ISubscriberService _subscriberService;
        private readonly IRentalService _rentalService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWhatsAppClient _whatsAppClient;

        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;

        public HangfireTasks(
            ISubscriberService subscriberService,
            IRentalService rentalService,
            IWebHostEnvironment webHostEnvironment,
            IWhatsAppClient whatsAppClient,
            IEmailBodyBuilder emailBodyBuilder,
            IEmailSender emailSender)
        {
            _subscriberService = subscriberService;
            _rentalService = rentalService;
            _webHostEnvironment = webHostEnvironment;
            _whatsAppClient = whatsAppClient;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
        }
        public async Task PrepareExpirationAlert()
        {
            var subscribers = _subscriberService.GetExpired(expiredWithin: 5);

            foreach (var subscriber in subscribers)
            {
                var endDate = subscriber.Subscriptions.Last().EndDate.ToString("d MMM, yyyy");

                //Send email and WhatsApp Message
                var placeholders = new Dictionary<string, string>()
                {
                    { "imageUrl", "https://res.cloudinary.com/devcreed/image/upload/v1671062674/calendar_zfohjc.png" },
                    { "header", $"Hello {subscriber.FirstName}," },
                    { "body", $"your subscription will be expired by {endDate} 🙁" }
                };

                var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

                await _emailSender.SendEmailAsync(
                    subscriber.Email,
                    "Bookify Subscription Expiration", body);

                if (subscriber.HasWhatsApp)
                {
                    var components = new List<WhatsAppComponent>()
                    {
                        new WhatsAppComponent
                        {
                            Type = "body",
                            Parameters = new List<object>()
                            {
                                new WhatsAppTextParameter { Text = subscriber.FirstName },
                                new WhatsAppTextParameter { Text = endDate },
                            }
                        }
                    };

                    var mobileNumber = _webHostEnvironment.IsDevelopment() ? "Add Your number" : subscriber.MobileNumber;

                    //Change 2 with your country code
                    await _whatsAppClient
                        .SendMessage($"2{mobileNumber}", WhatsAppLanguageCode.English,
                        WhatsAppTemplates.SubscriptionExpiration, components);
                }
            }
        }

        public async Task RentalsExpirationAlert()
        {
            var tomorrow = DateTime.Today.AddDays(1);

            var rentals = _rentalService.GetExpired(tomorrow);

            foreach (var rental in rentals)
            {
                var expiredCopies = rental.RentalCopies.Where(c => c.EndDate.Date == tomorrow && !c.ReturnDate.HasValue).ToList();

                var message = $"your rental for the below book(s) will be expired by tomorrow {tomorrow.ToString("dd MMM, yyyy")} 💔:";
                message += "<ul>";

                foreach (var copy in expiredCopies)
                {
                    message += $"<li>{copy.BookCopy!.Book!.Title}</li>";
                }

                message += "</ul>";

                var placeholders = new Dictionary<string, string>()
                {
                    { "imageUrl", "https://res.cloudinary.com/devcreed/image/upload/v1671062674/calendar_zfohjc.png" },
                    { "header", $"Hello {rental.Subscriber!.FirstName}," },
                    { "body", message }
                };

                var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

                await _emailSender.SendEmailAsync(
                    rental.Subscriber!.Email,
                    "Bookify Rental Expiration 🔔", body);
            }
        }
    }
}