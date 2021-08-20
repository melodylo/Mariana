using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace DatabaseToGraph
{
    public partial class ManageUsersDialog : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged ([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SqlDB SqlDB { get; set; }

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

        public ManageUsersDialog (SqlDB dB)
        {
            SqlDB = dB;
            SqlDB.GetAccounts();

            InitializeComponent();
        }


        private void CreateAccountClicked (object sender, RoutedEventArgs e)
        {
            CreateAccountDialog createAccountDialog = new(SqlDB);
            createAccountDialog.ShowDialog();
        }

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

        private void DeleteAccountClicked (object sender, RoutedEventArgs e)
        {
            SqlDB.DeleteAccount(SelectedAccount);
            EnableDeleteAccount = false;
        }
    }
}
