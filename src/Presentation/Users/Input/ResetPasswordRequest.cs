namespace Presentation.Users.Input;

public record ResetPasswordRequest(string Token, string NewPassword);
