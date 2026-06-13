namespace Presentation.Users.Input;

public record LoginRequest(
    string Email,
    string Username,
    string Password
    );