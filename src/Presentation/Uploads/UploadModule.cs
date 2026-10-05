using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.FileStorage;

namespace Presentation.Uploads;

public class UploadModule : BaseModule
{
    const string BasePath = "/api/uploads";
    const long MaxBytes = 5 * 1024 * 1024; // 5 MB

    static readonly string[] AllowedContentTypes =
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/svg+xml",
    };

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BasePath).WithTags("Uploads");

        group.MapPost("/image", UploadImageAsync).RequireAuthorization().DisableAntiforgery();
    }

    private static async Task<IResult> UploadImageAsync(
        [FromServices] IFileStorageService storage,
        IFormFile? file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return Results.BadRequest(new { error = "Nenhum arquivo enviado." });

        if (file.Length > MaxBytes)
            return Results.BadRequest(new { error = "A imagem excede o tamanho máximo de 5 MB." });

        if (!AllowedContentTypes.Contains(file.ContentType))
            return Results.BadRequest(new { error = "Formato de imagem não suportado." });

        var url = await storage.UploadAsync(file, ct);

        return Results.Ok(new { url });
    }
}
