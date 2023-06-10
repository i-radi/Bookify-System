namespace Bookify.Application.Services;
public interface IAreaService
{
    IEnumerable<Area> GetActiveAreasByGovernorateId(int id);
}