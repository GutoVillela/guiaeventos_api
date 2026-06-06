namespace Presentation.Users.Input;

public record LoginRequestDTO(
    string Email,
    string Username,
    string Password
    );