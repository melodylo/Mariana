// File Name: ExportWindow.xaml.cs
// Author: Melody Lo
// Version number: 1.0
// Date published: Sept. 17, 2021
// Project name: Milano – Project Mariana
// Company/Division: KLA BBP-GPG Advanced Tech
// File Description: Requests constraints from user 

using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace Mariana
{
    /// <summary>
    /// Exports data to csv file.
    /// </summary>
    public partial class ExportArchiveWindow : Window, INotifyPropertyChanged
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
        /// Stores the MySql database.
        /// </summary>
        public SqlDB SqlDB { get; set; }

        /// <summary>
        /// Stores the MySql table name.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Stores the constraints for the MySql query.
        /// This is done automatically through databinding.
        /// </summary>
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

        /// <summary>
        /// When the Enable property is set to true, the XAML button's IsEnabled property is set to True.
        /// When the Enable property is set to false, the XAML button's IsEnabled property is set to False.
        /// This is done automatically through databinding. 
        /// </summary>
        #region Enable buttons
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
        #endregion

        /// <summary>
        /// When the Enable property is set to true, the XAML textbox's IsEnabled property is set to True.
        /// When the Enable property is set to false, the XAML textbox's IsEnabled property is set to False.
        /// This is done automatically through databinding.
        /// </summary>
        #region Enable textboxes
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
        #endregion

        /// <summary>
        /// When the Valid property is set to true, the XAML textbox's Background property is set to a transparent color.
        /// When the Valid property is set to false, the XAML textbox's Background property is set to a red color.
        /// </summary>
        #region Validate textboxes
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
        #endregion

        // When the Show property set to true, UI combobox's visibility automatically set to Visible.
        // When the Show property set to false, UI combobox's visibility automatically set to Collapsed.
        #region Show comboboxes
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
        #endregion

        /// <summary>
        /// Initializes member variables and window.
        /// </summary>
        /// <param name="dB"> A reference to a MySql database. </param>
        /// <param name="tableName"> A string. </param>
        public ExportArchiveWindow (ref SqlDB dB, string tableName, Privilege? privilege)
        {
            SqlDB = dB;
            TableName = tableName;

            InitializeComponent();

            if (privilege == Privilege.ReadOnly)
            {

                archiveButton.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Exports the data into a csv file when the Export button is clicked.
        /// Displays an error message if no data is found with the given constraints. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportClicked (object sender, RoutedEventArgs e)
        {
            try
            {
                string sortBy = null;
                string order = null;
                if (sortByComboBox.SelectedItem != null)
                {
                    sortBy = sortByComboBox.SelectedItem.ToString();
                }
                if (orderComboBox.SelectedItem != null)
                {
                    order = orderComboBox.SelectedValue.ToString();
                }

                string text = SqlDB.ExportData(TableName, constraints.ToList(), sortBy, order);

                if (text == null)
                {
                    string message = "No data found with the requested constraints.";
                    string title = "Export Data";
                    MessageBox.Show(message, title);

                    return;
                }
                SaveFileDialog saveFileDialog = new();
                saveFileDialog.DefaultExt = ".csv";
                saveFileDialog.Filter = "CSV|*.csv";

                bool? saved = saveFileDialog.ShowDialog();
                if (saved == true)
                {
                    try
                    {
                        File.WriteAllText(saveFileDialog.FileName, text);
                        Close();

                        // opens saved file with excel
                        Process process = new();
                        process.StartInfo = new ProcessStartInfo()
                        {
                            FileName = @"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE",
                            Arguments = saveFileDialog.FileName
                        };
                        process.Start();
                        

                    }
                    catch (IOException ex)
                    {
                        string title = "Export Data";
                        MessageBox.Show(ex.Message, title);
                    }
                }
            }
            catch (MySqlException ex)
            {
                string title = "Export Data";
                MessageBox.Show(ex.Message, title);
            }
        }

        /// <summary>
        /// The entered constraint is saved into constraints when the Add button is clicked.
        /// Displays an error message if the constraint already exists.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
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

        /// <summary>
        /// Error-checking is done based on the column's data type when the From textbox is modified.
        /// </summary>
        /// <param name="sender"> A reference to the textbox. </param>
        /// <param name="e"> Event data. </param>
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
        
        /// <summary>
        /// Error checking is done based on the column's data type when the To textbox is modified.
        /// </summary>
        /// <param name="sender"> A reference to the textbox. </param>
        /// <param name="e"> Event data. </param>
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

        /// <summary>
        /// Update the controls when the column combobox's selection is changed. 
        /// </summary>
        /// <param name="sender"> A reference to the combobox. </param>
        /// <param name="e"> Event data. </param>
        private void ColumnComboBoxSelectionChanged (object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            EnableAdd = false;

            EnableMin = true;
            EnableMax = true;

            minTextBox.Clear();
            ValidMin = true;

            maxTextBox.Clear();
            ValidMax = true;
        }

        /// <summary>
        /// Updates the controls when the and-or combobox's selection is changed. 
        /// </summary>
        /// <param name="sender"> A reference to the combobox. </param>
        /// <param name="e"> Event data. </param>
        private void AndOrComboBoxSelectionChanged (object sender, EventArgs e)
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
        }

        /// <summary>
        /// Deletes the selected constraint when the Delete button is clicked. 
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void DeleteClicked (object sender, RoutedEventArgs e)
        {
            if (constraintsListBox.SelectedIndex == 0 && constraints.Count > 1)
            {
                constraints[1] = constraints[1].Replace("OR ", "");
                constraints[1] = constraints[1].Replace("AND", "");
            }
            else if (constraints.Count == 1)
            {
                ShowAndOr = false;
            }

            constraints.Remove(constraintsListBox.SelectedItem.ToString());

            EnableDelete = false;
        }

        /// <summary>
        /// Clears all constraints when the Clear button is clicked. 
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void ClearClicked (object sender, RoutedEventArgs e)
        {  
            constraints.Clear();
            ShowAndOr = false;
            EnableClear = false;
        }

        /// <summary>
        /// Enables the Delete button when a constraint is selected in the XAML listbox.
        /// </summary>
        /// <param name="sender"> A reference to the listbox. </param>
        /// <param name="e"> Event data. </param>
        private void ConstraintSelected (object sender, RoutedEventArgs e)
        {
            EnableDelete = true;
        }

        /// <summary>
        /// Exports the data into a csv file and deletes the data from the table when the Archive button is clicked.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void ArchiveClicked (object sender, RoutedEventArgs e)
        {
            try
            {
                string sortBy = null;
                string order = null;
                if (sortByComboBox.SelectedItem != null)
                {
                    sortBy = sortByComboBox.SelectedItem.ToString();
                }
                if (orderComboBox.SelectedItem != null)
                {
                    order = orderComboBox.SelectedItem.ToString();
                }

                string text = SqlDB.ExportData(TableName, constraints.ToList(), sortBy, order);

                if (text == null)
                {
                    string message = "No data found with the requested constraints.";
                    string title = "Archive Data";
                    MessageBox.Show(message, title);

                    return;
                }
                SaveFileDialog saveFileDialog = new();
                saveFileDialog.DefaultExt = ".csv";
                saveFileDialog.Filter = "CSV|*.csv";

                bool? saved = saveFileDialog.ShowDialog();
                if (saved == true)
                {
                    try
                    {
                        File.WriteAllText(saveFileDialog.FileName, text);
                        SqlDB.DeleteData(TableName, constraints.ToList());
                        Close();

                        // opens saved file with excel
                        Process process = new();
                        process.StartInfo = new ProcessStartInfo()
                        {
                            FileName = @"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE",
                            Arguments = saveFileDialog.FileName
                        };
                        process.Start();
                    }
                    catch (IOException ex)
                    {
                        string title = "Archive Data";
                        MessageBox.Show(ex.Message, title);
                    }
                }
            }
            catch (MySqlException ex)
            {
                string title = "Archive Data";
                MessageBox.Show(ex.Message, title);
            }
        }
    }
}
