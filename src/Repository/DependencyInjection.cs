using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repository.Interceptors;
using Repository.Outbox;
using Repository.Persistence;
using Shared.Outbox;

namespace Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddRepository(this IServiceCollection services, string connectionString)
    {
        // Database
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options.UseMySQL(
                connectionString
                , o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            )
            .AddInterceptors(new ConvertDomainEventsToOutboxMessagesInterceptor());
        });

        services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

        services.AddScoped<IOutboxProcessor, OutboxMessagesProcessor>();

        return services;
    }

}