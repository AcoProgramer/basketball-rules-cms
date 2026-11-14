using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BasketballRulesCms.App.Services;
using BasketballRulesCms.App.Views.Converters;

namespace BasketballRulesCms.App.Views;

// prikaz detalja pravila (bez binding-a, read-only)
public partial class RuleDetailsPage : Page
{
    private readonly Guid _id;
    private static readonly RelativePathToImage _imgConv = new();
    private static readonly DateToString        _dtConv  = new();

    public RuleDetailsPage(Guid id)
    {
        InitializeComponent();
        _id = id;
        Loaded += (_, _) => Load();
    }

    private void Load()
    {
        var doc  = AppServices.Rules.Load();
        var rule = doc.Items.FirstOrDefault(r => r.Id == _id);

        if (rule is null)
        {
            MessageBox.Show("Traženo pravilo nije pronađeno.",
                "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            NavigationService?.GoBack();
            return;
        }

        // popuni polja ručno (bez DataContext bind-a)
        TitleTb.Text      = rule.Title;
        RuleNumberTb.Text = rule.RuleNumber.ToString(CultureInfo.InvariantCulture);
        DateTb.Text       = rule.AddedAt.UtcDateTime
                                .ToString("dd.MM.yyyy · HH:mm 'UTC'", CultureInfo.InvariantCulture);

        if (_imgConv.Convert(rule.ImagePath, typeof(BitmapImage), null!,
                System.Globalization.CultureInfo.InvariantCulture) is BitmapImage bmp)
            RuleImage.Source = bmp;

        var flowDoc = AppServices.Rtf.LoadToFlowDocument(rule.RtfPath);
        flowDoc.Foreground = System.Windows.Media.Brushes.White;
        RtfViewer.Document = flowDoc;
    }

    private void Back_Click(object sender, RoutedEventArgs e)
        => NavigationService?.GoBack();
}
