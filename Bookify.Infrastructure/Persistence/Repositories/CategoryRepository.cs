using Bookify.Domain.Dtos;

namespace Bookify.Infrastructure.Persistence.Repositories;
internal class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public IEnumerable<CategoryDto> GetAll(string culture)
    {
        var categories = (from c in _context.Categories
                          join s in _context.LocalizationSets
                          on c.NameId equals s.Id
                          join l in _context.Localizations
                          on s.Id equals l.LocalizationSetId
                          where l.CultureCode == culture
                          select new CategoryDto(
                              c.Id,
                              l.Value,
                              c.IsDeleted,
                              c.CreatedOn,
                              c.LastUpdatedOn))
                              .AsNoTracking()
                              .ToList();

        return categories;
    }
}