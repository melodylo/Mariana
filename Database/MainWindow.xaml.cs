using MySql.Data.MySqlClient;
using System.Windows;
using System.Data;

namespace Database
{
    public partial class MainWindow : Window
    {
        MySqlConnection connection;
        bool connectionSuccessful;

        public MainWindow()
        {
            InitializeComponent();
        }

        bool testConnection(string server, string userID, string password, string database)
        {
            string connectionstring = "server=" + server + ";uid=" + userID +
                ";pwd=" + password + ";database=" + database;
            connection = new MySqlConnection(connectionstring);
            
            try
            {
                connection.Open();
                connectionSuccessful = true;

                addTables();

                button_plotData.IsEnabled = true;
                button_disconnect.IsEnabled = true;
                button_connect.IsEnabled = false;
            }
            catch
            {
                connectionSuccessful = false;
            }

            return connectionSuccessful;
        }

        private void addTables()
        {
            DataTable tables = connection.GetSchema("Tables");
            foreach (DataRow row in tables.Rows)
            {
                comboBox_tables.Items.Add(row[2].ToString());
            }
        }

        private void buttonClick_connect(object sender, RoutedEventArgs e)
        {
            ConnectionDialog connectionDialog = new ConnectionDialog();
            connectionDialog.testConnectionClicked += new ConnectionDialog.TestConnectionDelegate(testConnection);
            connectionDialog.ShowDialog();
        }

        private void buttonClick_plotData(object sender, RoutedEventArgs e)
        {
            if (comboBox_tables.SelectedItem != null)
            {
                string tableName = comboBox_tables.SelectedItem.ToString();
                if (comboBox_x.SelectedItem != null)
                {
                    string xAxis = comboBox_x.SelectedItem.ToString();
                    if (comboBox_y.SelectedItem != null)
                    {
                        string yAxis = comboBox_y.SelectedItem.ToString();
                        (double[] x, double[] y) = MakeQuery(tableName, xAxis, yAxis);
                        plotData(x, y);
                    }
                    else
                    {
                        string message = "Please select y-axis.";
                        string title = "Missing y-axis";
                        MessageBox.Show(message, title);
                    }
                }
                else
                {
                    string message = "Please select x-axis.";
                    string title = "Missing x-axis";
                    MessageBox.Show(message, title);
                }
            }
            else
            {
                string message = "Please select table.";
                string title = "Missing table";
                MessageBox.Show(message, title);
            }
        }

        private (double[], double[]) MakeQuery(string tableName, string xAxis, string yAxis)
        {
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM " + tableName;
            int rowcount = (int) (long) command.ExecuteScalar();

            command.CommandText = "SELECT * FROM " + tableName;
            double[] x = new double[rowcount];
            double[] y = new double[rowcount];

            using (MySqlDataReader rowsReader = command.ExecuteReader())
            {
                int index = 0;
                while (rowsReader.Read())
                {
                    x[index] = rowsReader.GetFloat(xAxis);
                    y[index] = rowsReader.GetFloat(yAxis);
                    index++;
                }
            }
            return (x, y);
        }

        public void plotData(double[] x, double[] y)
        {
            plot.Plot.AddScatter(x, y);
            button_clear.IsEnabled = true;
        }

        private void buttonClick_disconnect(object sender, RoutedEventArgs e)
        {
            connection.Close();
            button_connect.IsEnabled = true;
            button_disconnect.IsEnabled = false;
            button_plotData.IsEnabled = false;

            comboBox_tables.Items.Clear();
            comboBox_x.Items.Clear();
            comboBox_y.Items.Clear();
        }

        private void buttonClick_clear(object sender, RoutedEventArgs e)
        {
            plot.Reset();
            button_clear.IsEnabled = false;
        }

        private void tableSelected (object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            comboBox_x.Items.Clear();
            comboBox_y.Items.Clear();

            if (IsLoaded && comboBox_tables.SelectedItem != null)
            {
                string tableName = comboBox_tables.SelectedItem.ToString();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "'";
                using (MySqlDataReader columnsReader = command.ExecuteReader())
                {
                    while (columnsReader.Read())
                    {
                        comboBox_x.Items.Add(columnsReader.GetString(3));
                        comboBox_y.Items.Add(columnsReader.GetString(3));
                    }
                }
            }
        }
    }
}
