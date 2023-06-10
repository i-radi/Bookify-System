namespace Bookify.Application.Services;
public interface IAuthorService
{
    IEnumerable<Author> GetAll();
    IEnumerable<Author> GetActiveAuthors();
    Author? GetById(int id);
    Author Add(string name, string createdById);
    Author? Update(int id, string name, string updatedById);
    Author? ToggleStatus(int id, string updatedById);
    bool AllowAuthor(int id, string name);
}