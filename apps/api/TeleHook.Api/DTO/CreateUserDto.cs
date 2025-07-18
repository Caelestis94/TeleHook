namespace TeleHook.Api.DTO;

public class CreateUserDto
{
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Role { get; set; } = "admin";
}
