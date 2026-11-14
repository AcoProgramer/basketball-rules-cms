using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using BasketballRulesCms.App.Models;
using BasketballRulesCms.App.Services;
using Microsoft.Win32;

namespace BasketballRulesCms.App.Views;

// stranica za dodavanje / izmenu pravila (toolbar, RTF editor, izbor slike i čuvanje)
public partial class AddEditRulePage : Page
{
    private readonly Guid? _editId;

    // Konstruktor: inicijalizuje komponentu, VM i toolbar kontrole
    public AddEditRulePage(Guid? editId = null)
    {
        InitializeComponent();
        _editId     = editId;
        DataContext = new AddEditVm();

        // popunjavanje toolbar kontrola
        FontFamilyBox.ItemsSource  = Fonts.SystemFontFamilies.OrderBy(f => f.Source).ToList();
        FontFamilyBox.SelectedItem = Fonts.SystemFontFamilies.FirstOrDefault(f => f.Source == "Segoe UI")
                                     ?? Fonts.SystemFontFamilies.First();

        FontSizeBox.ItemsSource  = new[] { 9, 10, 11, 12, 13, 14, 16, 18, 20, 22, 24, 28, 32, 36, 48, 72 };
        FontSizeBox.SelectedItem = 14;

        ColorBox.ItemsSource  = ColourVm.All;
        ColorBox.SelectedItem = ColourVm.All.FirstOrDefault(c => c.Name == "White") ?? ColourVm.All.First();

        Loaded += (_, _) => LoadContent();
    }

    private AddEditVm Vm => (AddEditVm)DataContext;

