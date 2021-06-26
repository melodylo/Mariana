using MySql.Data.MySqlClient;
using System.Windows;
using System.Data;
using System.Windows.Media;
using System.Collections.Generic;

namespace Database
{
    public partial class MainWindow : Window
    {
        private MySqlConnection connection;

        public MainWindow()
        {
            InitializeComponent();
        }

        private bool TestConnection (string server, string userID, string password, string database)
        {
            if (connection != null)
            {
                disconnect();
            }

            string connectionstring = "server=" + server + ";uid=" + userID +
                ";pwd=" + password + ";database=" + database;
            connection = new MySqlConnection(connectionstring);

            bool connectionSuccessful;
            try
            {
                connection.Open();
                connectionSuccessful = true;

                AddTables();

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

        private void AddTables ()
        {
            DataTable tables = connection.GetSchema("Tables");
            foreach (DataRow row in tables.Rows)
            {
                comboBox_tables.Items.Add(row[2].ToString());
            }
        }

        private void ButtonClick_Connect (object sender, RoutedEventArgs e)
        {
            ConnectionDialog connectionDialog = new();

            // get info for sql connection string
            connectionDialog.TestConnectionClicked += new ConnectionDialog.TestConnection(TestConnection);

            connectionDialog.ShowDialog();
        }

        private void ButtonClick_PlotData (object sender, RoutedEventArgs e)
        {
            if (comboBox_tables.SelectedItem != null)
            {
                if (comboBox_xAxis.SelectedItem != null)
                {
                    if (comboBox_yAxis.SelectedItem != null)
                    {
                        (string xMin, string xMax) = Assign_xMinMax();
                        if (Valid_xMinMax(xMin, xMax))
                        {
                            List<(double, double)> points = MakeQuery(xMin, xMax);
                            (double[] x, double[] y) = ConvertListToArray(points);
                            if (x.Length != 0 && y.Length != 0)
                            {
                                PlotData(x, y);
                            }
                            else
                            {
                                string message = "No data found.";
                                string title = "Plot data";
                                MessageBox.Show(message, title);
                            }
                            plot.Plot.Legend();
                        }
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

        private static (double[], double[]) ConvertListToArray(List<(double, double)> points)
        {
            double[] x = new double[points.Count];
            double[] y = new double[points.Count];

            int index = 0;
            foreach((double, double) point in points)
            {
                x[index] = point.Item1;
                y[index] = point.Item2;
                index++;
            }
            return (x, y);
        }

        private (string, string) Assign_xMinMax()
        {
            string xMin = null;
            string xMax = null;
            if ((bool) checkBox_xMin.IsChecked)
            {
                xMin = textBox_xMin.Text;
            }
            if ((bool) checkBox_xMax.IsChecked)
            {
                xMax = textBox_xMax.Text;
            }
            return (xMin, xMax);
        }

        private List<(double, double)> MakeQuery (string xMin, string xMax)
        {
            MySqlCommand command = connection.CreateCommand();
            string tableName = comboBox_tables.SelectedItem.ToString();

            string xTable = comboBox_xAxis.SelectedItem.ToString();
            string yTable = comboBox_yAxis.SelectedItem.ToString();

            command.CommandText = "SELECT " + xTable + "," + yTable + " FROM " + tableName;

            if (xMin != null)
            {
                xMin = xMin.Replace(",", "");
            }
            if (xMax != null)
            {
                xMax = xMax.Replace(",", "");
            }

            if (xMin != null && xMax != null)
            {
                command.CommandText += " WHERE " + xTable + " > " + xMin + " AND " + xTable + " < " + xMax;
            }
            else if (xMin != null)
            {
                command.CommandText += " WHERE " + xTable + " > " + xMin;
            }
            else if (xMax != null)
            {
                command.CommandText += " WHERE " + xTable + " < " + xMax;
            }

            string xAxis = comboBox_xAxis.SelectedItem.ToString();
            string yAxis = comboBox_yAxis.SelectedItem.ToString();

            List<(double, double)> points = new();

            using (MySqlDataReader rowsReader = command.ExecuteReader())
            {
                while (rowsReader.Read())
                {
                    (double x, double y) point = (x: rowsReader.GetDouble(xAxis), y: rowsReader.GetDouble(yAxis));
                    points.Add(point);
                }
            }
            return points;
        }

        private bool Valid_xMinMax (string xMin, string xMax)
        {
            bool xMin_ok;
            bool xMax_ok;
            if (xMin == null)
            {
                xMin_ok = true;
            }
            else
            {
                if (double.TryParse(xMin, out double _))
                {
                    xMin_ok = true;
                }
                else
                {
                    xMin_ok = false;
                    string message = "The minimum value must be a number.";
                    string title = "Filter x-axis";
                    MessageBox.Show(message, title);
                }
            }

            if (xMax == null)
            {
                xMax_ok = true;
            }
            else
            {
                if (double.TryParse(xMax, out double _))
                {
                    xMax_ok = true;
                }
                else
                {
                    xMax_ok = false;
                    string message = "The maximum value must be a number.";
                    string title = "Filter x-axis";
                    MessageBox.Show(message, title);
                }
            }
            return xMin_ok && xMax_ok;
        }

        private void PlotData (double[] x, double[] y)
        {
            string tableName = comboBox_tables.SelectedItem.ToString();
            string yAxis = comboBox_yAxis.SelectedItem.ToString();
            plot.Plot.AddScatter(x, y, markerSize: 0, label: tableName + ": " + yAxis);
            button_clear.IsEnabled = true;
        }

        private void ButtonClick_Disconnect (object sender, RoutedEventArgs e)
        {
            disconnect();
        }

        private void disconnect()
        {
            connection.Close();
            connection = null;
            button_connect.IsEnabled = true;
            button_disconnect.IsEnabled = false;
            button_plotData.IsEnabled = false;

            comboBox_tables.Items.Clear();
            comboBox_xAxis.Items.Clear();
            comboBox_yAxis.Items.Clear();
        }

        private void ButtonClick_Clear (object sender, RoutedEventArgs e)
        {
            plot.Reset();
            button_clear.IsEnabled = false;
        }

        private void TableSelected (object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            comboBox_xAxis.Items.Clear();
            comboBox_yAxis.Items.Clear();

            if (IsLoaded && comboBox_tables.SelectedItem != null)
            {
                string tableName = comboBox_tables.SelectedItem.ToString();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "'";
                using MySqlDataReader columnsReader = command.ExecuteReader();
                while (columnsReader.Read())
                {
                    comboBox_xAxis.Items.Add(columnsReader.GetString(3));
                    comboBox_yAxis.Items.Add(columnsReader.GetString(3));
                }
            }
        }

        private void Window_Closing (object sender, System.ComponentModel.CancelEventArgs e)
        {
            string message = "Close window?";
            string title = "Confirm close";
            MessageBoxResult result = MessageBox.Show(message, title, MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        private void TextBoxFocus_xMin (object sender, RoutedEventArgs e)
        {
            if (textBox_xMin.Text == "Enter min...")
            {
                textBox_xMin.Text = "";
                textBox_xMin.Foreground = Brushes.Black;
            }
        }

        private void TextBoxFocus_xMax (object sender, RoutedEventArgs e)
        {
            if (textBox_xMax.Text == "Enter max...")
            {
                textBox_xMax.Text = "";
                textBox_xMax.Foreground = Brushes.Black;
            }
        }

        private void TextBoxNotFocus_xMin (object sender, RoutedEventArgs e)
        {
            if (textBox_xMin.Text == "")
            {
                textBox_xMin.Text = "Enter min...";
                textBox_xMin.Foreground = Brushes.Gray;
            }
        }

        private void TextBoxNotFocus_xMax (object sender, RoutedEventArgs e)
        {
            if (textBox_xMax.Text == "")
            {
                textBox_xMax.Text = "Enter max...";
                textBox_xMax.Foreground = Brushes.Gray;
            }
        }
    }
}
