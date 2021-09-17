// File Name: AddDataWindow.xaml.cs
// Author: Melody Lo
// Version number: 1.0
// Date published: Sept. 17, 2021
// Project name: Milano – Project Mariana
// Company/Division: KLA BBP-GPG Advanced Tech
// File Description: Gets data to add to a table.

using Mariana;
using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Mariana
{
    /// <summary>
    /// Collects data to add to a MySql table
    /// </summary>
    public partial class AddDataWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// A required event for INotifyPropertyChanged.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invoked after a databound property is set.
        /// </summary>
        /// <param name="propertyName"> The CallerMemberName attribute automatically sets propertyName to the property name of the caller. </param>
        private void NotifyPropertyChanged ([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Used for communicating to MainWindow.xaml.cs after data has been added to the table.
        /// </summary>
        /// <param name="datatable"></param>
        public delegate void DataAdded (DataTable datatable);

        /// <summary>
        /// Method defined in MainWindow.xaml.cs.
        /// </summary>
        public DataAdded UpdateLines { get; set; }

        /// <summary>
        /// Stores the MySql table name.
        /// </summary>
        private string TableName { get; set; }

        /// <summary>
        /// Stores the MySql database name.
        /// </summary>
        private SqlDB SqlDB { get; set; }

        /// <summary>
        /// Stores the data to be added to the MySql table. 
        /// This is done automatically through databinding.
        /// </summary>
        private DataTable datatable = new();
        public DataTable Datatable
        {
            get => datatable;
            set
            {
                datatable = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// When enableDeleteRow is set to true, the XAML button's IsEnabled property is set to True.
        /// When enableDeleteRow is set to false, the XAML button's IsEnabled property is set to False.
        /// This is done automatically through databinding.
        /// </summary>
        private bool enableDeleteRow;
        public bool EnableDeleteRow
        {
            get => enableDeleteRow;
            set
            {
                enableDeleteRow = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// When enableClear is set to true, the XAML button's IsEnabled property is set to True.
        /// When enableClear is set to false, the XAML button's IsEnabled property is set to False.
        /// This is done automatically through databinding.
        /// </summary>
        private bool enableClear;
        public bool EnableClear
        {
            get => enableClear;
            set
            {
                enableClear = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes member variables and window.
        /// Sets column names in DataTable.
        /// </summary>
        /// <param name="dB"> A reference to a SqlDB object. </param>
        /// <param name="tableName"> A string. </param>
        public AddDataWindow(ref SqlDB dB, string tableName)
        {
            InitializeComponent();

            TableName = tableName;
            SqlDB = dB;

            foreach (string columnName in SqlDB.Columns)
            {
                Datatable.Columns.Add(columnName);
            }
        }

        /// <summary>
        /// Adds data to the MySql table when the Submit Data button is clicked.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void SubmitDataClicked(object sender, RoutedEventArgs e)
        {
            bool dataAdded = SqlDB.AddData(Datatable, TableName);
            if (dataAdded)
            {
                UpdateLines(Datatable); // calls UpdateLines in MainWindow.xaml.cs

                string message = "Data successfully added.";
                string title = "Add Data";
                MessageBox.Show(message, title);

                Close();
            }
        }

        /// <summary>
        /// Adds an empty row to Datatable when the Add Row button is clicked.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void AddRowClicked(object sender, RoutedEventArgs e)
        {
            Datatable.Rows.Add();
            EnableClear = true;
        }

        /// <summary>
        /// Imports csv file to Datatable when the Import button is clicked.
        /// Displays error message if the file cannot be read.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void ImportClicked (object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.DefaultExt = ".csv";
            openFileDialog.Filter = "CSV|*.csv";

            bool? opened = openFileDialog.ShowDialog();
            if (opened == true)
            {
                try
                {
                    foreach (string line in File.ReadLines(openFileDialog.FileName).Skip(1)) // skips first line (column names) in csv file
                    {
                        List<string> data = line.Split(",").ToList();
                        DataRow row = Datatable.NewRow();
                        for (int i = 0; i < data.Count; i++)
                        {
                            try
                            {
                                row[i] = data[i];
                            }
                            catch (System.IndexOutOfRangeException)
                            {
                            }
                        }
                        Datatable.Rows.Add(row);
                    }

                    EnableClear = true;
                }
                catch (IOException ex)
                {
                    string title = "Import";
                    MessageBox.Show(ex.Message, title);
                }
            }
        }

        /// <summary>
        /// The Delete Row button is enabled when a row is selected in the XAML data grid. 
        /// </summary>
        /// <param name="sender"> A reference to the data grid. </param>
        /// <param name="e"> Event data. </param>
        private void RowSelected (object sender, RoutedEventArgs e)
        {
            EnableDeleteRow = true;
        }

        /// <summary>
        /// A row is deleted from Datatable when the Delete Row button is clicked.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void DeleteRowClicked (object sender, RoutedEventArgs e)
        {
            Datatable.Rows.Remove((datagrid.SelectedItem as DataRowView).Row);
            EnableDeleteRow = false;

            if (Datatable.Rows.Count == 0)
            {
                EnableClear = false;
            }
        }

        /// <summary>
        /// Datatable is cleared when the Clear button is clicked.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void ClearClicked (object sender, RoutedEventArgs e)
        {
            Datatable.Clear();
            EnableClear = false;
        }
    }
}
