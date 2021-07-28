using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DatabaseToGraph
{
    public class SqlDB : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged ([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<string> tables = new();
        public ObservableCollection<string> Tables
        {
            get => tables;
            set
            {
                tables = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<string> columns = new();
        public ObservableCollection<string> Columns
        {
            get => columns;
            set
            {
                columns = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<Point> data = new();
        public ObservableCollection<Point> Data
        {
            get => data;
            set
            {
                data = value;
                NotifyPropertyChanged();
            }
        }

        private MySqlConnection connection;
        public MySqlConnection Connection
        {
            get => connection;
            set
            {
                if (value == null)
                {
                    connectionSuccessful = false;
                    tables.Clear();
                    columns.Clear();
                }
                connection = value;
                NotifyPropertyChanged();
            }
        }

        private bool connectionSuccessful;
        public bool ConnectionSuccessful
        {
            get => connectionSuccessful;
            private set
            {
                connectionSuccessful = value;
                NotifyPropertyChanged();
            }
        }

        public bool TestConnection (string server, string userID, string password, string databaseName)
        {
            if (connection != null)
            {
                connection.Close();
                connection = null;
            }

            string connectionstring = "server=" + server + ";uid=" + userID +
                ";pwd=" + password + ";database=" + databaseName;
            connection = new MySqlConnection(connectionstring);

            try
            {
                connection.Open();
                connectionSuccessful = true;

                if (tables != null)
                {
                    tables.Clear();
                }
                
                if (columns != null)
                {
                    columns.Clear();
                }
            }
            catch
            {
                connectionSuccessful = false;
            }

            return connectionSuccessful;
        }

        public void AddTables ()
        {
            tables.Clear();

            DataTable tablesInfo = connection.GetSchema("Tables");
            foreach (DataRow row in tablesInfo.Rows)
            {
                tables.Add(row[2].ToString());
            }
        }

        public void AddColumns (string tableName)
        {
            if (columns != null)
            {
                columns.Clear();
            }

            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "'";
            using MySqlDataReader columnsReader = command.ExecuteReader();
            while (columnsReader.Read())
            {
                columns.Add(columnsReader.GetString(3));
            }
        }

        public ObservableCollection<Point> GetData (string tableName, string xColumn, string yColumn, string xMin, string xMax)
        {
            data.Clear();

            MySqlCommand command = connection.CreateCommand();

            command.CommandText = "SELECT " + xColumn + "," + yColumn + " FROM " + tableName;

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
                command.CommandText += " WHERE " + xColumn + " > " + xMin + " AND " + xColumn + " < " + xMax;
            }
            else if (xMin != null)
            {
                command.CommandText += " WHERE " + xColumn + " > " + xMin;
            }
            else if (xMax != null)
            {
                command.CommandText += " WHERE " + xColumn + " < " + xMax;
            }

            using (MySqlDataReader rowsReader = command.ExecuteReader())
            {
                while (rowsReader.Read())
                {
                    Point point = new(Convert.ToDouble(rowsReader[xColumn]), Convert.ToDouble(rowsReader[yColumn]));
                    data.Add(point);
                }
            }

            return data;
        }

        public void Disconnect ()
        {
            connection.Close();
            connection = null; // updates connectionSuccessful to false

            tables.Clear();
            columns.Clear();
        }
    }
}
