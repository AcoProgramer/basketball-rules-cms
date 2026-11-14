using BasketballRulesCms.App.Models;

namespace BasketballRulesCms.App.Services;

// upravljanje kolekcijom pravila (u?itavanje i ?uvanje XML dokumenata)
public sealed class RulesRepository
{
    private readonly XmlFileStore<RulesDocument> _store;

    public RulesRepository(XmlFileStore<RulesDocument> store)
    {
        _store = store;
    }

    public RulesDocument Load() => _store.LoadOrCreate(() => new RulesDocument());

    public void Save(RulesDocument doc) => _store.Save(doc);
}

