// File Name: SqlDB.cs
// Author: Melody Lo
// Version number: 1.0
// Date published: Sept. 17, 2021
// Project name: Milano – Project Mariana
// Company/Division: KLA BBP-GPG Advanced Tech
// File Description: Executes all MySql commands

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

namespace Mariana
{
    /// <summary>
    /// The different privilege levels. 
    /// </summary>
    public enum Privilege
    {
        ReadOnly,
        ReadAndWrite,
        Admin
    };

    /// <summary>
    /// Stores all the necessary information for a MySql account.
    /// </summary>
    public class Account : INotifyPropertyChanged
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
        /// Stores the host name.
        /// This is automatically done through databinding.
        /// </summary>
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

        /// <summary>
        /// Stores the username.
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
        /// Stores the privilege.
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
        /// Initializes an account.
        /// </summary>
        /// <param name="host"> The host name. </param>
        /// <param name="user"> The username. </param>
        /// <param name="privilege"> The privilege level. </param>
        public Account (string host = null, string user = null, Privilege privilege = Privilege.ReadOnly)
        {
            Host = host;
            User = user;
            Privilege = privilege;
        }
    }

    /// <summary>
    /// Handles all MySql commands.
    /// </summary>
    public class SqlDB : INotifyPropertyChanged
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
        /// Keeps track of whether the x-axis data is of the datetime type. 
        /// Necessary because the datetime type must be treated differently from numeric types. 
        /// </summary>
        public bool DatetimeXAxis { get; private set; }

        /// <summary>
        /// Keeps track of whether the y-axis data is of the datetime type.
        /// Necessary because the datetime type must be treated differently from numeric types.
        /// </summary>
        public bool DatetimeYAxis { get; private set; }

        /// <summary>
        /// Stores the host name.
        /// </summary>
        private string host;

        /// <summary>
        /// Stores the MySql database name.
        /// </summary>
        private string dB;

        /// <summary>
        /// Stores all the MySql accounts.
        /// Bound to the datagrid.
        /// </summary>
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

        /// <summary>
        /// Stores all the tables in the selected MySql database.
        /// Bound to the table combobox in MainWindow.xaml.cs.
        /// </summary>
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

        /// <summary>
        /// Stores all the columns in the selected MySql table.
        /// Bound to the x and y-axis combobox in MainWindow.xaml.cs.
        /// </summary>
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

        /// <summary>
        /// Stores all the data in the selected x and y-axis MySql columns. 
        /// </summary>
        public List<Point> Data { get; set; } = new();

        /// <summary>
        /// Stores the connection string to connect to the database. 
        /// </summary>
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
                    dB = null;

                    Tables.Clear();
                    Columns.Clear();
                    Data.Clear();
                }

                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Keeps track of whether the login was successful.
        /// </summary>
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

        /// <summary>
        /// Attempts to login to the database.
        /// </summary>
        /// <param name="server"> The server name. </param>
        /// <param name="user"> The username. </param>
        /// <param name="password"> The password. </param>
        /// <param name="database"> The database name. </param>
        /// <returns> Whether the login was successful. </returns>
        public bool TryLogin (string server, string user, string password, string database)
        {
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
                    dB = database;
                }
                catch
                {
                    loginSuccessful = false;
                }
            }

            return loginSuccessful;
        }

        /// <summary>
        /// Gets the tables from the MySql database.
        /// </summary>
        public void GetTables ()
        {
            using MySqlConnection connection = new(ConnectionString);
            connection.Open();
            DataTable tablesInfo = connection.GetSchema("Tables");
            foreach (DataRow row in tablesInfo.Rows)
            {
                tables.Add(row[2].ToString());
            }
        }

        /// <summary>
        /// Gets the columns from the selected table.
        /// </summary>
        /// <param name="tableName"> The selected table name. </param>
        public void GetColumns (string tableName)
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

        /// <summary>
        /// Gets the type of the given column.
        /// </summary>
        /// <param name="tableName"> The name of the table that the column resides in. </param>
        /// <param name="columnName"> The name of the given column. </param>
        /// <returns> The given column type. </returns>
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

        /// <summary>
        /// Gets the data from the given columns and constraints.
        /// </summary>
        /// <param name="tableName"> The table name. </param>
        /// <param name="xColumn"> The x-axis column. </param>
        /// <param name="yColumn"> The y-axis column. </param>
        /// <param name="xMin"> The minimum x-axis value. </param>
        /// <param name="xMax"> The maximum x-axis value. </param>
        /// <returns> The data found from the columns with the given constraints. </returns>
        public List<Point> GetData (string tableName, string xColumn, string yColumn, string xMin, string xMax)
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

        /// <summary>
        /// Adds data to a MySql table.
        /// </summary>
        /// <param name="datatable"> The data to be added. </param>
        /// <param name="tableName"> The table name. </param>
        /// <returns></returns>
        public bool AddData (DataTable datatable, string tableName)
        {
            bool dataAdded = false;

            foreach (DataRow row in datatable.Rows)
            {

                // gets columns with entered data
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

                    // sets up command
                    using MySqlConnection connection = new(ConnectionString);
                    connection.Open();
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO " + tableName + "(" + enteredColumnsJoined + ") VALUES(" + columnValueNamesJoined + ")";

                    try
                    {
                        // adds parameters to command 
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

        /// <summary>
        /// Exports a MySql table with the given constraints.
        /// </summary>
        /// <param name="tableName"> The table name. </param>
        /// <param name="constraints"> The given constraints. </param>
        /// <returns> A string in CSV format. </returns>
        public string ExportData (string tableName, List<string> constraints = null, string sortBy = null, string order = null)
        {
            using MySqlConnection connection = new(ConnectionString);
            connection.Open();
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM " + tableName;
            if (constraints != null && constraints.Count != 0)
            {
                string constraintsJoined = string.Join(" ", constraints);
                command.CommandText += " WHERE " + constraintsJoined;
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                command.CommandText += $" ORDER BY {sortBy}";
            }

            if (!string.IsNullOrWhiteSpace(order))
            {
                if (order.ToLower() == "ascending")
                {
                    command.CommandText += " ASC";
                }
                else if (order.ToLower() == "descending")
                {
                    command.CommandText += " DESC";
                }
            }

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
                        if (!reader.IsDBNull(i))
                        {
                            values.Add(reader.GetString(i));
                        }
                        else
                        {
                            values.Add("NULL");
                        }
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

        /// <summary>
        /// Logs out of the MySql database.
        /// </summary>
        public void Logout ()
        {
            ConnectionString = null;
            Accounts.Clear();
        }

        /// <summary>
        /// Gets all the MySql accounts.
        /// </summary>
        public void GetAccounts ()
        {
            if (Accounts.Count == 0)
            {
                using MySqlConnection connection = new(ConnectionString);
                connection.Open();
                MySqlCommand getUsersCommand = connection.CreateCommand();
                getUsersCommand.CommandText = "SELECT Host, User FROM mysql.user WHERE User NOT LIKE 'mysql.%'";

                // gets all accounts
                using MySqlDataReader accountReader = getUsersCommand.ExecuteReader();
                while (accountReader.Read())
                {
                    Account account = new(host: accountReader.GetString(accountReader.GetOrdinal("Host")), user: accountReader.GetString(accountReader.GetOrdinal("User")));
                    Accounts.Add(account);
                }

                accountReader.Close();

                // gets privileges for each account
                foreach (Account account in Accounts)
                {
                    MySqlCommand getPrivilegeCommand = connection.CreateCommand();
                    getPrivilegeCommand.CommandText = $"SHOW GRANTS FOR '{account.User}'@'{account.Host}'";

                    using MySqlDataReader privilegeReader = getPrivilegeCommand.ExecuteReader();
                    while (privilegeReader.Read())
                    {
                        if ((privilegeReader.GetString(0).Contains("ALL") || privilegeReader.GetString(0).Contains("CREATE USER")) && privilegeReader.GetString(0).Contains("WITH GRANT OPTION") && (privilegeReader.GetString(0).Contains("*.*") || privilegeReader.GetString(0).Contains(dB)))
                        {
                            account.Privilege = Privilege.Admin;
                            break;
                        }
                        else if (privilegeReader.GetString(0).Contains("SELECT") && privilegeReader.GetString(0).Contains("INSERT") && (privilegeReader.GetString(0).Contains("*.*") || privilegeReader.GetString(0).Contains(dB)))
                        {
                            account.Privilege = Privilege.ReadAndWrite;
                            break;
                        }
                        else if (privilegeReader.GetString(0).Contains("SELECT") && (privilegeReader.GetString(0).Contains("*.*") || privilegeReader.GetString(0).Contains(dB)))
                        {
                            account.Privilege = Privilege.ReadOnly;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the privilege for the current user. 
        /// </summary>
        /// <returns> The current user's privilege. </returns>
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

        /// <summary>
        /// Gets the host of the current user.
        /// </summary>
        /// <returns> The current user's host. </returns>
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

        // create mysql account
        /// <summary>
        /// Creates a MySql account.
        /// </summary>
        /// <param name="user"> The username. </param>
        /// <param name="password"> The password. </param>
        /// <param name="privilege"> The privilege level. </param>
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
                grantPrivilegeCommand.CommandText = $"GRANT SELECT, INSERT, ALTER ON {dB}.* TO '{user}'@'{host}';";
            }
            else
            {
                grantPrivilegeCommand.CommandText = $"GRANT SELECT ON {dB}.* TO '{user}'@'{host}';";
            }

            grantPrivilegeCommand.CommandText += " FLUSH PRIVILEGES;";
            grantPrivilegeCommand.ExecuteNonQuery();

            Account account = new(host, user, privilege);
            Accounts.Add(account);
        }

        /// <summary>
        /// Modifies the privilege for an existing MySql account.
        /// </summary>
        /// <param name="host"> The host. </param>
        /// <param name="user"> The username. </param>
        /// <param name="privilege"> The privilege level. </param>
        public void AssignPrivilege (string host, string user, Privilege privilege)
        {
            using MySqlConnection connection = new(ConnectionString);
            connection.Open();

            MySqlCommand command = connection.CreateCommand();
            command.CommandText = $"REVOKE ALL, GRANT OPTION FROM '{user}'@'{host}';";
            if (privilege == Privilege.Admin)
            {
                command.CommandText += $" GRANT ALL ON *.* TO '{user}'@'{host}' WITH GRANT OPTION;";
                command.CommandText += $" GRANT SELECT ON mysql.user TO '{user}'@'{host}';";
            }
            else if (privilege == Privilege.ReadAndWrite)
            {
                command.CommandText += $" GRANT SELECT, INSERT, ALTER ON {dB}.* TO '{user}'@'{host}';";
            }
            else
            {
                command.CommandText += $" GRANT SELECT ON {dB}.* TO '{user}'@'{host}';";
            }
            command.CommandText += " FLUSH PRIVILEGES;";

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes the given MySql account.
        /// </summary>
        /// <param name="account"> The account to be deleted. </param>
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

        /// <summary>
        /// Adds the given column to a MySql table.
        /// </summary>
        /// <param name="tableName"> The table name. </param>
        /// <param name="columnName"> The column name. </param>
        /// <param name="columnType"> The column's type. </param>
        public void AddColumn (string tableName, string columnName, string columnType)
        {
            using MySqlConnection connection = new(ConnectionString);
            connection.Open();

            MySqlCommand command = connection.CreateCommand();
            command.CommandText = $"ALTER TABLE {tableName}";
            command.CommandText += $" ADD COLUMN {columnName} {columnType};";

            command.ExecuteNonQuery();
            columns.Add(columnName);
        }

        public void DeleteData (string tableName, List<string> constraints = null)
        {
            using MySqlConnection connection = new(ConnectionString);
            connection.Open();

            MySqlCommand command = connection.CreateCommand();
            if (constraints == null || constraints.Count == 0)
            {
                command.CommandText = $"TRUNCATE TABLE {tableName}";
            }
            else
            {
                string constraintsJoined = string.Join(" ", constraints);
                command.CommandText = $"DELETE FROM {tableName} WHERE {constraintsJoined}";
            }

            command.ExecuteNonQuery();
        }
    }
}