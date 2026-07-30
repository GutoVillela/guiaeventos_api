using Infrastructure.BackgroundJobs;
using Infrastructure.FileStorage;
using Microsoft.Extensions.DependencyInjection;
using Presentation.FileStorage;
using Quartz;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, bool isProduction)
    {
        AddBackgroundServices(services);
        if(isProduction)
        {
            services.AddSingleton<IFileStorageService, GoogleCloudFileStorageService>();
        }
        else
        {
            services.AddSingleton<IFileStorageService, LocalFileStorageService>();
        }
        return services;
    }

    private static void AddBackgroundServices(this IServiceCollection services) 
    {
        services.AddQuartz(q =>
        {
            var jobKey = new JobKey(nameof(OutboxMessagesProcessorJob));
            q.AddJob<OutboxMessagesProcessorJob>(jobKey)
            .AddTrigger(trigger => trigger
            .ForJob(jobKey)
            .WithSimpleSchedule(schedule => schedule
                .WithInterval(TimeSpan.FromMinutes(1))
                .RepeatForever())
                );
        });
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    }

}
