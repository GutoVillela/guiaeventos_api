namespace Presentation.Users.Input;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);
