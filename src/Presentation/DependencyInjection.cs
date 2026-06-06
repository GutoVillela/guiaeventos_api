using Carter;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;


namespace Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddCarter();

        var assembly = typeof(DependencyInjection).Assembly;

        return services;
    }
}