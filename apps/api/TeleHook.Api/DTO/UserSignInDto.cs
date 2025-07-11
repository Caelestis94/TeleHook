namespace TeleHook.Api.DTO;

public class EmailPasswordSignInDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class OidcSignInDto
{
    public required string Email { get; set; }
    public required string OidcId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? DisplayName { get; set; }
}
