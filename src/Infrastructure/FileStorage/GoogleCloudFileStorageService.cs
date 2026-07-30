using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Infrastructure.GoogleCloud;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Presentation.FileStorage;

namespace Infrastructure.FileStorage;

public class GoogleCloudFileStorageService : IFileStorageService
{
    private readonly GoogleCloudOptions _googleCloud;
    private readonly StorageClient _storageClient;
    private readonly ILogger<GoogleCloudFileStorageService> _logger;

    public GoogleCloudFileStorageService(IOptions<GoogleCloudOptions> googleCloud, ILogger<GoogleCloudFileStorageService> logger)
    {
        _googleCloud = googleCloud.Value;
        _logger = logger;

        GoogleCredential credential;
        try
        {
            credential = GoogleCredential.GetApplicationDefault();
        }
        catch (AggregateException e)
        {
            // no default credentials found
            _logger.LogError(e, "Credential creation failed. Trying to create from json");
            try
            {
                credential = GoogleCredential.FromJson(_googleCloud.Secret.AccountService.ToJsonString());
                _logger.LogInformation("Credential creation success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Credential creation failed");
                throw;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Credential creation failed");
            throw;
        }

        _storageClient = StorageClient.Create(credential);
    }

    public async Task<string> UploadAsync(IFormFile file, CancellationToken ct = default)
    {
        var objectName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        using var stream = file.OpenReadStream();

        await _storageClient.UploadObjectAsync(
            _googleCloud.BucketName,
            objectName,
            file.ContentType,
            stream,
            cancellationToken: ct);

        return $"https://storage.googleapis.com/{_googleCloud.BucketName}/{objectName}";
    }
}
