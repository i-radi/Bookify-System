namespace Bookify.Web.Core.ViewModels;

public class RentalCopiesViewModel
{
    public int SubscriberId { get; set; }
    public string SubscriberName { get; set; } = null!;
    public string SubscriberMobile { get; set; } = null!;
    public string BookTitle { get; set; } = null!;
    public string BookAuthor { get; set; } = null!;
    public string CopySerialNumber { get; set; } = null!;
    public DateTime RentalDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public DateTime? ExtendedOn { get; set; }

    public int DelayInDays
    {
        get
        {
            var delay = 0;

            if (ReturnDate.HasValue && ReturnDate.Value > EndDate)
                delay = (int)(ReturnDate.Value - EndDate).TotalDays;

            else if (!ReturnDate.HasValue && DateTime.Today > EndDate)
                delay = (int)(DateTime.Today - EndDate).TotalDays;

            return delay;
        }
    }
}