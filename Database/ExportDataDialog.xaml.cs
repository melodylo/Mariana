using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DatabaseToGraph
{
    public partial class ExportDataDialog : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged ([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SqlDB SqlDB { get; set; }
        public string TableName { get; set; }

        private ObservableCollection<string> constraints = new();
        public ObservableCollection<string> Constraints
        {
            get => constraints; 
            set
            {
                constraints = value;
                NotifyPropertyChanged();
            }
        }

        private bool enableAdd;
        public bool EnableAdd
        {
            get => enableAdd;
            set
            {
                enableAdd = value;
                NotifyPropertyChanged();
            }
        }

        private bool enableDelete;
        public bool EnableDelete
        {
            get => enableDelete;
            set
            {
                enableDelete = value;
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

        private bool enableMin;
        public bool EnableMin
        {
            get => enableMin;
            set
            {
                enableMin = value;
                NotifyPropertyChanged();
            }
        }

        private bool enableMax;
        public bool EnableMax
        {
            get => enableMax;
            set
            {
                enableMax = value;
                NotifyPropertyChanged();
            }
        }

        private bool validMin = true;
        public bool ValidMin
        {
            get => validMin;
            set
            {
                validMin = value;
                NotifyPropertyChanged();
            }
        }

        private bool validMax = true;
        public bool ValidMax
        {
            get => validMax;
            set
            {
                validMax = value;
                NotifyPropertyChanged();
            }
        }

        private bool showAndOr;
        public bool ShowAndOr
        {
            get => showAndOr;
            set
            {
                andOrComboBox.SelectedIndex = -1;
                showAndOr = value;
                NotifyPropertyChanged();
            }
        }

        public ExportDataDialog (SqlDB dB, string tableName)
        {
            SqlDB = dB;
            TableName = tableName;

            InitializeComponent();
        }

        private void ExportClicked (object sender, RoutedEventArgs e)
        {
            try
            {
                string text = SqlDB.ExportData(TableName, constraints.ToList());

                if (text == null)
                {
                    string message = "No data found with the requested constraints.";
                    string title = "Export Data";
                    MessageBox.Show(message, title);

                    constraints.Clear();
                    ShowAndOr = false;

                    return;
                }
                SaveFileDialog saveFileDialog = new();
                saveFileDialog.DefaultExt = ".csv";
                saveFileDialog.Filter = "CSV|*.csv";

                bool? saved = saveFileDialog.ShowDialog();
                if (saved == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, text);

                    Process process = new();
                    process.StartInfo = new ProcessStartInfo()
                    {
                        FileName = @"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE",
                        Arguments = saveFileDialog.FileName
                    };
                    process.Start();

                    constraints.Clear();
                    ShowAndOr = false;
                }
            }
            catch (MySqlException ex)
            {
                string title = "Export Data";
                MessageBox.Show(ex.Message, title);

                constraints.Clear();
                ShowAndOr = false;
            }
        }

        private void AddClicked (object sender, RoutedEventArgs e)
        {
            string columnName = columnComboBox.SelectedItem.ToString();
            if (!constraints.Any(constraint => constraint.Contains(columnName)))
            {
                string constraintName = "";

                if (ShowAndOr && andOrComboBox.SelectedItem != null)
                {
                    constraintName += andOrComboBox.Text + " ";
                }

                bool minNotEmpty = !string.IsNullOrWhiteSpace(minTextBox.Text);
                bool maxNotEmpty = !string.IsNullOrWhiteSpace(maxTextBox.Text);
                Type columnType = SqlDB.GetColumnType(TableName, columnName);

                if (minNotEmpty)
                {
                    if (maxNotEmpty)
                    {
                        if (columnType == typeof(DateTime))
                        {
                            constraintName += "(" + columnName + " >= '" + DateTime.Parse(minTextBox.Text).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                        }
                        else if (columnType == typeof(string))
                        {
                            constraintName += "(" + columnName + " >= '" + minTextBox.Text + "'";
                        }
                        else
                        {
                            constraintName += "(" + columnName + " >= " + minTextBox.Text;
                        }
                    }
                    else
                    {
                        if (columnType == typeof(DateTime))
                        {
                            constraintName += columnName + " >= '" + DateTime.Parse(minTextBox.Text).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                        }
                        else if (columnType == typeof(string))
                        {
                            constraintName += columnName + " >= '" + minTextBox.Text + "'";
                        }
                        else
                        {
                            constraintName += columnName + " >= " + minTextBox.Text;
                        }
                                
                    }
                        
                }

                if (maxNotEmpty)
                {
                    if (minNotEmpty)
                    {
                        if (columnType == typeof(DateTime))
                        {
                            constraintName += " AND " + columnName + " < '" + DateTime.Parse(maxTextBox.Text).ToString("yyyy-MM-dd HH:mm:ss") + "')";
                        }
                        else if (columnType == typeof(string))
                        {
                            constraintName += " AND " + columnName + " < '" + maxTextBox.Text + "')";
                        }
                        else
                        {
                            constraintName += " AND " + columnName + " < " + maxTextBox.Text + ")";
                        }
                               
                    }
                    else
                    {
                        if (columnType == typeof(DateTime))
                        {
                            constraintName += columnName + " < '" + DateTime.Parse(maxTextBox.Text).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                        }
                        else if (columnType == typeof(string))
                        {
                            constraintName += columnName + " < '" + maxTextBox.Text + "'";
                        }
                        else
                        {
                            constraintName += columnName + " < " + maxTextBox.Text;
                        }
                                
                    }
                        
                }

                constraints.Add(constraintName);

                columnComboBox.SelectedIndex = -1;
                andOrComboBox.SelectedIndex = -1;

                minTextBox.Clear();
                maxTextBox.Clear();

                EnableMin = false;
                EnableMax = false;
                EnableAdd = false;

                if (constraints.Count == 1)
                {
                    ShowAndOr = true;
                    EnableClear = true;
                }
            }
            else
            {
                string message = "Constraint already exists for this column.";
                string title = "Add Constraint";
                MessageBox.Show(message, title);
            }
        }

        private void MinTextBoxTextChanged (object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (columnComboBox.SelectedItem != null)
            {
                if (string.IsNullOrEmpty(minTextBox.Text))
                {
                    ValidMin = true;
                }
                else
                {
                    Type columnType = SqlDB.GetColumnType(TableName, columnComboBox.SelectedItem.ToString());

                    if (columnType == typeof(DateTime))
                    {
                        if (DateTime.TryParse(minTextBox.Text, out DateTime min))
                        {
                            if (!string.IsNullOrWhiteSpace(maxTextBox.Text) && DateTime.TryParse(maxTextBox.Text, out DateTime max))
                            {
                                if (min >= max)
                                {
                                    ValidMin = false;
                                    ValidMax = true;
                                    EnableAdd = false;
                                }
                                else
                                {
                                    if (ShowAndOr)
                                    {
                                        ValidMin = true;

                                        if (andOrComboBox.SelectedItem != null)
                                        {
                                            ValidMax = true;
                                            EnableAdd = true;
                                        }
                                    }
                                    else
                                    {
                                        ValidMin = true;
                                        ValidMax = true;
                                        EnableAdd = true;
                                    }
                                }
                            }
                            else
                            {
                                ValidMin = true;
                                if (ValidMax)
                                {
                                    if (ShowAndOr)
                                    {
                                        if (andOrComboBox.SelectedItem != null)
                                        {
                                            EnableAdd = true;
                                        }
                                    }
                                    else
                                    {
                                        EnableAdd = true;
                                    }  
                                }
                            }
                        }
                        else
                        {
                            ValidMin = false;
                            EnableAdd = false;
                        }
                    }
                    else
                    {
                        if (double.TryParse(minTextBox.Text, out double min))
                        {
                            if (!string.IsNullOrWhiteSpace(maxTextBox.Text) && double.TryParse(maxTextBox.Text, out double max))
                            {
                                if (min >= max)
                                {
                                    ValidMin = false;
                                    ValidMax = true;
                                    EnableAdd = false;
                                }
                                else
                                {
                                    if (ShowAndOr)
                                    {
                                        ValidMin = true;

                                        if (andOrComboBox.SelectedItem != null)
                                        {
                                            ValidMax = true;
                                            EnableAdd = true;
                                        }
                                    }
                                    else
                                    {
                                        ValidMin = true;
                                        ValidMax = true;
                                        EnableAdd = true;
                                    }
                                }
                            }
                            else
                            {
                                ValidMin = true;
                                if (ValidMax)
                                {
                                    if (ShowAndOr)
                                    {
                                        if (andOrComboBox.SelectedItem != null)
                                        {
                                            EnableAdd = true;
                                        }
                                    }
                                    else
                                    {
                                        EnableAdd = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            ValidMin = false;
                            EnableAdd = false;
                        }
                    }
                }
            }
        }

        private void MaxTextBoxTextChanged (object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (columnComboBox.SelectedItem != null)
            {
                if (string.IsNullOrWhiteSpace(maxTextBox.Text))
                {
                    ValidMax = true;
                }
                else
                {
                    Type columnType = SqlDB.GetColumnType(TableName, columnComboBox.SelectedItem.ToString());
                    if (columnType == typeof(DateTime))
                    {
                        if (DateTime.TryParse(maxTextBox.Text, out DateTime max))
                        {
                            if (!string.IsNullOrWhiteSpace(minTextBox.Text) && DateTime.TryParse(minTextBox.Text, out DateTime min))
                            {
                                if (max <= min)
                                {
                                    ValidMax = false;
                                    ValidMin = true;
                                    EnableAdd = false;
                                }
                                else
                                {
                                    if (ShowAndOr)
                                    {
                                        ValidMin = true;

                                        if (andOrComboBox.SelectedItem != null)
                                        {
                                            ValidMax = true;
                                            EnableAdd = true;
                                        }
                                    }
                                    else
                                    {
                                        ValidMin = true;
                                        ValidMax = true;
                                        EnableAdd = true;
                                    }
                                }
                            }
                            else
                            {
                                ValidMax = true;
                                if (ValidMin)
                                {
                                    if (ShowAndOr)
                                    {
                                        if (andOrComboBox.SelectedItem != null)
                                        {
                                            EnableAdd = true;
                                        }
                                    }
                                    else
                                    {
                                        EnableAdd = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            ValidMax = false;
                            EnableAdd = false;
                        }
                    }
                    else
                    {
                        if (double.TryParse(maxTextBox.Text, out double max))
                        {
                            if (!string.IsNullOrWhiteSpace(minTextBox.Text) && double.TryParse(minTextBox.Text, out double min))
                            {
                                if (max <= min)
                                {
                                    ValidMax = false;
                                    ValidMin = true;
                                    EnableAdd = false;
                                }
                                else
                                {
                                    if (ShowAndOr)
                                    {
                                        ValidMin = true;

                                        if (andOrComboBox.SelectedItem != null)
                                        {
                                            ValidMax = true;
                                            EnableAdd = true;
                                        }
                                        else
                                        {
                                            ValidMax = true;
                                        }
                                    }
                                    else
                                    {
                                        ValidMin = true;
                                        ValidMax = true;
                                        EnableAdd = true;
                                    }
                                }
                            }
                            else
                            {
                                ValidMax = true;
                                if (ValidMin)
                                {
                                    if (ShowAndOr)
                                    {
                                        if (andOrComboBox.SelectedItem != null)
                                        {
                                            EnableAdd = true;
                                        }
                                    }
                                    else
                                    {
                                        EnableAdd = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            ValidMax = false;
                            EnableAdd = false;
                        }
                    }
                }
            }
        }

        private void ColumnComboBoxSelectionChanged (object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (columnComboBox.SelectedItem != null)
            {
                if (andOrComboBox.Visibility == Visibility.Visible)
                {
                    if (andOrComboBox.SelectedItem != null)
                    {
                        EnableAdd = false;
                    }
                }

                EnableMin = true;
                EnableMax = true;

                minTextBox.Clear();
                ValidMin = true;

                maxTextBox.Clear();
                ValidMax = true;
            }
        }

        private void AndOrComboBoxSelectionChanged (object sender, EventArgs e)
        {
            if (columnComboBox.SelectedItem != null)
            {
                if (EnableMin && EnableMax)
                {
                    if (!string.IsNullOrWhiteSpace(minTextBox.Text) || !string.IsNullOrWhiteSpace(maxTextBox.Text))
                    {
                        if (minTextBox.Background == Brushes.Transparent && maxTextBox.Background == Brushes.Transparent)
                        {
                            EnableAdd = true;
                        }
                    }
                }
                else
                {
                    EnableMin = true;
                    EnableMax = true;
                    EnableAdd = false;
                }
            }
        }

        private void DeleteClicked (object sender, RoutedEventArgs e)
        {
            if (constraintsListBox.SelectedIndex == 0 && constraints.Count > 1)
            {
                constraints[1] = constraints[1].Replace("OR ", "");
                constraints[1] = constraints[1].Replace("AND", "");
            }

            constraints.Remove(constraintsListBox.SelectedItem.ToString());

            EnableDelete = false;
        }

        private void ClearClicked (object sender, RoutedEventArgs e)
        {  
            constraints.Clear();
            ShowAndOr = false;
            EnableClear = false;
        }

        private void ConstraintSelected (object sender, RoutedEventArgs e)
        {
            EnableDelete = true;
        }
    }
}
