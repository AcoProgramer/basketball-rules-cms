using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using BasketballRulesCms.App.Models;
using BasketballRulesCms.App.Services;

namespace BasketballRulesCms.App.Views;

public partial class RulesListPage : Page
{
    // kolekcija pravila (listanje objekata)
    public ObservableCollection<BasketballRule> Rules { get; } = new();

    // Konstruktor: inicijalizuje komponentu i osvežava listu
    public RulesListPage()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += (_, _) => Refresh();
    }

    // Lifecycle: učitava i prikazuje sva pravila
    private void Refresh()
    {
        var isAdmin = AppServices.Session.IsAdmin;

        Rules.Clear();
        foreach (var r in AppServices.Rules.Load().Items.OrderBy(x => x.RuleNumber))
        {
            // registracija promene selektovanih objekata
            r.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(BasketballRule.IsSelected))
                    UpdateSelectAllState();
            };
            Rules.Add(r);
        }

        // podešavanje vidljivosti admin kontrola
        AddBtn.Visibility     = isAdmin ? Visibility.Visible : Visibility.Collapsed;
        DeleteBtn.Visibility  = isAdmin ? Visibility.Visible : Visibility.Collapsed;
        ToolbarRow.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
        CheckCol.Visibility   = isAdmin ? Visibility.Visible : Visibility.Collapsed;

        // prikaz praznog stanja (nema objekata)
        EmptyLabel.Visibility = Rules.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        UpdateFooter();
    }

    // Ažurira footer (meta informacije: korisnik, ukupno, poslednje dodato)
    private void UpdateFooter()
    {
        var user = AppServices.Session.CurrentUser;
        RoleLabel.Text = user?.Role.ToString().ToUpper() ?? "";

        TotalLabel.Text = Rules.Count.ToString();

        var last = Rules.OrderByDescending(r => r.AddedAt).FirstOrDefault();
        LastAddedLabel.Text = last is not null
            ? last.AddedAt.UtcDateTime.ToString("dd.MM.yyyy")
            : "N/A";
    }

    // Ažurira stanje 'Select All' checkbox-a na osnovu selekcije redova
    private void UpdateSelectAllState()
    {
        var sel = Rules.Count(r => r.IsSelected);
        SelectAllBox.IsChecked = sel == 0 ? false : sel == Rules.Count ? true : null;
    }

    // Odjava korisnika i vraćanje na login stranicu
    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        AppServices.Session.CurrentUser = null;
        NavigationService?.Navigate(new LoginPage());
    }

    // Navigacija na stranicu za dodavanje pravila
    private void Add_Click(object sender, RoutedEventArgs e)
    {
        if (!AppServices.Session.IsAdmin) return;
        NavigationService?.Navigate(new AddEditRulePage());
    }

    // Brisanje selektovanih pravila uz potvrdu i uklanjanje fajlova
    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (!AppServices.Session.IsAdmin) return; // Samo admin sme da briše.

        var toDelete = Rules.Where(r => r.IsSelected).ToList(); // Uzima sva pravila koja su čekirana.
        if (toDelete.Count == 0)
        {
            ShellWindow.Instance.ShowWarning("Nijedan red nije označen za brisanje!"); // Ako ništa nije izabrano, prikazuje upozorenje.
            return;
        }

        var res = MessageBox.Show(
            $"Da li si siguran da želiš da obrišeš {toDelete.Count} označenih pravila?\nOva akcija se ne može poništiti.",
            "Potvrda brisanja",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning); // Traži potvrdu od korisnika pre brisanja.

        if (res != MessageBoxResult.Yes) return; // Ako korisnik ne potvrdi, prekida se brisanje.

        var doc = AppServices.Rules.Load(); // Učitava pravila iz XML-a.
        var ids = toDelete.Select(r => r.Id).ToHashSet(); // Pamti ID-jeve pravila koja treba obrisati.

        foreach (var rule in doc.Items.Where(r => ids.Contains(r.Id)))
        {
            TryDelete(rule.RtfPath);   // Briše RTF fajl tog pravila.
            TryDelete(rule.ImagePath); // Briše sliku tog pravila.
        }

        doc.Items = doc.Items.Where(r => !ids.Contains(r.Id)).ToList(); // Uklanja ta pravila iz liste.
        AppServices.Rules.Save(doc); // Snima izmenjenu listu nazad u XML.

        ShellWindow.Instance.ShowSuccess($"Uspešno obrisano {toDelete.Count} pravila."); // Success poruka.
        SelectAllBox.IsChecked = false; // Resetuje "select all" checkbox.
        Refresh(); // Osvežava prikaz liste na ekranu.
    }

    // Selektovanje / poništavanje selekcije svih redova
    private void SelectAll_Click(object sender, RoutedEventArgs e)
    {
        var check = SelectAllBox.IsChecked == true;
        foreach (var r in Rules) r.IsSelected = check;
    }

    // Otvara detalje ili stranicu za izmenu pri klik na naslov
    private void TitleLink_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Hyperlink hl) return;
        var rule = FindRule(hl);
        if (rule is null) return;

        if (AppServices.Session.IsAdmin)
            NavigationService?.Navigate(new AddEditRulePage(rule.Id));
        else
            NavigationService?.Navigate(new RuleDetailsPage(rule.Id));
    }

    // Hover efekat za naslov: primeni stil
    private void TitleTb_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not TextBlock tb) return;
        if (tb.Inlines.FirstInline is Hyperlink hl)
        {
            hl.Foreground      = System.Windows.Media.Brushes.White;
            hl.TextDecorations = TextDecorations.Underline;
        }
    }

    // Hover efekat za naslov: ukloni stil
    private void TitleTb_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not TextBlock tb) return;
        if (tb.Inlines.FirstInline is Hyperlink hl)
        {
            hl.Foreground      = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x00));
            hl.TextDecorations = null;
        }
    }

    // Pomoćna: pronađi BasketballRule instancu iz FrameworkContentElement-a
    private static BasketballRule? FindRule(FrameworkContentElement? el)
    {
        while (el is not null)
        {
            if (el.DataContext is BasketballRule r) return r;
            el = el.Parent as FrameworkContentElement;
        }
        return null;
    }

    // Pomoćna: pokušaj brisanja fajla sa diska bez izbacivanja izuzetaka
    private static void TryDelete(string rel)
    {
        try
        {
            var abs = AppPaths.ToAbsolutePath(rel);
            if (System.IO.File.Exists(abs)) System.IO.File.Delete(abs);
        }
        catch {}
    }
}
