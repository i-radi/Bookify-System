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

    public IEnumerable<Category> GetActiveCategories()
    {
        return _unitOfWork.Categories.FindAll(predicate: c => !c.IsDeleted, orderBy: c => c.Name, OrderBy.Ascending);
    }

    public Category? GetById(int id)
    {
        return _unitOfWork.Categories.GetById(id);
    }

    public Category Add(string name, string createdById)
    {
        var category = new Category
        {
            Name = name,
            CreatedById = createdById
        };

        _unitOfWork.Categories.Add(category);
        _unitOfWork.Complete();

        return category;
    }

    public Category? Update(int id, string name, string updatedById)
    {
        var category = GetById(id);

        if (category is null)
            return null;

        category.Name = name;
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

    public bool AllowCategory(int id, string name)
    {
        var category = _unitOfWork.Categories.Find(c => c.Name == name);
        return category is null || category.Id.Equals(id);
    }
}