namespace Presentation.Users.Input;

public record CreateUserRequest(
    string Name,
    string Username,
    string Email,
    string Password
);
