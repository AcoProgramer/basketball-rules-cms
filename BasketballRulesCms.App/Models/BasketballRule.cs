using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BasketballRulesCms.App.Models;

[Serializable]
public sealed class BasketballRule : INotifyPropertyChanged
{
    public Guid           Id         { get; set; } = Guid.NewGuid();
    public int            RuleNumber { get; set; }
    public required string Title     { get; set; }
    public required string ImagePath { get; set; }
    public required string RtfPath   { get; set; }
    public DateTimeOffset  AddedAt   { get; set; }

    // ── IsSelected lives only in memory — not serialised ─────────────────────
    private bool _isSelected;

    [System.Xml.Serialization.XmlIgnore]
    public bool IsSelected
    {
        get => _isSelected;
        set { if (_isSelected == value) return; _isSelected = value; Notify(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void Notify([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
