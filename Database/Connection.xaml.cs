using MySql.Data.MySqlClient;
using ScottPlot;
using System.Windows;
using System.Windows.Media;


namespace Database
{ 
    public partial class ConnectionDialog : Window
    {
        public delegate bool TestConnection(string server, string userID, string password, string database);
        public TestConnection TestConnectionClicked;

        public ConnectionDialog()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click (object sender, RoutedEventArgs e)
        {
            if (TestConnectionClicked != null)
            {
                bool successful = TestConnectionClicked(serverTextBox.Text, userIDTextBox.Text, passwordTextBox.Password, databaseTextBox.Text);

                if (successful)
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
                connectionLight.Fill = new SolidColorBrush(Colors.Gray);
                testConnectionButton.IsEnabled = true;
                cancelButton.Content = "Cancel";
            }
        }

        private void PasswordChanged (object sender, RoutedEventArgs e)
        {
            connectionLight.Fill = new SolidColorBrush(Colors.Gray);
            testConnectionButton.IsEnabled = true;
            cancelButton.Content = "Cancel";
        }
    }
}