    // Lifecycle: učitava sadržaj za dodavanje ili izmenu
    private void LoadContent()
    {
        if (_editId is null)
        {
            Vm.SetMode(false);
            PageTitleTb.Text = "DODAJ NOVO PRAVILO";
            Editor.Document  = new FlowDocument();
            Vm.RefreshWordCount(Editor.Document);
            return;
        }

        var doc  = AppServices.Rules.Load();
        var rule = doc.Items.FirstOrDefault(r => r.Id == _editId.Value);
        if (rule is null)
        {
            MessageBox.Show("Traženo pravilo nije pronađeno.",
                "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            NavigationService?.GoBack();
            return;
        }

        Vm.SetMode(true);
        PageTitleTb.Text = "IZMENA PRAVILA";
        Vm.LoadFromModel(rule);

        Editor.Document = AppServices.Rtf.LoadToFlowDocument(rule.RtfPath);
        Vm.RefreshWordCount(Editor.Document);
        Editor.Focus();
    }

    // Toolbar akcija: prebacuje tekst u bold
    private void Bold_Click(object sender, RoutedEventArgs e)
        => EditingCommands.ToggleBold.Execute(null, Editor);

    // Toolbar akcija: prebacuje tekst u italic
    private void Italic_Click(object sender, RoutedEventArgs e)
        => EditingCommands.ToggleItalic.Execute(null, Editor);

    // Toolbar akcija: podvlači ili uklanja podvlačenje
    private void Underline_Click(object sender, RoutedEventArgs e)
        => EditingCommands.ToggleUnderline.Execute(null, Editor);

    // Toolbar: primeni izabrani font family na selekciju
    private void FontFamily_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (FontFamilyBox.SelectedItem is FontFamily ff)
            Editor.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, ff);
    }

    // Toolbar: primeni izabranu veličinu fonta na selekciju
    private void FontSize_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (FontSizeBox.SelectedItem is int sz)
            Editor.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, (double)sz);
    }

    // Toolbar: primeni boju na selekciju i ažuriraj swatch
    private void Color_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (ColorBox.SelectedItem is not ColourVm cv) return;
        Editor.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, cv.Brush);
        // ažuriranje swatch-a boje
        ColourSwatch.Fill = cv.Brush;
    }

    // Editor event: tekst promenjen — osveži broj reči i validaciju RTF-a
    private void Editor_TextChanged(object sender, TextChangedEventArgs e)
    {
        Vm.RefreshWordCount(Editor.Document);
        Vm.ValidateRtf(Editor.Document);
    }

    // Editor event: selekcija promenjena — ažuriraj toolbar dugmad (bold/italic/underline)
    private void Editor_SelectionChanged(object sender, RoutedEventArgs e)
    {
        BoldBtn.IsChecked      = GetProp(TextElement.FontWeightProperty, FontWeights.Bold);
        ItalicBtn.IsChecked    = GetProp(TextElement.FontStyleProperty,  FontStyles.Italic);
        UnderlineBtn.IsChecked = GetUnderline();
    }

    // Helper: proverava svoju vrednost dependency property-ja u selekciji
    private bool GetProp(DependencyProperty prop, object compareTo)
    {
        var val = Editor.Selection.GetPropertyValue(prop);
        return val != DependencyProperty.UnsetValue && val.Equals(compareTo);
    }

    // Helper: proverava da li selekcija ima underline dekoraciju
    private bool GetUnderline()
    {
        var val = Editor.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
        return val is TextDecorationCollection col && col.Count > 0
               && col[0].Location == TextDecorationLocation.Underline;
    }

    // Akcija: otvara dijalog za izbor slike i kopira je u Data/images
    private void PickImage_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title     = "Izbor slike za pravilo",
            Filter    = "Slike|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp|Sve|*.*",
            Multiselect = false,
        };
        if (dlg.ShowDialog() != true) return;

        Mouse.OverrideCursor = Cursors.Wait;
        try
        {
            Vm.ImagePath = AppServices.Images.CopyIntoDataImages(dlg.FileName);
            Vm.ImageHint = System.IO.Path.GetFileName(dlg.FileName);
            Vm.ValidateImage();
        }
        finally { Mouse.OverrideCursor = null; }
    }

    // Akcija: validacija i čuvanje modela (novog ili izmenjenog)
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Vm.ValidateAll();                 // Provera običnih polja
        Vm.ValidateRtf(Editor.Document);  // Provera opisa
        if (!Vm.IsValid) return;          // Ako nešto nije ispravno, prekida se

        try
        {
            var doc = AppServices.Rules.Load(); // Učitava sva pravila iz XML-a
            BasketballRule model;

            if (_editId is null)
            {
                model = Vm.BuildNew();    // Pravi novo pravilo
                doc.Items.Add(model);     // Dodaje ga u listu
            }
            else
            {
                model = doc.Items.First(r => r.Id == _editId.Value); // Nalazi postojeće
                Vm.ApplyTo(model);        // Upisuje nove vrednosti u postojeći objekat
            }

            if (string.IsNullOrWhiteSpace(model.RtfPath))
                model.RtfPath = System.IO.Path.Combine("Data", "rtf", $"{model.Id:N}.rtf");

            AppServices.Rtf.SaveFlowDocument(model.RtfPath, Editor.Document); // Čuva opis u .rtf
            AppServices.Rules.Save(doc);                                       // Čuva listu pravila u XML

            ShellWindow.Instance.ShowSuccess(
                _editId is null
                    ? $"Pravilo \"{model.Title}\" je uspešno dodato."
                    : $"Pravilo \"{model.Title}\" je uspešno izmenjeno.");

            NavigationService?.GoBack(); // Vraća na prethodnu stranicu
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    // Akcija: povratak na prethodnu stranicu
    private void Back_Click(object sender, RoutedEventArgs e)
        => NavigationService?.GoBack();

    // ViewModel

    internal sealed class AddEditVm : INotifyPropertyChanged
    {
        private string _ruleNumberText = "";
        private string _title          = "";
        private string _imagePath      = "";
        private string _imageHint      = "Nije izabrana slika";
        private int    _wordCount;

        // fields

        public string RuleNumberText
        {
            get => _ruleNumberText;
            set { _ruleNumberText = value; Notify(); ValidateRuleNumber(); }
        }

        public string Title
        {
            get => _title;
            set { _title = value; Notify(); ValidateTitle(); }
        }

        public string ImagePath
        {
            get => _imagePath;
            set { _imagePath = value; Notify(); Notify(nameof(HasImagePreview)); }
        }

        public bool HasImagePreview => !string.IsNullOrWhiteSpace(ImagePath);

        public string ImageHint
        {
            get => _imageHint;
            set { _imageHint = value; Notify(); }
        }

        public string WordCountText => $"Reči: {_wordCount}";

        // mode

        public void SetMode(bool isEditing) { /* page title set in code-behind */ }

        // load / build

        public void LoadFromModel(BasketballRule r)
        {
            RuleNumberText = r.RuleNumber.ToString(CultureInfo.InvariantCulture);
            Title          = r.Title;
            ImagePath      = r.ImagePath;
            ImageHint      = System.IO.Path.GetFileName(r.ImagePath);
        }

        public BasketballRule BuildNew()
        {
            var id = Guid.NewGuid(); // Novi jedinstveni ID
            return new BasketballRule
            {
                Id         = id,
                RuleNumber = int.Parse(RuleNumberText, CultureInfo.InvariantCulture),
                Title      = Title.Trim(),
                ImagePath  = ImagePath,
                RtfPath    = System.IO.Path.Combine("Data", "rtf", $"{id:N}.rtf"), // Putanja do opisa
                AddedAt    = DateTimeOffset.UtcNow,
            };
        }

        public void ApplyTo(BasketballRule r)
        {
            r.RuleNumber = int.Parse(RuleNumberText, CultureInfo.InvariantCulture);
            r.Title      = Title.Trim();
            r.ImagePath  = ImagePath;
            // AddedAt NOT updated on edit
        }

        public void RefreshWordCount(FlowDocument doc)
        {
            _wordCount = RtfService.CountWords(doc);
            Notify(nameof(WordCountText));
        }

        // validation

        public void ValidateAll() { ValidateRuleNumber(); ValidateTitle(); ValidateImage(); }

        public void ValidateRuleNumber()
        {
            RuleNumberError = "";
            if (string.IsNullOrWhiteSpace(RuleNumberText))
                RuleNumberError = "Broj pravila je obavezan.";
            else if (!int.TryParse(RuleNumberText, NumberStyles.Integer,
                     CultureInfo.InvariantCulture, out var n))
                RuleNumberError = "Mora biti ceo broj (npr. 1, 2, 3).";
            else if (n <= 0)
                RuleNumberError = "Broj mora biti veći od nule.";
            Notify(nameof(RuleNumberError)); Notify(nameof(HasRuleNumberError));
        }

        public void ValidateTitle()
        {
            TitleError = "";
            if (string.IsNullOrWhiteSpace(Title))
                TitleError = "Naziv pravila je obavezan.";
            else if (Title.Trim().Length < 3)
                TitleError = "Naziv mora imati najmanje 3 karaktera.";
            else if (Title.Trim().Length > 200)
                TitleError = "Naziv ne može biti duži od 200 karaktera.";
            Notify(nameof(TitleError)); Notify(nameof(HasTitleError));
        }

        public void ValidateImage()
        {
            ImageError = "";
            if (string.IsNullOrWhiteSpace(ImagePath))
                ImageError = "Slika je obavezna. Izaberite fajl klikom na dugme.";
            Notify(nameof(ImageError)); Notify(nameof(HasImageError));
            Notify(nameof(IsValid));
        }

        public void ValidateRtf(FlowDocument? doc)
        {
            if (doc is null) return;
            var text = new TextRange(doc.ContentStart, doc.ContentEnd).Text;
            RtfError = string.IsNullOrWhiteSpace(text)
                ? "Opis (RTF editor) je obavezan — unesite tekst."
                : "";
            Notify(nameof(RtfError)); Notify(nameof(HasRtfError));
            Notify(nameof(IsValid));
        }

        public string RuleNumberError { get; private set; } = "";
        public bool   HasRuleNumberError => !string.IsNullOrEmpty(RuleNumberError);
        public string TitleError         { get; private set; } = "";
        public bool   HasTitleError      => !string.IsNullOrEmpty(TitleError);
        public string ImageError         { get; private set; } = "";
        public bool   HasImageError      => !string.IsNullOrEmpty(ImageError);
        public string RtfError           { get; private set; } = "";
        public bool   HasRtfError        => !string.IsNullOrEmpty(RtfError);

        public bool IsValid => !HasRuleNumberError && !HasTitleError
                            && !HasImageError      && !HasRtfError;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void Notify([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    // Colour view-model — svi sistemski fajlovi boja sa swatch-om

    internal sealed class ColourVm
    {
        public required string          Name  { get; init; }
        public required SolidColorBrush Brush { get; init; }

        public static IReadOnlyList<ColourVm> All { get; } = Build();

        private static List<ColourVm> Build()
        {
            var list = new List<ColourVm>();
            foreach (var p in typeof(Colors)
                         .GetProperties(BindingFlags.Public | BindingFlags.Static)
                         .Where(p => p.PropertyType == typeof(Color))
                         .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase))
            {
                var col   = (Color)p.GetValue(null)!;
                var brush = new SolidColorBrush(col);
                brush.Freeze();
                list.Add(new ColourVm { Name = p.Name, Brush = brush });
            }
            return list;
        }
    }
}
