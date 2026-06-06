using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Presentation.Users.Input;
using Repository.Persistence;
using Domain.Entities;

namespace Presentation.Users;

public class UserModule : BaseModule
{
    const string BasePath = "/user";
    const string LoginPath = BasePath + "/login";
    const string CreateUserPath = BasePath;
    const string GetUserPath = BasePath + "/{username}";
    const string ListUsersPath = BasePath;
    const string UpdateUserPath = BasePath + "/{id}";
    const string DeleteUserPath = BasePath + "/{id}";
    const string ChangePasswordPath = BasePath + "/{id}/password";
    const string CheckIfUsernameExistsPath = BasePath + "/check_username";
    const string SendResetPasswordCode = BasePath + "/send-reset-password-code";
    const string ValidateResetPasswordCode = BasePath + "/validate-reset-password-code";
    const string ResetPasswordPath = BasePath + "/reset-password";
    const string ResendResetPasswordCodePath = BasePath + "/resend-reset-password-code";
    const string CreateUserRolePath = BasePath + "/role";
    const string GetAllRolesPath = BasePath + "/roles";


    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(LoginPath, LoginAsync);
        app.MapGet(GetUserPath, GetUserAsync).RequireAuthorization();

    }

    private async Task<IResult> LoginAsync([FromServices] AppDbContext dbContext, [FromBody] LoginRequestDTO request, CancellationToken cancellationToken)
    {
        User? user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user is null)
        {
            return Results.BadRequest("User not found");
        }

        return Results.Ok(user);
    }

    private async Task<IResult> GetUserAsync([FromServices] AppDbContext dbContext, [FromRoute] string username, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

        if (user is null)
        {
            return Results.BadRequest("User not found");
        }

        return Results.Ok(user);
    }

}