namespace BasketballRulesCms.App.Models;

public enum UserRole { Visitor = 0, Admin = 1 }

public sealed class User
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public UserRole Role { get; set; }
}
