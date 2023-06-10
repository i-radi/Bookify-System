using Bookify.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bookify.Application;
public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAreaService, AreaService>();
        services.AddScoped<IAuthorService, AuthorService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IBookCopyService, BookCopyService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IGovernorateService, GovernorateService>();
        services.AddScoped<IRentalService, RentalService>();
        services.AddScoped<ISubscriberService, SubscriberService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}