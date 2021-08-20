using System.Windows;
using System.Windows.Media;


namespace DatabaseToGraph
{
    public partial class loginDialog : Window
    {
        public delegate bool LogIn (string server, string userID, string password, string database);
        public LogIn TryLogin { get; set; }
        public SqlDB Database { get; set; }

        public loginDialog (ref SqlDB database)
        {
            InitializeComponent();

            Database = database;
        }

        private void LogInClicked (object sender, RoutedEventArgs e)
        {
            if (TryLogin != null)
            {
                bool loginSuccessful = TryLogin(
                    serverTextBox.Text, userIDTextBox.Text, passwordBox.Password, databaseTextBox.Text);

                if (loginSuccessful)
                {
                    loginLight.Fill = new SolidColorBrush(Colors.LightGreen);
                    loginButton.IsEnabled = false;
                    cancelButton.Content = "Done";
                }
                else
                {
                    loginLight.Fill = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private void TextChanged (object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Database.ConnectionString = null;
                loginLight.Fill = new SolidColorBrush(Colors.Gray);
                loginButton.IsEnabled = true;
                cancelButton.Content = "Cancel";
            }
        }

        private void PasswordChanged (object sender, RoutedEventArgs e)
        {
            Database.ConnectionString = null;
            loginLight.Fill = new SolidColorBrush(Colors.Gray);
            loginButton.IsEnabled = true;
            cancelButton.Content = "Cancel";
        }
    }
}
