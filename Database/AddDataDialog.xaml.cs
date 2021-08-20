using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DatabaseToGraph
{
    public partial class AddDataDialog : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged ([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public delegate void DataAdded (DataTable datatable);
        public DataAdded UpdateLines { get; set; }
        private string TableName { get; set; }

        private SqlDB SqlDB { get; set; }

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

        public AddDataDialog(ref SqlDB dB, string tableName)
        {
            InitializeComponent();

            TableName = tableName;

            SqlDB = dB;
            foreach (string columnName in SqlDB.Columns)
            {
                Datatable.Columns.Add(columnName);
            }
        }

        private void SubmitDataClicked(object sender, RoutedEventArgs e)
        {
            bool dataAdded = SqlDB.AddData(Datatable, TableName);
            if (dataAdded)
            {
                UpdateLines(Datatable);
                Close();
            }
        }

        private void AddRowClicked(object sender, RoutedEventArgs e)
        {
            Datatable.Rows.Add();
            EnableClear = true;
        }

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
                    foreach (string line in File.ReadLines(openFileDialog.FileName).Skip(1))
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

        private void RowSelected (object sender, RoutedEventArgs e)
        {
            EnableDeleteRow = true;
        }

        private void DeleteRowClicked (object sender, RoutedEventArgs e)
        {
            Datatable.Rows.Remove((datagrid.SelectedItem as DataRowView).Row);
            EnableDeleteRow = false;

            if (Datatable.Rows.Count == 0)
            {
                EnableClear = false;
            }
        }

        private void ClearClicked (object sender, RoutedEventArgs e)
        {
            Datatable.Clear();
            EnableClear = false;
        }
    }
}
