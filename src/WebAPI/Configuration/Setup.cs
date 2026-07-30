using WebAPI.Configuration.OptionsSetup;

namespace WebAPI.Configuration;

public static class Setup
{
    public static void SetupGoogleCloudOptions(WebApplicationBuilder builder)
    {
        builder.Services.ConfigureOptions<GoogleCloudOptionsSetup>();
    }
}
