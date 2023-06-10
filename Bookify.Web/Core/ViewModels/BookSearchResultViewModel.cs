namespace Bookify.Web.Core.ViewModels;

public class BookSearchResultViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Author { get; set; } = null!;
    public string Key { get; set; } = null!;
}