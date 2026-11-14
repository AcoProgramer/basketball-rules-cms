using BasketballRulesCms.App.Models;

namespace BasketballRulesCms.App.Services;

public static class AppServices
{
    private static bool _initialized;

    public static UserRepository  Users   { get; private set; } = null!;
    public static RulesRepository Rules   { get; private set; } = null!;
    public static RtfService      Rtf     { get; private set; } = null!;
    public static ImageService    Images  { get; private set; } = null!;
    public static AppSession      Session { get; }              = new();

    public static void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;

        Users  = new UserRepository (new XmlFileStore<UsersDocument>(AppPaths.UsersXmlRelativePath));
        Rules  = new RulesRepository(new XmlFileStore<RulesDocument>(AppPaths.RulesXmlRelativePath));
        Rtf    = new RtfService();
        Images = new ImageService();

        Users.EnsureDefaultUsers();
        SeedService.EnsureSeedData();
    }
}

public sealed class AppSession
{
    public User? CurrentUser { get; set; }
    public bool  IsAdmin     => CurrentUser?.Role == UserRole.Admin;
}
