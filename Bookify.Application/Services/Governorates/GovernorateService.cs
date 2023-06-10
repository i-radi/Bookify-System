namespace Bookify.Application.Services;
internal class GovernorateService : IGovernorateService
{
    private readonly IUnitOfWork _unitOfWork;

    public GovernorateService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IEnumerable<Governorate> GetActiveGovernorates()
    {
        return _unitOfWork.Governorates.FindAll(predicate: a => !a.IsDeleted, orderBy: a => a.Name, orderByDirection: OrderBy.Ascending);
    }
}