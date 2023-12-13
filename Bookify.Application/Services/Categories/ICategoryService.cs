namespace Bookify.Application.Services;
public interface ICategoryService
{
    IEnumerable<Category> GetAll();
    IEnumerable<CategoryDto> GetAll(string culture);
    IEnumerable<Category> GetActiveCategories();
    Category? GetById(int id);
    Category Add(IEnumerable<LocalizationDto> name, string createdById);
    Category? Update(int id, IEnumerable<LocalizationDto> name, string updatedById);
    Category? ToggleStatus(int id, string updatedById);
    bool AllowCategory(int id, string name, string cultureCode);
}