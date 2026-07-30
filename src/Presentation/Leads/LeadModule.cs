using System.Text;
using Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Presentation.Leads.Input;
using Presentation.Leads.Output;
using Repository.Persistence;

namespace Presentation.Leads;

public class LeadModule : BaseModule
{
    const string BasePath = "/api/leads";

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BasePath).WithTags("Leads");
        group.MapPost("/", CreateAsync);
        group.MapGet("/", ListAsync).RequireAuthorization("AdminOnly");
        group.MapGet("/export", ExportCsvAsync).RequireAuthorization("AdminOnly");
        group.MapPut("/{id:int}/read", MarkAsReadAsync).RequireAuthorization("AdminOnly");
        group.MapPut("/bulk-read", BulkMarkAsReadAsync).RequireAuthorization("AdminOnly");
        group.MapDelete("/{id:int}", DeleteAsync).RequireAuthorization("AdminOnly");
        group.MapPost("/bulk-delete", BulkDeleteAsync).RequireAuthorization("AdminOnly");
    }

    private async Task<IResult> CreateAsync(
        [FromServices] AppDbContext db,
        [FromBody] CreateLeadRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(request.Email))
            return Results.BadRequest("E-mail é obrigatório.");
        if (string.IsNullOrWhiteSpace(request.Phone))
            return Results.BadRequest("Telefone é obrigatório.");
        if (request.AdvertisementId <= 0)
            return Results.BadRequest("Anúncio inválido.");

        var lead = new Lead(
            request.Name.Trim(),
            request.Email.Trim().ToLowerInvariant(),
            request.Phone.Trim(),
            request.Company?.Trim(),
            request.AdvertisementId,
            request.AdvertisementType)
        {
            CreatedBy = "visitor"
        };

        db.Leads.Add(lead);
        await db.SaveChangesAsync(ct);

        return Results.Created($"{BasePath}/{lead.Id}", LeadResponse.FromEntity(lead));
    }

    private async Task<IResult> ListAsync(
        [FromServices] AppDbContext db,
        int page = 1,
        int pageSize = 20,
        string? search = null,
        string? company = null,
        bool? isRead = null,
        string? advertisementType = null,
        int? advertisementId = null,
        CancellationToken ct = default)
    {
        var query = db.Leads.Where(x => !x.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search) || x.Email.Contains(search));

        if (!string.IsNullOrWhiteSpace(company))
            query = query.Where(x => x.Company != null && x.Company.Contains(company));

        if (isRead.HasValue)
            query = query.Where(x => x.IsRead == isRead.Value);

        if (!string.IsNullOrWhiteSpace(advertisementType))
            query = query.Where(x => x.AdvertisementType == advertisementType);

        if (advertisementId.HasValue)
            query = query.Where(x => x.AdvertisementId == advertisementId.Value);

        query = query.OrderByDescending(x => x.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Results.Ok(new { total, page, pageSize, items = items.Select(LeadResponse.FromEntity) });
    }

    private async Task<IResult> ExportCsvAsync(
        [FromServices] AppDbContext db,
        string? search = null,
        string? company = null,
        bool? isRead = null,
        string? advertisementType = null,
        int? advertisementId = null,
        string? ids = null,
        CancellationToken ct = default)
    {
        var query = db.Leads.Where(x => !x.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(ids))
        {
            var idList = ids.Split(',')
                .Select(s => int.TryParse(s.Trim(), out var v) ? (int?)v : null)
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .ToList();
            query = query.Where(x => idList.Contains(x.Id));
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x => x.Name.Contains(search) || x.Email.Contains(search));

            if (!string.IsNullOrWhiteSpace(company))
                query = query.Where(x => x.Company != null && x.Company.Contains(company));

            if (isRead.HasValue)
                query = query.Where(x => x.IsRead == isRead.Value);

            if (!string.IsNullOrWhiteSpace(advertisementType))
                query = query.Where(x => x.AdvertisementType == advertisementType);

            if (advertisementId.HasValue)
                query = query.Where(x => x.AdvertisementId == advertisementId.Value);
        }

        var items = await query.OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("Id,Nome,Email,Telefone,Empresa,Tipo Anuncio,ID Anuncio,Lido,Data");
        foreach (var item in items)
        {
            sb.AppendLine(string.Join(",",
                item.Id,
                CsvEscape(item.Name),
                CsvEscape(item.Email),
                CsvEscape(item.Phone),
                CsvEscape(item.Company ?? ""),
                CsvEscape(item.AdvertisementType),
                item.AdvertisementId,
                item.IsRead ? "Sim" : "Nao",
                item.CreatedAt.ToString("dd/MM/yyyy HH:mm")));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return Results.File(bytes, "text/csv; charset=utf-8", "leads.csv");
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private async Task<IResult> BulkMarkAsReadAsync(
        [FromServices] AppDbContext db,
        [FromBody] BulkIdsRequest request,
        CancellationToken ct)
    {
        if (request.Ids is not { Count: > 0 }) return Results.BadRequest("Nenhum ID informado.");

        var leads = await db.Leads
            .Where(x => request.Ids.Contains(x.Id) && !x.IsDeleted && !x.IsRead)
            .ToListAsync(ct);

        foreach (var lead in leads) lead.MarkAsRead();
        await db.SaveChangesAsync(ct);

        return Results.Ok(new { updated = leads.Count });
    }

    private async Task<IResult> BulkDeleteAsync(
        [FromServices] AppDbContext db,
        [FromBody] BulkIdsRequest request,
        CancellationToken ct)
    {
        if (request.Ids is not { Count: > 0 }) return Results.BadRequest("Nenhum ID informado.");

        var leads = await db.Leads
            .Where(x => request.Ids.Contains(x.Id) && !x.IsDeleted)
            .ToListAsync(ct);

        foreach (var lead in leads)
        {
            lead.IsDeleted = true;
            lead.UpdatedAt = DateTimeOffset.UtcNow;
        }
        await db.SaveChangesAsync(ct);

        return Results.Ok(new { deleted = leads.Count });
    }

    private async Task<IResult> MarkAsReadAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var lead = await db.Leads.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (lead is null) return Results.NotFound();

        lead.MarkAsRead();
        await db.SaveChangesAsync(ct);

        return Results.Ok(LeadResponse.FromEntity(lead));
    }

    private async Task<IResult> DeleteAsync(
        [FromServices] AppDbContext db,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var lead = await db.Leads.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (lead is null) return Results.NotFound();

        lead.IsDeleted = true;
        lead.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
