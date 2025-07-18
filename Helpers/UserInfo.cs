namespace RestAPI.Helpers;

public class UserInfo
{
    public string UserId { get; set; } = null!;
    public string? Name { get; set; } = null!;
    public string Role { get; set; } = "User";

    public string? Email { get; set; }

    public string? Password { get; set; }
}
