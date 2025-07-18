namespace TeleHook.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string? PasswordHash { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Role { get; set; } = "admin";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // OIDC-related fields
    public string? OidcId { get; set; } 
    public string? AuthProvider { get; set; }
    public bool IsOidcLinked => !string.IsNullOrEmpty(OidcId);
    
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string DisplayName => !string.IsNullOrEmpty(FullName) && FullName != " " ? FullName : Username;
}
