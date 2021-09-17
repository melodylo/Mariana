// File Name: MainWindow.xaml.cs
// Author: Melody Lo
// Version number: 1.0
// Date published: Sept. 17, 2021
// Project name: Milano – Project Mariana
// Company/Division: KLA BBP-GPG Advanced Tech
// File Description: Plots data from database

using Mariana;
using MySql.Data.MySqlClient;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Color = System.Windows.Media.Color;

namespace Mariana
{
    /// <summary>
    /// Stores all the necessary data for a plotted line.
    /// </summary>
    public class Line : INotifyPropertyChanged
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
        /// Stores the plotted line as a ScatterPlot type.
        /// Used to easily hide/delete plotted lines.
        /// </summary>
        public ScatterPlot Plottable { get; set; }

        /// <summary>
        /// Stores the points that make up the line.
        /// </summary>
        public List<Point> Points { get; set; } = new();

        /// <summary>
        /// Stores which y-axis the line is plotted on.
        /// </summary>
        public string YAxis { get; set; }

        /// <summary>
        /// Stores the name of the line.
        /// The XAML listview automatically displays the line name through databinding.
        /// </summary>
        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Stores the name of the column used for the x-axis.
        /// </summary>
        private string xColumn;
        public string XColumn
        {
            get => xColumn;
            set
            {
                xColumn = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Stores the name of the column used for the y-axis.
        /// </summary>
        private string yColumn;
        public string YColumn
        {
            get => yColumn;
            set
            {
                yColumn = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets hex code from color.
        /// Used purely for databinding color to XAML listview.
        /// </summary>
        public string Hex => color.ToString();

        /// <summary>
        /// Stores the color of the plotted line.
        /// The XAML listview automatically displays the color of each line through databinding.
        /// </summary>
        private Color color;
        public Color Color
        {
            get => color;
            set
            {
                color = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Stores the visibility of the plotted line.
        /// Databound to XAML checkbox in listview.
        /// </summary>
        private bool visible = true;
        public bool Visible
        {
            get => visible;
            set
            {
                visible = value;
                NotifyPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Stores all the necessary data for each axis.
    /// </summary>
    public class Axis
    {
        /// <summary>
        /// Stores the name of the axis.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Stores the scale of the axis.
        /// </summary>
        public string Scale { get; set; }

        /// <summary>
        /// Stores all the lines that are plotted on the axis.
        /// </summary>
        public List<Line> Lines { get; set; } = new();
    }

    /// <summary>
    /// Houses all the main controls.
    /// Handles all ScottPlot commands for plotting lines.
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
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
        /// Stores the MySql database name.
        /// </summary>
        private string dBName;

        /// <summary>
        /// Keeps track of what colors have been used for the plotted lines to prevent colors from being repeated.
        /// </summary>
        private readonly List<Color> colorsUsed = new();

        /// <summary>
        /// Stores all the axes.
        /// </summary>
        private readonly List<Axis> yAxes = new();

        /// <summary>
        /// Highlights the nearest line coordinate to the mouse cursor.
        /// </summary>
        private readonly ScatterPlot hoveredPoint;

        /// <summary>
        /// Stores the privilege of the current user.
        /// </summary>
        private Privilege? privilege;
        public ObservableCollection<Line> PlottedLines { get; } = new();

        /// <summary>
        /// Stores a MySql database object for performing queries.
        /// </summary>
        private SqlDB sqlDB = new();
        public SqlDB SqlDB
        {
            get => sqlDB;
            set
            {
                sqlDB = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// When the Enable property is set to true, the XAML button's IsEnabled property is set to True.
        /// When the Enable property is set the false, the XAML button's IsEnabled property is set to False.
        /// This is done automatically through databinding.
        /// </summary>
        #region Enable buttons
        private bool enableLogin = true;
        public bool EnableLogin
        {
            get => enableLogin;
            set
            {
                enableLogin = value;
                NotifyPropertyChanged();
            }
        }

        private bool enableLogout;
        public bool EnableLogout
        {
            get => enableLogout;
            set
            {
                enableLogout = value;
                NotifyPropertyChanged();
            }
        }

        private bool enableAddLine;
        public bool EnableAddLine
        {
            get => enableAddLine;
            set
            {
                enableAddLine = value;
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

        private bool enableAddData;
        public bool EnableAddData
        {
            get => enableAddData;
            set
            {
                enableAddData = value;
                NotifyPropertyChanged();
            }
        }

        private bool enableExportArchive;
        public bool EnableExportArchive
        {
            get => enableExportArchive;
            set
            {
                enableExportArchive = value;
                NotifyPropertyChanged();
            }
        }

        private bool enableUpdate;
        public bool EnableUpdate
        {
            get => enableUpdate;
            set
            {
                enableUpdate = value;
                NotifyPropertyChanged();
            }
        }

        private bool enableManageAccounts;
        public bool EnableManageAccounts
        {
            get => enableManageAccounts;
            set
            {
                enableManageAccounts = value;
                NotifyPropertyChanged();
            }
        }

        private bool enableAddColumn;
        public bool EnableAddColumn
        {
            get => enableAddColumn;
            set
            {
                enableAddColumn = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// When the Show property is set to true, the XAML button's Visibility property is set to Visible.
        /// When the Show property is set to false, the XAML button's Visibility property is set to Collapsed.
        /// This is done automatically through databinding.
        /// </summary>
        #region Show buttons
        private bool showManageAccounts;
        public bool ShowManageAccounts
        {
            get => showManageAccounts;
            set
            {
                showManageAccounts = value;
                NotifyPropertyChanged();
            }
        }

        private bool showAddData;
        public bool ShowAddData
        {
            get => showAddData;
            set
            {
                showAddData = value;
                NotifyPropertyChanged();
            }
        }

        private bool showAddColumn;
        public bool ShowAddColumn
        {
            get => showAddColumn;
            set
            {
                showAddColumn = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// When the Enable property is set to true, the XAML combobox's IsEnabled property is set to True.
        /// When the Enable property is set to false, the XAML combobox's IsEnabled property is set to False.
        /// This is done automatically through databinding.
        /// </summary>
        #region Enable comboboxes
        private bool enableTable;
        public bool EnableTable
        {
            get => enableTable;
            set
            {
                enableTable = value;
                NotifyPropertyChanged();
            }
        }

        private bool enableXColumn;
        public bool EnableXColumn
        {
            get => enableXColumn;
            set
            {
                enableXColumn = value;
                NotifyPropertyChanged();
            }
        }

        private bool enableYColumn;
        public bool EnableYColumn
        {
            get => enableYColumn;
            set
            {
                enableYColumn = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// When the Enable property is set to true, the XAML textbox's IsEnabled property is set to True.
        /// When the Enable property is set to false, the XAML textbox's IsEnabled property is set to False.
        /// </summary>
        #region Enable textboxes
        private bool enableXMin;
        public bool EnableXMin
        {
            get => enableXMin;
            set
            {
                enableXMin = value;
                NotifyPropertyChanged();
            }
        }

        private bool enableXMax;
        public bool EnableXMax
        {
            get => enableXMax;
            set
            {
                enableXMax = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// When the Valid property is set to true, the XAML textbox's Background property is set to a transparent color.
        /// When the Valid property is set to false, the XAML textbox's Background property is set to a red color.
        /// This is done automatically through databinding.
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

        /// <summary>
        /// Initializes all member variables and window.
        /// </summary>
        public MainWindow ()
        {
            InitializeComponent();
            
            // sets window to 80% of screen
            MinHeight = SystemParameters.PrimaryScreenHeight * 0.8;
            MinWidth = SystemParameters.PrimaryScreenWidth * 0.8;

            sqlDB.PropertyChanged += new PropertyChangedEventHandler(DBPropertyChanged);

            plot.Plot.YAxis.Label("Primary");
            plot.Plot.YAxis2.Label("Secondary");
            plot.Plot.YAxis2.Ticks(true);

            Axis primary = new();
            primary.Name = "primary";
            primary.Scale = "linear";

            Axis secondary = new();
            secondary.Name = "secondary";
            primary.Scale = "linear";

            yAxes.Add(primary);
            yAxes.Add(secondary);

            hoveredPoint = plot.Plot.AddPoint(0, 0);
            hoveredPoint.Color = System.Drawing.Color.Black;
            hoveredPoint.MarkerSize = 10;
            hoveredPoint.MarkerShape = ScottPlot.MarkerShape.filledCircle;
            hoveredPoint.IsVisible = false;
        }

        /// <summary>
        /// Resets controls when the login is unsuccesful.
        /// Invoked when sqlDB's property is changed.
        /// </summary>
        /// <param name="sender"> A reference to the sqlDB object. </param>
        /// <param name="e"> EVent data. </param>
        private void DBPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(sqlDB.ConnectionString))
            {
                if (!sqlDB.LoginSuccessful)
                {
                    EnableLogin = true;

                    EnableAddLine = false;
                    EnableLogout = false;
                    EnableExportArchive = false;

                    EnableTable = false;
                    EnableXColumn = false;
                    EnableYColumn = false;

                    EnableXMin = false;
                    EnableXMax = false;

                    ShowAddData = false;
                    EnableAddData = false;

                    ShowAddColumn = false;
                    EnableAddColumn = false;

                    ShowManageAccounts = false;
                    EnableManageAccounts = false;

                    exportArchiveButton.Content = "Export";
                }
            }
        }

        /// <summary>
        /// Updates the controls when the login is successful.
        /// Called in LoginWindow.xaml.cs when the Log In button is clicked.
        /// </summary>
        /// <param name="server"> The server name. </param>
        /// <param name="userID"> The username. </param>
        /// <param name="password"> The password. </param>
        /// <param name="dBName"> The database name. </param>
        /// <returns> Whether the login was successful. </returns>
        private bool LogIn (string server, string userID, string password, string dBName)
        {
            bool loginSuccessful = sqlDB.TryLogin(server, userID, password, dBName);

            if (loginSuccessful)
            {
                sqlDB.GetTables();

                privilege = SqlDB.GetPrivilege();
                if (privilege == Privilege.Admin)
                {
                    ShowManageAccounts = true;
                    EnableManageAccounts = true;
                     
                    ShowAddData = true;
                    ShowAddColumn = true;

                    exportArchiveButton.Content = "Export/Archive";
                }
                else if (privilege == Privilege.ReadAndWrite)
                {
                    ShowAddData = true;
                    ShowAddColumn = true;

                    exportArchiveButton.Content = "Export/Archive";
                }
                else
                {
                    ShowManageAccounts = false;
                    ShowAddData = false;
                    ShowAddColumn = false;
                }

                EnableLogout = true;
                EnableTable = true;

                EnableLogin = false;

                this.dBName = dBName;
            }

            return sqlDB.LoginSuccessful;
        }

        /// <summary>
        /// Opens Login Window when the Login button is clicked.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void LoginClicked (object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new(ref sqlDB) { Owner = this };
            loginWindow.TryLogin += new LoginWindow.Login(LogIn);
            loginWindow.ShowDialog();
        }

        /// <summary>
        /// Verifies that the line can be plotted when the Add Line button is clicked.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void AddLineClicked (object sender, RoutedEventArgs e)
        {
            string tableName = tableComboBox.SelectedItem.ToString();
            string xAxis = xColumnComboBox.SelectedItem.ToString();
            string yAxis = yColumnComboBox.SelectedItem.ToString();
            string lineName =  dBName + "." + tableName + ": (" + xAxis + ", " + yAxis + ")";

            foreach (Line currentLine in PlottedLines)
            {
                if (currentLine.Name == lineName)
                {
                    string message = "This line already exists.";
                    string title = "Duplicate line";
                    MessageBox.Show(message, title);
                    return;
                }
            }
                        
            try
            {
                string xMin = xMinTextBox.Text;
                string xMax = xMaxTextBox.Text;
                Line line = GetLine(xMin, xMax);

                if (line != null)
                {
                    if (line.Points.Count != 0)
                    {
                        (double[] x, double[] y) = GetXY(line);
                        AddLine(x, y, line);

                        yColumnComboBox.SelectedIndex = -1;

                        xMinTextBox.Clear();
                        xMaxTextBox.Clear();

                        EnableAddLine = false;
                    }
                    else
                    {
                        string message = "No data found.";
                        string title = "Plot data";
                        MessageBox.Show(message, title);
                    }
                }
                else
                {
                    string message = "Cannot plot line.";
                    string title = "Graph";
                    MessageBox.Show(message, title);
                }
            }
            catch (MySqlException ex)
            {
                string title = "Graph";
                MessageBox.Show(ex.Message, title);
            }
        }

        /// <summary>
        /// converts Point to (double[], double[])
        /// </summary>
        /// <param name="line"> The line. </param>
        /// <returns> The points in (double[], double[]) format. </returns>
        private (double[], double[]) GetXY (Line line)
        {
            double[] x = new double[line.Points.Count];
            double[] y = new double[line.Points.Count];

            int index = 0;
            foreach (Point point in line.Points)
            {
                x[index] = point.X;
                y[index] = point.Y;
                index++;
            }
            return (x, y);
        }

        /// <summary>
        /// Creates a line.
        /// 
        /// </summary>
        /// <param name="xMin"> The minimum x-axis value. </param>
        /// <param name="xMax"> The maximum x-axis value. </param>
        /// <returns> The newly created line. </returns>
        private Line GetLine (string xMin, string xMax)
        {
            string tableName = tableComboBox.SelectedItem.ToString();
            string xColumn = xColumnComboBox.SelectedItem.ToString();
            string yColumn = yColumnComboBox.SelectedItem.ToString();
            string name = dBName + "." + tableName + ": (" + xColumn + ", " + yColumn + ")";

            Line line = new();
            line.Name = name;
            line.XColumn = xColumn;
            line.YColumn = yColumn;

            if ((bool) primaryRadioButton.IsChecked)
            {
                line.YAxis = "primary";
            }
            else if ((bool) secondaryRadioButton.IsChecked)
            {
                line.YAxis = "secondary";
            }

            Axis yAxis = yAxes.Find(axis => axis.Name == line.YAxis);
            yAxis.Lines.Add(line);

            if ((bool) linearRadioButton.IsChecked)
            {
                yAxis.Scale = "linear";
            }
            else if ((bool) logarithmicRadioButton.IsChecked)
            {
                yAxis.Scale = "logarithmic";
            }

            try
            {
                line.Points = new(sqlDB.GetData(tableName, xColumn, yColumn, xMin, xMax));

                if (line.Points.Count != 0)
                {
                    PlottedLines.Add(line);
                }

                return line;
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }

        /// <summary>
        /// Adds line to the plot.
        /// </summary>
        /// <param name="x"> All the x-axis points to be plotted. </param>
        /// <param name="y"> All the y-axis points to be plotted. </param>
        /// <param name="line"> The line's info. </param>
        private void AddLine (double[] x, double[] y, Line line)
        {
            string yAxis = line.YAxis;
            string yScale = yAxes.Find(axis => axis.Name == yAxis).Scale;

            Color color = WindowsMediaColor(plot.Plot.GetNextColor());
            ScottPlot.Drawing.Palette palette = ScottPlot.Drawing.Palette.Category10;
            if (colorsUsed.Contains(color))
            {
                for (int i = 0; i < palette.Count(); i++)
                {
                    color = WindowsMediaColor(palette.GetColor(i));
                    if (!colorsUsed.Contains(color))
                    {
                        colorsUsed.Add(color);
                        break;
                    }
                }
            }
            else
            {
                colorsUsed.Add(color);
            }


            ScatterPlot plottable = null;
            if (yScale == "linear")
            {
                plottable = plot.Plot.AddScatter(x, y, markerSize: 0, label: line.Name, color: DrawingColor(color));
                plot.Plot.YAxis.MinorLogScale(false);
            }
            else if (yScale == "logarithmic")
            {
                plottable = plot.Plot.AddScatter(x, ScottPlot.Tools.Log10(y), markerSize: 0, label: line.Name, color: DrawingColor(color));
                plot.Plot.YAxis.MinorLogScale(true);
            }
            line.Plottable = plottable;
            line.Color = color;

            if (yAxis == "primary")
            {
                plottable.YAxisIndex = 0;
                plot.Plot.YAxis.Color(plottable.Color);

                if (SqlDB.DatetimeYAxis)
                {
                    plot.Plot.YAxis.DateTimeFormat(true);
                }
                else
                {
                    plot.Plot.YAxis.DateTimeFormat(false);
                }
            }
            else if (yAxis == "secondary")
            {
                plottable.YAxisIndex = 1;
                plot.Plot.YAxis2.Color(plottable.Color);

                if (SqlDB.DatetimeYAxis)
                {
                    plot.Plot.YAxis2.DateTimeFormat(true);
                }
                else
                {
                    plot.Plot.YAxis2.DateTimeFormat(false);
                }
            }

            if (SqlDB.DatetimeXAxis)
            {
                plot.Plot.XAxis.DateTimeFormat(true);
            }
            else
            {
                plot.Plot.XAxis.DateTimeFormat(false);
            }

            EnableAddLine = false;
            EnableClear = true;
        }

        /// <summary>
        /// Converts System.Drawing.Color to System.Windows.Media.Color.
        /// ScottPlot uses System.Drawing.Color but XAML uses System.Windows.Media.Color.
        /// </summary>
        /// <param name="color"> The color of the line. </param>
        /// <returns> The line color in the correct type. </returns>
        private static Color WindowsMediaColor (System.Drawing.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Converts System.Windows.Media.Color to System.Drawing.Color.
        /// XAML uses System.Window.Media.Color but ScottPlot uses System.Drawing.Color.
        /// </summary>
        /// <param name="color"> The color of the line. </param>
        /// <returns> The line color in the correct type. </returns>
        private static System.Drawing.Color DrawingColor (Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Resets the controls when the Log Out button is clicked.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void LogoutClicked (object sender, RoutedEventArgs e)
        {
            sqlDB.Logout();

            EnableLogin = true;

            EnableLogout = false;
            EnableAddLine = false;
            EnableExportArchive = false;

            EnableTable = false;
            EnableXColumn = false;
            EnableYColumn = false;

            EnableXMin = false;
            EnableXMax = false;

            ShowAddData = false;
            EnableAddData = false;

            ShowAddColumn = false;
            EnableAddColumn = false;

            ShowManageAccounts = false;
            EnableManageAccounts = false;

            exportArchiveButton.Content = "Export";
        }

        /// <summary>
        /// Clears all lines from the plot when the Clear button is clicked.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void ClearClicked (object sender, RoutedEventArgs e)
        {
            plot.Plot.Clear();
            PlottedLines.Clear();

            foreach (Axis yAxis in yAxes)
            {
                yAxis.Lines.Clear();
            }

            plot.Plot.YAxis.Color(System.Drawing.Color.Black);
            plot.Plot.YAxis2.Color(System.Drawing.Color.Black);

            xColumnComboBox.SelectedIndex = -1;
            yColumnComboBox.SelectedIndex = -1;

            EnableClear = false;
            EnableUpdate = false;
            EnableAddLine = false;
        }

        /// <summary>
        /// Updates the controls when the table combobox's selection is modified.
        /// </summary>
        /// <param name="sender"> A reference to the combobox. </param>
        /// <param name="e"> Event data. </param>
        private void TableComboBoxSelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && tableComboBox.SelectedItem != null)
            {
                sqlDB.GetColumns(tableComboBox.SelectedItem.ToString());

                EnableExportArchive = true;

                if (privilege != Privilege.ReadOnly)
                {
                    EnableAddData = true;
                    EnableAddColumn = true;
                }

                EnableXColumn = true;
                EnableYColumn = true;

                if (xColumnComboBox.SelectedItem != null && yColumnComboBox.SelectedItem != null)
                {
                    EnableAddLine = true;
                }
            }
        }
        
        /// <summary>
        /// Displays a confirmation message before the window is closed.
        /// </summary>
        /// <param name="sender"> A reference to the window. </param>
        /// <param name="e"> Event data. </param>
        private void WindowClosing (object sender, CancelEventArgs e)
        {
            string message = "Close window?";
            string title = "Confirm close";
            MessageBoxResult result = MessageBox.Show(message, title, MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        // deletes selected line from plot
        /// <summary>
        /// Deletes the selected lines from the plot when the delete button is clicked.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void DeleteLineClicked (object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Line line = button.DataContext as Line;
            string name = line.Name;

            foreach (Line currentLine in PlottedLines)
            {
                if (currentLine.Name == name)
                {
                    plot.Plot.Remove(line.Plottable);

                    PlottedLines.Remove(currentLine);
                    colorsUsed.Remove(currentLine.Color);
                    Axis yAxis = yAxes.Find(axis => axis.Name == currentLine.YAxis);
                    yAxis.Lines.Remove(currentLine);
                    
                    break;
                }
            }

            plot.Plot.YAxis.Color(System.Drawing.Color.Black);
            plot.Plot.YAxis2.Color(System.Drawing.Color.Black);

            plot.Render();

            if (PlottedLines.Count == 0)
            {
                plot.Plot.Clear();

                EnableClear = false;
                EnableUpdate = false;
            }
        }

        /// <summary>
        /// Updates the plot when the XAML checkbox for a plotted line is checked/unchecked.
        /// </summary>
        /// <param name="sender"> A reference to the checkbox. </param>
        /// <param name="e"> Event data. </param>
        private void UpdateLegend (object sender, RoutedEventArgs e)
        {
            foreach (Line line in PlottedLines)
            {
                line.Plottable.IsVisible = line.Visible;
                plot.Render();
            }
        }

        /// <summary>
        /// Updates the plot when the scale of the y-axis is changed.
        /// Triggered when a radio button is selected.
        /// </summary>
        /// <param name="sender"> A reference to the radio button. </param>
        /// <param name="e"> Event data. </param>
        private void ChangeAxisScale (object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                RadioButton button = sender as RadioButton;
                string scale = button.Content.ToString().ToLower();
                string axisName = null;
                if ((bool) primaryRadioButton.IsChecked)
                {
                    axisName = "primary";

                    if (scale == "linear")
                    {
                        plot.Plot.YAxis.MinorLogScale(false);
                    }
                    else if (scale == "logarithmic")
                    {
                        plot.Plot.YAxis.MinorLogScale(true);
                    }
                }
                else if ((bool) secondaryRadioButton.IsChecked)
                {
                    axisName = "secondary";

                    if (scale == "linear")
                    {
                        plot.Plot.YAxis2.MinorLogScale(false);
                    }
                    else if (scale == "logarithmic")
                    {
                        plot.Plot.YAxis2.MinorLogScale(true);
                    }
                }
                Axis yAxis = yAxes.Find(axis => axis.Name == axisName);
                if (yAxis.Scale != scale)
                {
                    yAxis.Scale = scale;
                    foreach (Line line in yAxis.Lines)
                    {
                        plot.Plot.Remove(line.Plottable);
                        (double[] x, double[] y) = GetXY(line);
                        ScatterPlot plottable = null;
                        if (scale == "linear")
                        {
                            plottable = plot.Plot.AddScatter(x, y, markerSize: 0, label: line.Name, color: DrawingColor(line.Color));
                        }
                        else if (scale == "logarithmic")
                        {
                            plottable = plot.Plot.AddScatter(x, ScottPlot.Tools.Log10(y), markerSize: 0, label: line.Name, color: DrawingColor(line.Color));
                        }
                        line.Plottable = plottable;
                    }
                    plot.Render();
                }
            }
        }

        /// <summary>
        /// Displays the nearest line coordinate when the mouse moves on the plot.
        /// </summary>
        /// <param name="sender"> A reference to the mouse cursor. </param>
        /// <param name="e"> Event data. </param>
        private void MouseMovePlot (object sender, MouseEventArgs e)
        {
            if (PlottedLines.Count != 0)
            {
                if ((bool) showCoordinatesOn.IsChecked)
                {
                    ScottPlot.Settings settings = plot.Plot.GetSettings();
                    int pixelX = (int) e.MouseDevice.GetPosition(plot).X;
                    int pixelY = (int) e.MouseDevice.GetPosition(plot).Y;

                    (double mouseX, double mouseY) = (0, 0);
                    double minDistance = double.MaxValue;
                    double minX = 0;
                    double minY = 0;
                    foreach (Line line in PlottedLines)
                    {
                        if (line.Visible)
                        {
                            double xyRatio = 0;
                            if (line.YAxis == "primary")
                            {
                                xyRatio = plot.Plot.XAxis.Dims.PxPerUnit / plot.Plot.YAxis.Dims.PxPerUnit;

                                (mouseX, mouseY) = plot.GetMouseCoordinates();
                            }
                            else if (line.YAxis == "secondary")
                            {
                                xyRatio = plot.Plot.XAxis.Dims.PxPerUnit / plot.Plot.YAxis2.Dims.PxPerUnit;

                                (mouseX, mouseY) = (settings.XAxis.Dims.GetUnit(pixelX), settings.YAxis2.Dims.GetUnit(pixelY));
                            }
                            (double lineX, double lineY, _) = line.Plottable.GetPointNearest(mouseX, mouseY, xyRatio);

                            double xDiff = Math.Abs(lineX - mouseX);
                            double yDiff = Math.Abs(lineY - mouseY);

                            double x2 = Math.Pow(xDiff, 2);
                            double y2 = Math.Pow(yDiff, 2);

                            double distance = Math.Sqrt(x2 + y2);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                minX = lineX;
                                minY = lineY;

                                if (line.YAxis == "primary")
                                {
                                    hoveredPoint.YAxisIndex = 0;
                                }
                                else if (line.YAxis == "secondary")
                                {
                                    hoveredPoint.YAxisIndex = 1;
                                }
                            }
                        }
                    }
                    hoveredPoint.Xs[0] = minX;
                    hoveredPoint.Ys[0] = minY;
                    hoveredPoint.IsVisible = true;
                    plot.Render();

                    hoveredPointXYText.Text = !sqlDB.DatetimeXAxis && !sqlDB.DatetimeYAxis
                        ? $"(x, y): ({minX:N2}, {minY:N2})"
                        : sqlDB.DatetimeXAxis && !sqlDB.DatetimeYAxis
                            ? $"(x, y): ({DateTime.FromOADate(minX)}, {minY:N2})"
                            : !sqlDB.DatetimeXAxis && sqlDB.DatetimeYAxis
                                ? $"(x, y): ({minX:N2}, {DateTime.FromOADate(minY)})"
                                : $"(x, y): ({DateTime.FromOADate(minX)}, {DateTime.FromOADate(minY)})";
                }
            }
        }

        /// <summary>
        /// Updates the window when the Show Coordinates feature is turned off through a radio button.
        /// </summary>
        /// <param name="sender"> A reference to the radio button. </param>
        /// <param name="e"> Event data. </param>
        private void DisableCoordinates (object sender, RoutedEventArgs e)
        {
            if (hoveredPoint != null)
            {
                hoveredPointXYText.Text = "";
                hoveredPoint.IsVisible = false;
                plot.Render();
            }
        }

        /// <summary>
        /// Show the Add Data window when the Add Data button is clicked.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void AddDataClicked (object sender, RoutedEventArgs e)
        {
            AddDataWindow addDataWindow = new(ref sqlDB, tableComboBox.SelectedItem.ToString()) { Owner = this };
            addDataWindow.UpdateLines += new AddDataWindow.DataAdded(UpdateLines);
            addDataWindow.ShowDialog();
        }

        /// <summary>
        /// Inserts newly added data to the relevant lines.
        /// Does not update the plot automatically.
        /// </summary>
        /// <param name="datatable"> Contains the newly added data. </param>
        private void UpdateLines(DataTable datatable)
        {
            foreach(Line line in PlottedLines)
            {
                foreach (DataRow row in datatable.Rows)
                {
                    object xValue = row[line.XColumn];
                    object yValue = row[line.YColumn];
                    if (xValue != DBNull.Value && yValue != DBNull.Value)
                    {
                        double x;
                        double y;

                        x = sqlDB.DatetimeXAxis ? Convert.ToDateTime(xValue).ToOADate() : Convert.ToDouble(xValue);
                        y = sqlDB.DatetimeYAxis ? Convert.ToDateTime(yValue).ToOADate() : Convert.ToDouble(yValue);

                        Point point = new(x, y);
                        line.Points.Add(point);
                    }
                }

                (double[] xs, double[] ys) = GetXY(line);
                line.Plottable.Update(xs, ys);
            }

            if (PlottedLines.Count != 0)
            {
                EnableUpdate = true;
            }
        }

        /// <summary>
        /// Shows the Export Archive window when the Export/Archive button is clicked.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void ExportArchiveClicked (object sender, RoutedEventArgs e)
        {
            ExportArchiveWindow exportArchiveWindow = new(ref sqlDB, tableComboBox.SelectedItem.ToString(), privilege) { Owner = this };
            exportArchiveWindow.ShowDialog();
        }

        /// <summary>
        /// Updates the plot when the Update button is clicked.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void UpdateClicked (object sender, RoutedEventArgs e)
        { 
            plot.Render();
            EnableUpdate = false;
        }

        /// <summary>
        /// Updates the controls when the x-axis combobox's selection is changed.
        /// </summary>
        /// <param name="sender"> A reference to the combobox. </param>
        /// <param name="e"> Event data. </param>
        private void XColumnComboBoxSelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            EnableXMin = true;
            EnableXMax = true;

            xMinTextBox.Clear();
            xMaxTextBox.Clear();

            if (tableComboBox.SelectedItem != null && yColumnComboBox.SelectedItem != null)
            {
                EnableAddLine = true;
            }
        }

        /// <summary>
        /// Updates the controls when the y-axis combobox's selection is changed. 
        /// </summary>
        /// <param name="sender"> A reference to the combobox. </param>
        /// <param name="e"> Event data. </param>
        private void YColumnComboBoxSelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            if (tableComboBox.SelectedItem != null && xColumnComboBox.SelectedItem != null)
            {
                EnableAddLine = true;
            }
        }

        /// <summary>
        /// Performs error-checking based on the column's data type when the From textbox is modified.
        /// </summary>
        /// <param name="sender"> A reference to the textbox. </param>
        /// <param name="e"> Event data. </param>
        private void XMinTextBoxTextChanged (object sender, TextChangedEventArgs e)
        {
            if (xColumnComboBox.SelectedItem != null)
            {
                if (string.IsNullOrEmpty(xMinTextBox.Text))
                {
                    ValidMin = true;
                    if (ValidMax && yColumnComboBox.SelectedItem != null)
                    {
                        EnableAddLine = true;
                    }
                }
                else
                {
                    Type columnType = SqlDB.GetColumnType(tableComboBox.SelectedItem.ToString(), xColumnComboBox.SelectedItem.ToString());

                    if (columnType == typeof(DateTime))
                    {
                        if (DateTime.TryParse(xMinTextBox.Text, out DateTime min))
                        {
                            if (!string.IsNullOrWhiteSpace(xMaxTextBox.Text) && DateTime.TryParse(xMaxTextBox.Text, out DateTime max))
                            {
                                if (min >= max)
                                {
                                    ValidMin = false;
                                    ValidMax = true;
                                    EnableAddLine = false;
                                }
                                else
                                {
                                    ValidMin = true;
                                    ValidMax = true;
                                    if (yColumnComboBox.SelectedItem != null)
                                    {
                                        EnableAddLine = true;
                                    }
                                }
                            }
                            else
                            {
                                ValidMin = true;
                                if (ValidMin && yColumnComboBox.SelectedItem != null)
                                {
                                    EnableAddLine = true;
                                }
                            }
                        }
                        else
                        {
                            ValidMin = false;
                            EnableAddLine = false;
                        }
                    }
                    else
                    {
                        if (double.TryParse(xMinTextBox.Text, out double min))
                        {
                            if (!string.IsNullOrWhiteSpace(xMaxTextBox.Text) && double.TryParse(xMaxTextBox.Text, out double max))
                            {
                                if (min >= max)
                                {
                                    ValidMin = false;
                                    ValidMax = true;
                                    EnableAddLine = false;
                                }
                                else
                                {
                                    ValidMin = true;
                                    ValidMax = true;
                                    if (yColumnComboBox.SelectedItem != null)
                                    {
                                        EnableAddLine = true;
                                    }
                                }
                            }
                            else
                            {
                                ValidMin = true;
                                if (ValidMin && yColumnComboBox.SelectedItem != null)
                                {
                                    EnableAddLine = true;
                                }
                            }
                        }
                        else
                        {
                            ValidMin = false;
                            EnableAddLine = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Performs error-checking based on the column's data type when the To textbox is modified.
        /// </summary>
        /// <param name="sender"> A reference to the textbox. </param>
        /// <param name="e"> Event data. </param>
        private void XMaxTextBoxTextChanged (object sender, TextChangedEventArgs e)
        {

            if (xColumnComboBox.SelectedItem != null)
            {
                if (string.IsNullOrWhiteSpace(xMaxTextBox.Text))
                {
                    ValidMax = true;
                    if (ValidMin && yColumnComboBox.SelectedItem != null)
                    {
                        EnableAddLine = true;
                    }
                }
                else
                {
                    Type columnType = SqlDB.GetColumnType(tableComboBox.SelectedItem.ToString(), xColumnComboBox.SelectedItem.ToString());
                    if (columnType == typeof(DateTime))
                    {
                        if (DateTime.TryParse(xMaxTextBox.Text, out DateTime max))
                        {
                            if (!string.IsNullOrWhiteSpace(xMinTextBox.Text) && DateTime.TryParse(xMinTextBox.Text, out DateTime min))
                            {
                                if (max <= min)
                                {
                                    ValidMax = false;
                                    ValidMin = true;
                                    EnableAddLine = false;
                                }
                                else
                                {
                                    ValidMin = true;
                                    ValidMax = true;
                                    if (yColumnComboBox.SelectedItem != null)
                                    {
                                        EnableAddLine = true;
                                    }
                                }
                            }
                            else
                            {
                                ValidMax = true;
                                if (ValidMin && yColumnComboBox.SelectedItem != null)
                                {
                                    EnableAddLine = true;
                                }
                            }
                        }
                        else
                        {
                            ValidMax = false;
                            EnableAddLine = false;
                        }
                    }
                    else
                    {
                        if (double.TryParse(xMaxTextBox.Text, out double max))
                        {
                            if (!string.IsNullOrWhiteSpace(xMinTextBox.Text) && double.TryParse(xMinTextBox.Text, out double min))
                            {
                                if (max <= min)
                                {
                                    ValidMax = false;
                                    ValidMin = true;
                                    EnableAddLine = false;
                                }
                                else
                                {
                                    ValidMin = true;
                                    ValidMax = true;
                                    if (yColumnComboBox.SelectedItem != null)
                                    {
                                        EnableAddLine = true;
                                    }
                                }
                            }
                            else
                            {
                                ValidMax = true;
                                if (ValidMin && yColumnComboBox.SelectedItem != null)
                                {
                                    EnableAddLine = true;
                                }
                            }
                        }
                        else
                        {
                            ValidMax = false;
                            EnableAddLine = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Shows the Manage Accounts Window when the Manage Accounts button is clicked.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void ManageAccountsClicked (object sender, RoutedEventArgs e)
        {
            ManageAccountsWindow manageAccountsWindow = new(ref sqlDB) { Owner = this };
            manageAccountsWindow.ShowDialog();
        }

        /// <summary>
        /// Shows the Add Column window when the Add Column button is clicked.
        /// </summary>
        /// <param name="sender"> A reference to the button. </param>
        /// <param name="e"> Event data. </param>
        private void AddColumnClicked (object sender, RoutedEventArgs e)
        {
            AddColumnWindow addColumnWindow = new(ref sqlDB, tableComboBox.SelectedItem.ToString());
            addColumnWindow.ShowDialog();
        }

        private void ShowCoordinatesOnChecked (object sender, RoutedEventArgs e)
        {
            hoveredPoint.IsVisible = true;
        }
    }
}
