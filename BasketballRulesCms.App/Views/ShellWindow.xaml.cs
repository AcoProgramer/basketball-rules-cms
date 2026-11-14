using System.Windows;
using System.Windows.Media.Animation;
using BasketballRulesCms.App.Services;
using Notification.Wpf;

namespace BasketballRulesCms.App.Views;

// glavni prozor aplikacije (navigacija, obaveštenja i window chrome)
public partial class ShellWindow : Window
{
    // lokalni menadžer za obaveštenja (toast)
    private readonly NotificationManager _notifMgr = new();

    // konstruktor - inicijalizuje servise i startnu stranicu
    public ShellWindow()
    {
        InitializeComponent();
        AppServices.EnsureInitialized();
        RootFrame.Navigate(new LoginPage());

        StateChanged += (_, _) =>
            MaxIcon.Icon = WindowState == WindowState.Maximized
                ? FontAwesome5.EFontAwesomeIcon.Regular_WindowRestore
                : FontAwesome5.EFontAwesomeIcon.Regular_Square;
    }

    // toast helper - prikaz poruka iz drugih stranica
    public static ShellWindow Instance => (ShellWindow)Application.Current.MainWindow;

    public void ShowSuccess(string message) =>
        _notifMgr.Show(message, NotificationType.Success, areaName: "NotifArea",
                        expirationTime: TimeSpan.FromSeconds(3));

    public void ShowWarning(string message) =>
        _notifMgr.Show(message, NotificationType.Warning, areaName: "NotifArea",
                        expirationTime: TimeSpan.FromSeconds(3));

    public void ShowError(string message) =>
        _notifMgr.Show(message, NotificationType.Error, areaName: "NotifArea",
                        expirationTime: TimeSpan.FromSeconds(4));

    // navigacioni helper
    public void Navigate(System.Windows.Controls.Page page) => RootFrame.Navigate(page);

    // window chrome - rukovanje naslovom i kontrolama prozora
    private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2) ToggleMax();
        else DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)   => WindowState = WindowState.Minimized;
    private void MaxRestore_Click(object sender, RoutedEventArgs e) => ToggleMax();
    private void Close_Click(object sender, RoutedEventArgs e)      => Application.Current.Shutdown();

    private void ToggleMax() =>
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal : WindowState.Maximized;
}
