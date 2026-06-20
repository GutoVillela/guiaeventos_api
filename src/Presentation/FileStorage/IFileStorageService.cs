using Microsoft.AspNetCore.Http;

namespace Presentation.FileStorage;

public interface IFileStorageService
{
    Task<string> UploadAsync(IFormFile file, CancellationToken ct = default);
}
