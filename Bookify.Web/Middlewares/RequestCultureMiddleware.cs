namespace Bookify.Web.Middlewares;

public class RequestCultureMiddleware
{
    private readonly RequestDelegate _next;

    public RequestCultureMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var currentLanguage = context.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
        var browserLanguage = context.Request.Headers["Accept-Language"].ToString()[..2];

        if (string.IsNullOrEmpty(currentLanguage))
        {
            var culture = string.Empty;

            switch (browserLanguage)
            {
                case "ar":
                    culture = "ar-EG";
                    break;

                default:
                    culture = "en-US";
                    break;
            }

            var requestCulture = new RequestCulture(culture, culture);
            context.Features.Set<IRequestCultureFeature>(new RequestCultureFeature(requestCulture, null));

            CultureInfo.CurrentCulture = new CultureInfo(culture);
            CultureInfo.CurrentUICulture = new CultureInfo(culture);
        }

        await _next(context);
    }
}