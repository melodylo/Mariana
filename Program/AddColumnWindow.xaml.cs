// File Name: AddColumnWindow.xaml.cs
// Author: Melody Lo
// Version number: 1.0
// Date published: Sept. 17, 2021
// Project name: Milano – Project Mariana
// Company/Division: KLA BBP-GPG Advanced Tech
// File Description: This file collects information to add a column to a MySql table.

using MySql.Data.MySqlClient;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Mariana
{
    /// <summary>
    /// Requests information to add a column to a MySql table.
    /// </summary>
    public partial class AddColumnWindow : Window, INotifyPropertyChanged
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
        /// Stores the MySql database name.
        /// </summary>
        public SqlDB SqlDB { get; set; }

        /// <summary>
        /// Stores the MySql table name.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Stores the column name.
        /// This is done automatically through databinding.
        /// </summary>
        private string columnName;
        public string ColumnName
        {
            get => columnName;
            set
            {
                columnName = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Stores the column's data type.
        /// This is done automatically through databinding.
        /// </summary>
        private string dataType;
        public string DataType
        {
            get => dataType;
            set
            {
                dataType = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Stores any extra information (eg. length for varchar) needed for the column's data type.
        /// This is done automatically through databinding.
        /// </summary>
        private string dataInput;
        public string DataInput
        {
            get => dataInput;
            set
            {
                dataInput = value;
                NotifyPropertyChanged();
            }
        }
        /// <summary>
        /// When showDataInput is set to true, the xaml element's Visibility property is set to visible.
        /// When showDataInput is set to false, the xaml element's Visibility property is set to collapsed.
        /// This is done automatically through databinding.
        /// </summary>
        private bool showDataInput;
        public bool ShowDataInput
        {
            get => showDataInput;
            set
            {
                showDataInput = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// When enableAdd is set to true, the xaml element's IsEnabled property is set to True.
        /// When enableAdd is set to false, the xaml element's IsEnabled property is set to False.
        /// This is done automatically through databinding.
        /// </summary>
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

        /// <summary>
        /// When validDataInput is set to true, the xaml element's Background property is set to a transparent color.
        /// When validDataInput is set to false, the xaml element's Background property is set to a red color.
        /// This is done automatically through databinding.
        /// </summary>
        private bool validDataInput = true;
        public bool ValidDataInput
        {
            get => validDataInput;
            set
            {
                validDataInput = value;
                if (!validDataInput)
                {
                    EnableAdd = false;
                }

                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes member variables and window.
        /// </summary>
        /// <param name="dB"> A reference to a SqlDB object. </param>
        /// <param name="tableName"> A string. </param>
        public AddColumnWindow (ref SqlDB dB, string tableName)
        {
            SqlDB = dB;
            TableName = tableName;

            InitializeComponent();
        }

        /// <summary>
        /// If all input fields are filled in, enable the Add button.
        /// Triggered when the text is changed for the text box that recieves the column name.
        /// </summary>
        /// <param name="sender"> A reference to the text box. </param>
        /// <param name="e"> Event data. </param>
        private void ColumnTextBoxTextChanged (object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ColumnName) && !string.IsNullOrWhiteSpace(DataType))
            {
                EnableAdd = !ShowDataInput || !string.IsNullOrWhiteSpace(DataInput);
            }
            else
            {
                EnableAdd = false;
            }
        }

        /// <summary>
        /// If all input fields are filled in, enable the Add button.
        /// Triggered when the selection is changed for the combo box that recieves the column's data type.
        /// </summary>
        /// <param name="sender"> A reference to the combo box. </param>
        /// <param name="e"> Event data. </param>
        private void TypeComboBoxSelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            if (DataType.ToLower() == "varchar")
            {
                if (ShowDataInput)
                {
                    if (!string.IsNullOrWhiteSpace(ColumnName))
                    {
                        EnableAdd = !string.IsNullOrWhiteSpace(DataInput);
                    }
                }
                else
                {
                    ShowDataInput = true;
                    EnableAdd = false;
                }
            }
            else
            {
                ShowDataInput = false;
                EnableAdd = !string.IsNullOrWhiteSpace(ColumnName);
            }
        }

        /// <summary>
        /// If all input fields are filled in, enable the Add button.
        /// Triggered when the text is changed for the text box that recieves extra information for the column's data type.
        /// </summary>
        /// <param name="sender"> A reference to the text box. </param>
        /// <param name="e"> Event data. </param>
        private void TypeTextBoxTextChanged (object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(DataInput))
            {
                if (int.TryParse(DataInput, out int result))
                {
                    if (result is >=1 and <= 255)
                    {
                        ValidDataInput = true;
                        EnableAdd = !string.IsNullOrWhiteSpace(ColumnName) && !string.IsNullOrWhiteSpace(DataInput) && !string.IsNullOrWhiteSpace(DataType);
                    }
                    else
                    {
                        ValidDataInput = false; // EnableAdd set to false
                    }
                }
                else
                {
                    ValidDataInput = false; // EnableAdd set to false
                }
            }
            else
            {
                ValidDataInput = true;
                EnableAdd = false;
            }
        }

        /// <summary>
        /// Adds a column to the MySql table when the Add button is clicked.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void AddClicked (object sender, RoutedEventArgs e)
        {
            string columnType = !string.IsNullOrWhiteSpace(DataInput) ? DataType + "(" + DataInput + ")" : DataType;
            try
            {
                SqlDB.AddColumn(TableName, ColumnName, columnType);
                MessageBox.Show("Column successfully added.", "Add Columnn");
                Close();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, "Add Column");
            }
        }
    }
}
