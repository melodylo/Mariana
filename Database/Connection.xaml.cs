using MySql.Data.MySqlClient;
using System.Windows;
using System.Windows.Media;


namespace DatabaseToGraph
{
    public partial class ConnectionDialog : Window
    {
        public delegate bool TestConnection (string server, string userID, string password, string database);
        internal TestConnection TestConnectionClicked;
        public SqlDB Database { get; set; }

        public ConnectionDialog (ref SqlDB database)
        {
            InitializeComponent();

            Database = database;
        }

        private void ConnectButton_Click (object sender, RoutedEventArgs e)
        {
            if (TestConnectionClicked != null)
            {
                bool connectionSuccessful = TestConnectionClicked(
                    serverTextBox.Text, userIDTextBox.Text, passwordTextBox.Password, databaseTextBox.Text);

                if (connectionSuccessful)
                {
                    connectionLight.Fill = new SolidColorBrush(Colors.LightGreen);
                    testConnectionButton.IsEnabled = false;
                    cancelButton.Content = "Done";
                }
                else
                {
                    connectionLight.Fill = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private void TextChanged (object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Database.Connection = null;
                connectionLight.Fill = new SolidColorBrush(Colors.Gray);
                testConnectionButton.IsEnabled = true;
                cancelButton.Content = "Cancel";
            }
        }

        private void PasswordChanged (object sender, RoutedEventArgs e)
        {
            Database.Connection = null;
            connectionLight.Fill = new SolidColorBrush(Colors.Gray);
            testConnectionButton.IsEnabled = true;
            cancelButton.Content = "Cancel";
        }
    }
}
