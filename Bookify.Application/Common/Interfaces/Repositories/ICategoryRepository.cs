namespace Bookify.Application.Common.Interfaces.Repositories;
public interface ICategoryRepository : IBaseRepository<Category>
{
    IEnumerable<CategoryDto> GetAll(string culture);
}