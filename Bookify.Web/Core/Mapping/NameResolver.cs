namespace Bookify.Web.Core.Mapping;

public class NameResolver : IValueResolver<object, object, string>
{
    public string Resolve(object source, object destination, string destMember, 
        ResolutionContext context)
    {
        return BaseLocalizationResolver.GetValue("Name", source);
    }
}