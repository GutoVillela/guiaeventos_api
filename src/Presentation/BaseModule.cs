using System.Net;
using Carter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Results;

namespace Presentation;

public abstract class BaseModule : ICarterModule
{
    public abstract void AddRoutes(IEndpointRouteBuilder app);

    protected IResult HandleFailureResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok();
        }

        return result.ErrorType switch
        {
            Result.FailureType.BadRequest => Results.BadRequest(result.Errors),
            Result.FailureType.NotFound => Results.NotFound(result.Errors),
            Result.FailureType.Unauthorized => Results.Unauthorized(),
            Result.FailureType.Conflict => Results.Conflict(result.Errors),
            _ => Results.StatusCode((int)HttpStatusCode.InternalServerError)
        };
    }
}
