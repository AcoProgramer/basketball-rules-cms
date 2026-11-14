using BasketballRulesCms.App.Models;

namespace BasketballRulesCms.App.Services;

// repozitorijum za upravljanje korisnicima (ucitavanje, cuvanje, autentikacija)
public sealed class UserRepository
{
    private readonly XmlFileStore<UsersDocument> _store;

    public UserRepository(XmlFileStore<UsersDocument> store)
    {
        _store = store;
    }

    // u?itavanje dokumenta korisnika
    public UsersDocument Load() => _store.LoadOrCreate(() => new UsersDocument());

    // ?uvanje dokumenta korisnika
    public void Save(UsersDocument doc) => _store.Save(doc);

    // inicijalizacija podrazumevanih korisnika
    public void EnsureDefaultUsers()
    {
        var doc = _store.LoadOrCreate(() => new UsersDocument());
        if (doc.Items.Count > 0) return;

        doc.Items.Add(new User
        {
            Username = "admin",
            Password = "admin123",
            Role = UserRole.Admin,
        });
        doc.Items.Add(new User
        {
            Username = "visitor",
            Password = "visitor123",
            Role = UserRole.Visitor,
        });

        _store.Save(doc);
    }

    // autentikacija korisnika
    public User? Authenticate(string username, string password)
    {
        var doc = Load();
        return doc.Items.FirstOrDefault(u =>
            string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase) &&
            u.Password == password);
    }
}

