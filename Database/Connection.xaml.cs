using MySql.Data.MySqlClient;
using ScottPlot;
using System.Windows;
using System.Windows.Media;


namespace Database
{ 
    public partial class ConnectionDialog : Window
    {
        public delegate bool TestConnectionDelegate(string server, string userID, string password, string database);
        public TestConnectionDelegate testConnectionClicked;

        public ConnectionDialog()
        {
            InitializeComponent();
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            if (testConnectionClicked != null)
            {
                bool successful = testConnectionClicked(serverTextBox.Text, userIDTextBox.Text, passwordTextBox.Password, databaseTextBox.Text);
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

        private void textChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (IsLoaded)
            {
                connectionLight.Fill = new SolidColorBrush(Colors.Gray);
                testConnectionButton.IsEnabled = true;
                cancelButton.Content = "Cancel";
            }
        }

        private void passwordChanged(object sender, RoutedEventArgs e)
        {
            connectionLight.Fill = new SolidColorBrush(Colors.Gray);
            testConnectionButton.IsEnabled = true;
            cancelButton.Content = "Cancel";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // confirm whether user wants to close window
        }
    }
}
