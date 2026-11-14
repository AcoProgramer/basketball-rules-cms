using System.Xml.Serialization;
using BasketballRulesCms.App.Models;

namespace BasketballRulesCms.App.Services;

[XmlRoot("Users")]
public sealed class UsersDocument
{
    [XmlElement("User")]
    public List<User> Items { get; set; } = new();
}

[XmlRoot("BasketballRules")]
public sealed class RulesDocument
{
    [XmlElement("Rule")]
    public List<BasketballRule> Items { get; set; } = new();
}

