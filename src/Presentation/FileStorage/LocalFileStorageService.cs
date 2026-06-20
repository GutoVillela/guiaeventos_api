using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Presentation.FileStorage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _savePath;
    private readonly string _urlPrefix;

    public LocalFileStorageService(IConfiguration config)
    {
        _savePath = config["FileStorage:LocalPath"]
            ?? Path.Combine(Path.GetTempPath(), "guiaeventos", "uploads");
        _urlPrefix = config["FileStorage:UrlPrefix"] ?? "/uploads";
        Directory.CreateDirectory(_savePath);
    }

    public async Task<string> UploadAsync(IFormFile file, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(_savePath, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream, ct);

        return $"{_urlPrefix}/{fileName}";
    }
}
