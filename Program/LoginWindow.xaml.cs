// File Name: LoginWindow.xaml.cs
// Author: Melody Lo
// Version number: 1.0
// Date published: Sept. 17, 2021
// Project name: Milano – Project Mariana
// Company/Division: KLA BBP-GPG Advanced Tech
// File Description: Requests login info 

using System.Windows;
using System.Windows.Media;


namespace Mariana
{
    /// <summary>
    /// Gets connection info from user.
    /// </summary>
    public partial class LoginWindow : Window
    {
        /// <summary>
        /// A delegate that points to a boolean function with string parameters.
        /// </summary>
        /// <param name="server"> The server name. </param>
        /// <param name="userID"> The username. </param>
        /// <param name="password"> The password. </param>
        /// <param name="database"> The MySql database name. </param>
        /// <returns> True, if the connection was successful; False, otherwise. </returns>
        public delegate bool Login (string server, string userID, string password, string database);

        /// <summary>
        /// Defined in MainWindow.xaml.cs.
        /// </summary>
        public Login TryLogin { get; set; }

        /// <summary>
        /// Stores the MySql database name.
        /// </summary>
        public SqlDB SqlDB { get; set; }

        /// <summary>
        /// Initializes member variables and window. 
        /// </summary>
        /// <param name="dB"> A reference to a MySql database. </param>
        public LoginWindow (ref SqlDB dB)
        {
            SqlDB = dB;

            InitializeComponent();
        }

        /// <summary>
        /// Updates the controls based on whether or not the connection was successful.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void LogInClicked (object sender, RoutedEventArgs e)
        {
            if (TryLogin != null)
            {
                bool loginSuccessful = TryLogin(
                    serverTextBox.Text, userIDTextBox.Text, passwordBox.Password, databaseTextBox.Text);

                if (loginSuccessful)
                {
                    loginLight.Fill = new SolidColorBrush(Colors.LightGreen);
                    logInButton.IsEnabled = false;
                    cancelButton.Content = "Done";
                }
                else
                {
                    loginLight.Fill = new SolidColorBrush(Colors.Red);
                }
            }
        }

        /// <summary>
        /// Resets the controls when any textbox is modified.
        /// </summary>
        /// <param name="sender"> A reference to the textbox. </param>
        /// <param name="e"> Event data. </param>
        private void TextChanged (object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (IsLoaded)
            {
                SqlDB.ConnectionString = null;
                loginLight.Fill = new SolidColorBrush(Colors.Gray);
                logInButton.IsEnabled = true;
                cancelButton.Content = "Cancel";
            }
        }

        /// <summary>
        /// Resets the controls when the passwordbox is modified.
        /// </summary>
        /// <param name="sender"> A reference to the passwordbox. </param>
        /// <param name="e"> Event data. </param>
        private void PasswordChanged (object sender, RoutedEventArgs e)
        {
            SqlDB.ConnectionString = null;
            loginLight.Fill = new SolidColorBrush(Colors.Gray);
            logInButton.IsEnabled = true;
            cancelButton.Content = "Cancel";
        }
    }
}
