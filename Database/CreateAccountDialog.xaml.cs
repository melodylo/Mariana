using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DatabaseToGraph
{
    public partial class CreateAccountDialog : Window
    {
        public SqlDB SqlDB { get; set; }
        public string User { get; set; }
        public Privilege Privilege { get; set; }

        public CreateAccountDialog (SqlDB dB)
        {
            SqlDB = dB;

            InitializeComponent();
        }

        private void SignUpClicked (object sender, RoutedEventArgs e)
        {
            SqlDB.CreateAccount(User, passwordBox.Password, Privilege);
            Close();
        }
    }
}
