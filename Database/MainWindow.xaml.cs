using MySql.Data.MySqlClient;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Color = System.Windows.Media.Color;

namespace DatabaseToGraph
{
    public class Line : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged ([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ScatterPlot Plottable { get; set; }
        public ObservableCollection<Point> Points { get; set; } = new();
        public string YAxis { get; set; }

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

        public string Hex => color.ToString();

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

    public class Axis
    {
        public string Name { get; set; }
        public string Scale { get; set; }
        public List<Line> Lines { get; set; } = new();
    }

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged ([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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

        private bool enableExportData;
        public bool EnableExportData
        {
            get => enableExportData;
            set
            {
                enableExportData = value;
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

        private bool enableManageUsers;
        public bool EnableManageUsers
        {
            get => enableManageUsers;
            set
            {
                enableManageUsers = value;
                NotifyPropertyChanged();
            }
        }
        #endregion
              #region Show buttons
        private bool showManageUsers;
        public bool ShowManageUsers
        {
            get => showManageUsers;
            set
            {
                showManageUsers = value;
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
        #endregion
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

        private string dBName;
        private readonly List<Color> colorsUsed = new();
        private readonly List<Axis> yAxes = new();
        private readonly ScatterPlot hoveredPoint;
        private Privilege? privilege;

        private SqlDB dB = new();
        public SqlDB SqlDB
        {
            get => dB;
            set
            {
                dB = value;
                NotifyPropertyChanged();
            }
        }
        public ObservableCollection<Line> PlottedLines { get; } = new();

        public MainWindow ()
        {
            InitializeComponent();
            
            MinHeight = SystemParameters.PrimaryScreenHeight * 0.8;
            MinWidth = SystemParameters.PrimaryScreenWidth * 0.8;

            dB.PropertyChanged += new PropertyChangedEventHandler(DBPropertyChanged);

            legend.ItemsSource = PlottedLines;

            plot.Plot.YAxis.Label("Primary");
            plot.Plot.YAxis2.Label("Secondary");
            plot.Plot.YAxis2.Ticks(true);

            plot.Plot.Palette = ScottPlot.Drawing.Palette.Category10;

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

        private void DBPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(dB.ConnectionString))
            {
                if (!dB.LoginSuccessful)
                {
                    EnableLogin = true;

                    EnableAddLine = false;
                    EnableLogout = false;
                    EnableExportData = false;

                    EnableTable = false;
                    EnableXColumn = false;
                    EnableYColumn = false;

                    EnableXMin = false;
                    EnableXMax = false;

                    ShowAddData = false;
                    EnableAddData = false;

                    ShowManageUsers = false;
                    EnableManageUsers = false;
                }
            }
        }

        private bool Login (string server, string userID, string password, string dBName)
        {
            bool loginSuccessful = dB.TryLogin(server, userID, password, dBName);

            if (loginSuccessful)
            {
                dB.AddTables();

                privilege = SqlDB.GetPrivilege();
                if (privilege == Privilege.Admin)
                {
                    ShowManageUsers = true;
                    EnableManageUsers = true;

                    ShowAddData = true;
                }
                else if (privilege == Privilege.ReadAndWrite)
                {
                    ShowAddData = true;
                }
                else
                {
                    ShowManageUsers = false;
                    ShowAddData = false;
                }

                EnableLogout = true;
                EnableTable = true;

                EnableLogin = false;

                this.dBName = dBName;
            }

            return dB.LoginSuccessful;
        }

        private void LoginClicked (object sender, RoutedEventArgs e)
        {
            loginDialog loginDialog = new(ref dB);

            loginDialog.TryLogin += new loginDialog.LogIn(Login);

            loginDialog.ShowDialog();
        }

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
                Line line = MakeQuery(xMin, xMax);

                if (line != null)
                {
                    if (line.Points.Count != 0)
                    {
                        (double[] x, double[] y) = GetXY(line);
                        AddLine(x, y, line);

                        yColumnComboBox.SelectedIndex = -1;

                        xMinTextBox.Clear();
                        xMaxTextBox.Clear();
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

        private Line MakeQuery (string xMin, string xMax)
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

            line.Points = dB.GetData(tableName, xColumn, yColumn, xMin, xMax);

            if (line.Points != null)
            {
                if (line.Points.Count != 0)
                {
                    PlottedLines.Add(line);
                }

                return line;
            }
            else
            {
                return null;
            }
        }

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

        private static Color WindowsMediaColor (System.Drawing.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        private static System.Drawing.Color DrawingColor (Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        private void LogoutClicked (object sender, RoutedEventArgs e)
        {
            dB.Logout();

            EnableLogin = true;

            EnableLogout = false;
            EnableAddLine = false;
            EnableExportData = false;

            EnableTable = false;
            EnableXColumn = false;
            EnableYColumn = false;

            EnableXMin = false;
            EnableXMax = false;

            ShowAddData = false;
            EnableAddData = false;

            ShowManageUsers = false;
            EnableManageUsers = false;
        }

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
        }

        private void TableComboBoxSelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && tableComboBox.SelectedItem != null)
            {
                dB.AddColumns(tableComboBox.SelectedItem.ToString());

                EnableExportData = true;

                if (privilege != Privilege.ReadOnly)
                {
                    EnableAddData = true;
                }

                EnableXColumn = true;
                EnableYColumn = true;

                if (xColumnComboBox.SelectedItem != null && yColumnComboBox.SelectedItem != null)
                {
                    EnableAddLine = true;
                }
            }
        }

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

        private void LineVisibility (object sender, RoutedEventArgs e)
        {
            foreach (Line line in PlottedLines)
            {
                line.Plottable.IsVisible = line.Visible;
                plot.Render();
            }
        }

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
                }
                else if ((bool) secondaryRadioButton.IsChecked)
                {
                    axisName = "secondary";
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
                    plot.Plot.YAxis.MinorLogScale(true);
                    plot.Render();
                }
            }
        }

        private void MouseMovePlot (object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (PlottedLines.Count != 0)
            {
                if ((bool) showCoordinatesOn.IsChecked)
                {
                    (double mouseX, double mouseY) = plot.GetMouseCoordinates();
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
                            }
                            else if (line.YAxis == "secondary")
                            {
                                xyRatio = plot.Plot.XAxis2.Dims.PxPerUnit / plot.Plot.YAxis2.Dims.PxPerUnit;
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
                            }
                        }
                    }
                    hoveredPoint.Xs[0] = minX;
                    hoveredPoint.Ys[0] = minY;
                    hoveredPoint.IsVisible = true;
                    plot.Render();

                    hoveredPointXYText.Text = !dB.DatetimeXAxis && !dB.DatetimeYAxis
                        ? $"(x, y): ({minX:N2}, {minY:N2})"
                        : dB.DatetimeXAxis && !dB.DatetimeYAxis
                            ? $"(x, y): ({DateTime.FromOADate(minX)}, {minY:N2})"
                            : !dB.DatetimeXAxis && dB.DatetimeYAxis
                                ? $"(x, y): ({minX:N2}, {DateTime.FromOADate(minY)})"
                                : $"(x, y): ({DateTime.FromOADate(minX)}, {DateTime.FromOADate(minY)})";
                }
            }
        }

        private void DisableCoordinates (object sender, RoutedEventArgs e)
        {
            if (hoveredPoint != null)
            {
                hoveredPointXYText.Text = "";
                hoveredPoint.IsVisible = false;
                plot.Render();
            }
        }

        private void AddDataClicked (object sender, RoutedEventArgs e)
        {
            AddDataDialog addDataDialog = new(ref dB, tableComboBox.SelectedItem.ToString());
            addDataDialog.UpdateLines += new AddDataDialog.DataAdded(UpdateLines);
            addDataDialog.ShowDialog();
        }

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

                        x = dB.DatetimeXAxis ? Convert.ToDateTime(xValue).ToOADate() : Convert.ToDouble(xValue);
                        y = dB.DatetimeYAxis ? Convert.ToDateTime(yValue).ToOADate() : Convert.ToDouble(yValue);

                        Point point = new(x, y);
                        line.Points.Add(point);
                    }
                }

                (double[] xs, double[] ys) = GetXY(line);
                line.Plottable.Update(xs, ys);
            }

            EnableUpdate = true;
        }

        private void ExportDataClicked (object sender, RoutedEventArgs e)
        {
            ExportDataDialog exportDataDialog = new(dB, tableComboBox.SelectedItem.ToString());
            exportDataDialog.ShowDialog();
        }

        private void UpdateClicked (object sender, RoutedEventArgs e)
        { 
            plot.Render();
            EnableUpdate = false;
        }

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

        private void YColumnComboBoxSelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            if (tableComboBox.SelectedItem != null && xColumnComboBox.SelectedItem != null)
            {
                EnableAddLine = true;
            }
        }

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

        private void ManageUsersClicked (object sender, RoutedEventArgs e)
        {
            ManageUsersDialog manageUsersDialog = new(SqlDB);
            manageUsersDialog.ShowDialog();
        }
    }
}
