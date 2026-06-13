namespace Presentation.Users.Output;

public record LoginResponse(
    string Token,
    DateTimeOffset ExpiresAt,
    UserResponse User
);
