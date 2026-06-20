using Carter;
using Microsoft.Extensions.DependencyInjection;
using Presentation.FileStorage;

namespace Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddCarter();
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}