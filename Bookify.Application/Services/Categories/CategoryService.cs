namespace Bookify.Application.Services;
internal class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IEnumerable<Category> GetAll()
    {
        return _unitOfWork.Categories.GetAll();
    }

    public IEnumerable<CategoryDto> GetAll(string culture)
    {
        return _unitOfWork.Categories.GetAll(culture);
    }

    public IEnumerable<Category> GetActiveCategories()
    {
        return _unitOfWork.Categories.FindAll(predicate: c => !c.IsDeleted, orderBy: c => c.Name, OrderBy.Ascending);
    }

    public Category? GetById(int id)
    {
        return _unitOfWork.Categories.GetQueryable()
            .Include(c => c.Name)
            .ThenInclude(n => n.Localizations)
            .SingleOrDefault(c => c.Id == id);
    }

    public Category Add(IEnumerable<LocalizationDto> name, string createdById)
    {
        var category = new Category
        {
            Name = new LocalizationSet
            {
                Reference = $"{nameof(Category)}_{nameof(Category.Name)}",
                Localizations = name.Select(c => new Localization
                {
                    CultureCode = c.CultureCode,
                    Value = c.Value
                }).ToList()
            },
            CreatedById = createdById
        };

        _unitOfWork.Categories.Add(category);
        _unitOfWork.Complete();

        return category;
    }

    public Category? Update(int id, IEnumerable<LocalizationDto> name, string updatedById)
    {
        var category = GetById(id);

        if (category is null)
            return null;

        category.Name.Localizations = name.Select(c => new Localization
        {
            CultureCode = c.CultureCode,
            Value = c.Value
        }).ToList();
        category.LastUpdatedById = updatedById;
        category.LastUpdatedOn = DateTime.Now;

        _unitOfWork.Complete();

        return category;
    }

    public Category? ToggleStatus(int id, string updatedById)
    {
        var category = GetById(id);

        if (category is null)
            return null;

        category.IsDeleted = !category.IsDeleted;
        category.LastUpdatedById = updatedById;
        category.LastUpdatedOn = DateTime.Now;

        _unitOfWork.Complete();

        return category;
    }

    public bool AllowCategory(int id, string name, string cultureCode)
    {
        var category = _unitOfWork.Categories
            .Find(c => c.Name.Localizations.Any(x => x.Value == name && x.CultureCode == cultureCode));

        return category is null || category.Id.Equals(id);
    }
}