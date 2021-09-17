// File Name: CreateAccountWindow.xaml.cs
// Author: Melody Lo
// Version number: 1.0
// Date published: Sept. 17, 2021
// Project name: Milano – Project Mariana
// Company/Division: KLA BBP-GPG Advanced Tech
// File Description: Requests account creation details 

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Mariana
{
    /// <summary>
    /// Creates new MySql account.
    /// </summary>
    public partial class CreateAccountWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged ([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Stores MySql database.
        /// </summary>
        public SqlDB SqlDB { get; set; }

        /// <summary>
        /// Stores MySql username.
        /// This is done automatically through databinding.
        /// </summary>
        private string user;
        public string User
        {
            get => user;
            set
            {
                user = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Stores the user's privilege.
        /// This is done automatically through databinding.
        /// </summary>
        private Privilege privilege;
        public Privilege Privilege
        {
            get => privilege;
            set
            {
                privilege = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes member variables and window.
        /// </summary>
        /// <param name="dB"> A reference to a MySql database object. </param>
        public CreateAccountWindow (ref SqlDB dB)
        {
            SqlDB = dB;

            InitializeComponent();
        }

        /// <summary>
        /// An account is created when the Create button is clicked.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void CreateClicked (object sender, RoutedEventArgs e)
        {
            SqlDB.CreateAccount(User, passwordBox.Password, Privilege);
            Close();
        }
    }
}
