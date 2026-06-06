using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repository.Persistence;

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
            );
        });

        services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

        return services;
    }

}