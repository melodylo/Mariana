using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace DatabaseToGraph
{
    public enum Privilege
    {
        ReadOnly,
        ReadAndWrite,
        Admin
    };

    public class Account : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged ([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string host;
        public string Host
        {
            get => host;
            set
            {
                host = value;
                NotifyPropertyChanged();
            }
        }

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

        public Account(string host = null, string user = null, Privilege privilege = Privilege.ReadOnly)
        {
            Host = host;
            User = user;
            Privilege = privilege;
        }
    }

    public class SqlDB : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged ([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool DatetimeXAxis { get; private set; }
        public bool DatetimeYAxis { get; private set; }

        private string host;
        private string database;

        private ObservableCollection<Account> accounts = new();
        public ObservableCollection<Account> Accounts
        {
            get => accounts;
            set
            {
                accounts = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<string> tables = new();
        public ObservableCollection<string> Tables
        {
            get => tables;
            private set
            {
                tables = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<string> columns = new();
        public ObservableCollection<string> Columns
        {
            get => columns;
            private set
            {
                columns = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<Point> data = new();
        public ObservableCollection<Point> Data
        {
            get => data;
            private set
            {
                data = value;
                NotifyPropertyChanged();
            }
        }

        private string connectionString;
        public string ConnectionString
        {
            get => connectionString;
            set
            {
                connectionString = value;
                if (connectionString == null)
                {
                    loginSuccessful = false;
                    DatetimeXAxis = false;
                    DatetimeYAxis = false;

                    host = null;
                    database = null;

                    Tables.Clear();
                    Columns.Clear();
                    Data.Clear();
                }

                NotifyPropertyChanged();
            }
        }
        private bool loginSuccessful;
        public bool LoginSuccessful
        {
            get => loginSuccessful;
            private set
            {
                loginSuccessful = value;
                NotifyPropertyChanged();
            }
        }

        public bool TryLogin (string server, string user, string password, string database)
        {
            this.database = database;

            MySqlConnectionStringBuilder connectionStringBuilder = new();
            connectionStringBuilder.Server = server;
            connectionStringBuilder.UserID = user;
            connectionStringBuilder.Password = password;
            connectionStringBuilder.Database = database;

            ConnectionString = connectionStringBuilder.ConnectionString;

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    loginSuccessful = true;
                    host = GetHost();
                }
                catch
                {
                    loginSuccessful = false;
                }
            }

            return loginSuccessful;
        }

        public void AddTables ()
        {
            using MySqlConnection connection = new(ConnectionString);
            connection.Open();
            DataTable tablesInfo = connection.GetSchema("Tables");
            foreach (DataRow row in tablesInfo.Rows)
            {
                tables.Add(row[2].ToString());
            }
        }

        public void AddColumns (string tableName)
        {
            Columns.Clear();

            using MySqlConnection connection = new(ConnectionString);
            connection.Open();
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM information_schema.columns WHERE table_name = '" + tableName + "' ORDER BY ordinal_position";
            using MySqlDataReader columnsReader = command.ExecuteReader();
            while (columnsReader.Read())
            {
                Columns.Add(columnsReader.GetString(3));
            }
        }

        public Type GetColumnType (string tableName, string columnName)
        {
            using MySqlConnection connection = new(ConnectionString);
            connection.Open();
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT " + columnName + " FROM " + tableName + " LIMIT 1";
            using MySqlDataReader reader = command.ExecuteReader();
            reader.Read();
            return reader.GetFieldType(0);
        }

        public ObservableCollection<Point> GetData (string tableName, string xColumn, string yColumn, string xMin, string xMax)
        {
            Data.Clear();

            Type xColumnType = GetColumnType(tableName, xColumn);
            if (xColumnType == typeof(DateTime))
            {
                DatetimeXAxis = true;
            }
            else
            {
                DatetimeXAxis = false;
            }

            Type yColumnType = GetColumnType(tableName, yColumn);
            if (yColumnType == typeof(DateTime))
            {
                DatetimeYAxis = true;
            }
            else
            {
                DatetimeYAxis = false;
            }

            using (MySqlConnection connection = new(ConnectionString))
            {
                connection.Open();
                MySqlCommand getXYCommand = connection.CreateCommand();

                if (Columns == null)
                {
                    getXYCommand.CommandText = "SELECT * FROM " + tableName;
                }
                else
                {
                    getXYCommand.CommandText = "SELECT " + xColumn + ", " + yColumn + " FROM " + tableName;
                }

                bool xMinNotEmpty = !string.IsNullOrWhiteSpace(xMin);
                bool xMaxNotEmpty = !string.IsNullOrWhiteSpace(xMax);

                if (xMinNotEmpty)
                {
                    getXYCommand.CommandText += " WHERE " + xColumn + " >= @xMin";

                    if (xMaxNotEmpty)
                    {
                        getXYCommand.CommandText += " AND " + xColumn + " < @xMax";
                    }
                }
                else if (xMaxNotEmpty)
                {
                    getXYCommand.CommandText += " WHERE " + xColumn + " < @xMax";
                }

                getXYCommand.CommandText += " ORDER BY " + xColumn + " ASC";

                if (xMinNotEmpty)
                {
                    if (DatetimeXAxis)
                    {
                        DateTime datetime = DateTime.Parse(xMin);
                        xMin = datetime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        xMin = xMin.Replace(",", "");
                    }

                    getXYCommand.Parameters.AddWithValue("@xMin", xMin);
                }
                if (xMaxNotEmpty)
                {
                    if (DatetimeXAxis)
                    {
                        DateTime datetime = DateTime.Parse(xMax);
                        xMax = datetime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        xMax = xMax.Replace(",", "");
                    }

                    getXYCommand.Parameters.AddWithValue("@xMax", xMax);
                }

                using MySqlDataReader rowsReader = getXYCommand.ExecuteReader();
                while (rowsReader.Read())
                {
                    if (rowsReader[xColumn] != DBNull.Value && rowsReader[yColumn] != DBNull.Value)
                    {
                        double x;
                        double y;

                        if (xColumnType == typeof(DateTime))
                        {
                            x = ((DateTime) rowsReader[xColumn]).ToOADate();
                        }
                        else
                        {
                            if (xColumnType != typeof(string))
                            {
                                x = Convert.ToDouble(rowsReader[xColumn]);
                            }
                            else
                            {
                                return null;
                            }
                        }

                        if (yColumnType == typeof(DateTime))
                        {
                            y = ((DateTime) rowsReader[yColumn]).ToOADate();
                        }
                        else
                        {
                            if (yColumnType != typeof(string))
                            {
                                y = Convert.ToDouble(rowsReader[yColumn]);
                            }
                            else
                            {
                                return null;
                            }
                        }

                        Point point = new(x, y);
                        Data.Add(point);
                    }
                }
            }

            return Data;
        }

        public bool AddData (DataTable datatable, string tableName)
        {
            bool dataAdded = false;

            foreach (DataRow row in datatable.Rows)
            {
                List<string> enteredColumns = new();
                foreach (string column in Columns)
                {
                    if (row[column] != DBNull.Value)
                    {
                        enteredColumns.Add(column);
                    }
                }

                if (enteredColumns.Count != 0)
                {
                    string enteredColumnsJoined = string.Join(",", enteredColumns);
                    List<string> columnValueNames = enteredColumns.Select(x => "@" + x).ToList();
                    string columnValueNamesJoined = string.Join(",", columnValueNames);

                    using MySqlConnection connection = new(ConnectionString);
                    connection.Open();
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO " + tableName + "(" + enteredColumnsJoined + ") VALUES(" + columnValueNamesJoined + ")";

                    try
                    {
                        foreach (string column in enteredColumns)
                        {
                            object value = row[column];
                            Type columnType = GetColumnType(tableName, column);
                            if (columnType == typeof(DateTime))
                            {
                                DateTime date = DateTime.Parse(value.ToString());
                                value = date.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            command.Parameters.AddWithValue("@" + column, value);
                        }

                        int successful = command.ExecuteNonQuery();

                        if (successful == 0)
                        {
                            string message = "Could not add data.";
                            string title = "Add Data";
                            MessageBox.Show(message, title);

                            dataAdded = false;
                        }
                        else
                        {
                            dataAdded = true;
                        }
                    }
                    catch (MySqlException exception)
                    {
                        string title = "Add Data";
                        MessageBox.Show(exception.Message, title);

                        dataAdded = false;

                        break;
                    }
                }
            }

            return dataAdded;
        }

        public string ExportData (string tableName, List<string> constraints)
        {
            string constraintsJoined = string.Join(" ", constraints);

            using MySqlConnection connection = new(ConnectionString);
            connection.Open();
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM " + tableName + " WHERE " + constraintsJoined;

            StringBuilder text = new();
            string columnsJoined = string.Join(", ", Columns);
            text.AppendLine(columnsJoined);

            using MySqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    List<string> values = new();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        values.Add(reader.GetString(i));
                    }
                    string valuesJoined = string.Join(", ", values);
                    text.AppendLine(valuesJoined);
                }

                return text.ToString();
            }
            else
            {
                return null;
            }
        }

        public void Logout ()
        {
            ConnectionString = null;
            Accounts.Clear();
        }

        public void GetAccounts ()
        {
            if (Accounts.Count == 0)
            {
                using MySqlConnection connection = new(ConnectionString);
                connection.Open();
                MySqlCommand getUsersCommand = connection.CreateCommand();
                getUsersCommand.CommandText = "SELECT Host, User FROM mysql.user WHERE User NOT LIKE 'mysql.%'";

                using MySqlDataReader userReader = getUsersCommand.ExecuteReader();
                while (userReader.Read())
                {
                    Account account = new(host: userReader.GetString(userReader.GetOrdinal("Host")), user: userReader.GetString(userReader.GetOrdinal("User")));
                    Accounts.Add(account);
                }

                userReader.Close();

                foreach (Account account in Accounts)
                {
                    MySqlCommand getPrivilegeCommand = connection.CreateCommand();
                    getPrivilegeCommand.CommandText = $"SHOW GRANTS FOR '{account.User}'@'{account.Host}'";

                    using MySqlDataReader privilegeReader = getPrivilegeCommand.ExecuteReader();
                    while (privilegeReader.Read())
                    {
                        if ((privilegeReader.GetString(0).Contains("ALL") || privilegeReader.GetString(0).Contains("CREATE USER")) && privilegeReader.GetString(0).Contains("WITH GRANT OPTION") && (privilegeReader.GetString(0).Contains("*.*") || privilegeReader.GetString(0).Contains(database)))
                        {
                            account.Privilege = Privilege.Admin;
                            break;
                        }
                        else if (privilegeReader.GetString(0).Contains("SELECT") && privilegeReader.GetString(0).Contains("INSERT") && (privilegeReader.GetString(0).Contains("*.*") || privilegeReader.GetString(0).Contains(database)))
                        {
                            account.Privilege = Privilege.ReadAndWrite;
                            break;
                        }
                        else if (privilegeReader.GetString(0).Contains("SELECT") && (privilegeReader.GetString(0).Contains("*.*") || privilegeReader.GetString(0).Contains(database)))
                        {
                            account.Privilege = Privilege.ReadOnly;
                            break;
                        }
                    }
                }
            }
        }

        public Privilege? GetPrivilege ()
        {
            using MySqlConnection connection = new(ConnectionString);
            connection.Open();
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "SHOW GRANTS";

            using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if ((reader.GetString(0).Contains("ALL") || reader.GetString(0).Contains("CREATE USER")) && reader.GetString(0).Contains("WITH GRANT OPTION"))
                {
                    return Privilege.Admin;
                }
                else if (reader.GetString(0).Contains("SELECT") && reader.GetString(0).Contains("INSERT"))
                {
                    return Privilege.ReadAndWrite;
                }
                else if (reader.GetString(0).Contains("SELECT"))
                {
                    return Privilege.ReadOnly;
                }
            }

            return null;
        }

        public string GetHost ()
        {
            using MySqlConnection connection = new(ConnectionString);
            connection.Open();
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT substr(current_user(),LOCATE('@', current_user()) + 1) AS host";

            using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                return reader.GetString(reader.GetOrdinal("host"));
            }

            return null;
        }

        public void CreateAccount (string user, string password, Privilege privilege)
        {
            using MySqlConnection connection = new(ConnectionString);
            connection.Open();

            MySqlCommand createUserCommand = connection.CreateCommand();
            createUserCommand.CommandText = $"CREATE USER '{user}'@'{host}' IDENTIFIED BY '{password}';";
            createUserCommand.ExecuteNonQuery();

            MySqlCommand grantPrivilegeCommand = connection.CreateCommand();
            if (privilege == Privilege.Admin)
            {
                grantPrivilegeCommand.CommandText = $"GRANT ALL ON *.* TO '{user}'@'{host}' WITH GRANT OPTION;";
                grantPrivilegeCommand.CommandText += $" GRANT SELECT ON mysql.user TO '{user}'@'{host}';";
            }
            else if (privilege == Privilege.ReadAndWrite)
            {
                grantPrivilegeCommand.CommandText = $"GRANT SELECT, INSERT ON {database}.* TO '{user}'@'{host}';";
            }
            else
            {
                grantPrivilegeCommand.CommandText = $"GRANT SELECT ON {database}.* TO '{user}'@'{host}';";
            }

            grantPrivilegeCommand.CommandText += " FLUSH PRIVILEGES;";
            grantPrivilegeCommand.ExecuteNonQuery();

            Account account = new(host, user, privilege);
            Accounts.Add(account);
        }

        public void AssignPrivilege (string host, string user, Privilege privilege)
        {
            using MySqlConnection connection = new(ConnectionString);
            connection.Open();

            MySqlCommand command = connection.CreateCommand();
            command.CommandText = $"REVOKE ALL, GRANT OPTION FROM '{user}'@'{host}';";
            if (privilege == Privilege.Admin)
            {
                command.CommandText += $" GRANT ALL ON {database}.* TO '{user}'@'{host}' WITH GRANT OPTION;";
            }
            else if (privilege == Privilege.ReadAndWrite)
            {
                command.CommandText += $" GRANT SELECT, INSERT ON {database}.* TO '{user}'@'{host}';";
            }
            else
            {
                command.CommandText += $" GRANT SELECT ON {database}.* TO '{user}'@'{host}';";
            }
            command.CommandText += " FLUSH PRIVILEGES;";

            command.ExecuteNonQuery();
        }

        public void DeleteAccount (Account account)
        {
            using MySqlConnection connection = new(ConnectionString);
            connection.Open();

            MySqlCommand command = connection.CreateCommand();
            command.CommandText = $"DROP USER '{account.User}'@'{account.Host}';";
            command.CommandText += " FLUSH PRIVILEGES;";

            command.ExecuteNonQuery();

            Accounts.Remove(account);
        }
    }
}