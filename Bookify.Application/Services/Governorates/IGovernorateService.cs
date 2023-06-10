namespace Bookify.Application.Services;
public interface IGovernorateService
{
    IEnumerable<Governorate> GetActiveGovernorates();
}