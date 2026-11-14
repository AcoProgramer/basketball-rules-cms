using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BasketballRulesCms.App.Services;

namespace BasketballRulesCms.App.Views;

// stranica za prijavu korisnika (validacija polja i autentikacija)
public partial class LoginPage : Page
{
    // Konstruktor: inicijalizuje komponentu i fokusira username polje
    public LoginPage()
    {
        InitializeComponent();
        Loaded += (_, _) => UsernameBox.Focus();
    }

    // Klik na dugme Prijava: pokušaj prijave
    private void Login_Click(object sender, RoutedEventArgs e) => TryLogin();

    // Klik na dugme Izlaz: zatvori aplikaciju
    private void Exit_Click(object sender, RoutedEventArgs e)  => Application.Current.Shutdown();

    // Enter pritisak u input polju: pokušaj prijave
    private void Input_KeyDown(object sender, KeyEventArgs e)  { if (e.Key == Key.Enter) TryLogin(); }

    // Fokus u Password polju: sakrij placeholder
    private void Password_GotFocus(object sender, RoutedEventArgs e)
        => PasswordPlaceholder.Visibility = Visibility.Collapsed;

    // Gubitak fokusa iz Password polja: pokaži placeholder ako je prazno
    private void Password_LostFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(PasswordBox.Password))
            PasswordPlaceholder.Visibility = Visibility.Visible;
    }

    // Pokušaj prijave: validacija polja i autentikacija
    private void TryLogin()
    {
        HideAll();
        var username = UsernameBox.Text.Trim();
        var password  = PasswordBox.Password;
        var ok = true;

        if (string.IsNullOrWhiteSpace(username))
        {
            ShowField(UsernameErr, "Korisničko ime je obavezno.");
            UsernameBox.BorderBrush = System.Windows.Media.Brushes.Red;
            ok = false;
        }
        if (string.IsNullOrEmpty(password))
        {
            ShowField(PasswordErr, "Lozinka je obavezna.");
            PasswordBox.BorderBrush = System.Windows.Media.Brushes.Red;
            ok = false;
        }
        if (!ok) return;

        var user = AppServices.Users.Authenticate(username, password);
        if (user is null)
        {
            AuthErrText.Text       = "Pogrešno korisničko ime ili lozinka.";
            AuthErrPanel.Visibility = Visibility.Visible;
            PasswordBox.Clear();
            return;
        }

        AppServices.Session.CurrentUser = user;
        ShellWindow.Instance.ShowSuccess($"Dobro došao, {user.Username}! ({user.Role})");
        NavigationService?.Navigate(new RulesListPage());
    }

    // Sakrij sve greške i povratne indikatore
    private void HideAll()
    {
        UsernameErr.Visibility  = Visibility.Collapsed;
        PasswordErr.Visibility  = Visibility.Collapsed;
        AuthErrPanel.Visibility = Visibility.Collapsed;
        UsernameBox.ClearValue(System.Windows.Controls.TextBox.BorderBrushProperty);
        PasswordBox.ClearValue(System.Windows.Controls.PasswordBox.BorderBrushProperty);
    }

    // Pomoćna funkcija: prikaži error poruku pored polja
    private static void ShowField(TextBlock tb, string msg)
    {
        tb.Text       = msg;
        tb.Visibility = Visibility.Visible;
    }
}
