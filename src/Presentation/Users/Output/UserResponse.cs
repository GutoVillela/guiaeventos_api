using Domain.Entities;

namespace Presentation.Users.Output;

public record UserResponse(
    int Id,
    string Name,
    string Username,
    string Email,
    bool IsActive,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
)
{
    public static UserResponse FromEntity(User user) => new(
        user.Id,
        user.Name,
        user.Username,
        user.Email,
        user.IsActive,
        user.CreatedBy,
        user.CreatedAt,
        user.UpdatedAt
    );
}
