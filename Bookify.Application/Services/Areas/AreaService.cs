namespace Bookify.Application.Services;
internal class AreaService : IAreaService
{
    private readonly IUnitOfWork _unitOfWork;

    public AreaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }


    public IEnumerable<Area> GetActiveAreasByGovernorateId(int id)
    {
        return _unitOfWork.Areas.FindAll(
            predicate: a => a.GovernorateId == id && !a.IsDeleted,
            orderBy: a => a.Name,
            orderByDirection: OrderBy.Ascending
        );
    }
}