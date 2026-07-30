using Infrastructure.GoogleCloud;
using Microsoft.Extensions.Options;

namespace WebAPI.Configuration.OptionsSetup;

public class GoogleCloudOptionsSetup : IConfigureOptions<GoogleCloudOptions>
{
    public static string SectionName = "GoogleCloud";
    public readonly IConfiguration _configuration;

    public GoogleCloudOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(GoogleCloudOptions options)
    {
        _configuration.GetSection(SectionName).Bind(options);
    }
}
