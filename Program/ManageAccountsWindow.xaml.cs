// File Name: ManageAccountsWindow.xaml.cs
// Author: Melody Lo
// Version number: 1.0
// Date published: Sept. 17, 2021
// Project name: Milano – Project Mariana
// Company/Division: KLA BBP-GPG Advanced Tech
// File Description: Manages all the MySql accounts

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Mariana
{
    /// <summary>
    /// Manages the MySql accounts.
    /// </summary>
    public partial class ManageAccountsWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// A required event for INotifyPropertyChanged.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invoked when a databound property is set.
        /// </summary>
        /// <param name="propertyName"></param>
        private void NotifyPropertyChanged ([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Stores a MySql database object.
        /// </summary>
        private SqlDB dB;
        public SqlDB SqlDB
        {
            get => dB;
            set => dB = value;
        }

        /// <summary>
        /// Stores the account that is selected in the XAML listbox.
        /// This is done automatically through databinding.
        /// </summary>
        private Account selectedAccount = new();
        public Account SelectedAccount
        {
            get => selectedAccount;
            set
            {
                selectedAccount = value;
                EnableDeleteAccount = true;
            }
        }

        /// <summary>
        /// When the Enable property is set to true, the XAML button's IsEnabled property is set to True.
        /// When the Enable property is set to false, the XAML button's IsEnabled property is set to False.
        /// This is done automatically through databinding.
        /// </summary>
        private bool enableDeleteAccount;
        public bool EnableDeleteAccount
        {
            get => enableDeleteAccount;
            set
            {
                enableDeleteAccount = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the member variables and window.
        /// </summary>
        /// <param name="dB"> A reference to a MySql database object. </param>
        public ManageAccountsWindow (ref SqlDB dB)
        {
            SqlDB = dB;
            SqlDB.GetAccounts();

            InitializeComponent();
        }

        /// <summary>
        /// Shows the Create Account window when the Create Account button is clicked.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void CreateAccountClicked (object sender, RoutedEventArgs e)
        {
            CreateAccountWindow createAccountDialog = new(ref dB);
            createAccountDialog.ShowDialog();
        }

        /// <summary>
        /// Modifies an account's privilege when the privilege combobox's selection is changed in the XAML datagrid.
        /// </summary>
        /// <param name="sender"> A reference to the combobox. </param>
        /// <param name="e"> Event data. </param>
        private void PrivilegeChanged (object sender, SelectionChangedEventArgs e)
        {
            ComboBox privilegeComboBox = sender as ComboBox;
            if (privilegeComboBox.IsDropDownOpen)
            {
                bool succeeded = Enum.TryParse(privilegeComboBox.Text, out Privilege oldPrivilege);
                Privilege newPrivilege = (Privilege) privilegeComboBox.SelectedValue;

                if (succeeded)
                {
                    if (newPrivilege != oldPrivilege)
                    {
                        SqlDB.AssignPrivilege(SelectedAccount.Host, SelectedAccount.User, newPrivilege);
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the selected account when the Delete Account button is clicked.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void DeleteAccountClicked (object sender, RoutedEventArgs e)
        {
            SqlDB.DeleteAccount(SelectedAccount);
            EnableDeleteAccount = false;
        }
    }
}
