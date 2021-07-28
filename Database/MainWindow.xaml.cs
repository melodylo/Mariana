using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

namespace DatabaseToGraph
{
    public class Line : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
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

        public string Hex => color.ToString();

        public Color color;
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

        private void NotifyPropertyChanged ([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        private string databaseName;
        private readonly List<Color> colorsUsed = new();
        private readonly List<Axis> yAxes = new();
        private readonly ScatterPlot hoveredPoint;

        private SqlDB database = new();
        public SqlDB Database 
        { 
            get => database; 
            set
            {
                database = value;
                NotifyPropertyChanged();
            }
        }
        public ObservableCollection<Line> PlottedLines { get; } = new();

        #region Buttons
        private bool connectButton_enable = true;
        public bool EnableConnectButton
        {
            get => connectButton_enable;
            set
            {
                connectButton_enable = value;
                NotifyPropertyChanged();
            }
        }

        private bool disconnectButton_enable;
        public bool EnableDisconnectButton
        {
            get => disconnectButton_enable;
            set
            {
                disconnectButton_enable = value;
                NotifyPropertyChanged();
            }
        }

        private bool addLineButton_enable;
        public bool EnableAddLineButton
        {
            get => addLineButton_enable;
            set
            {
                addLineButton_enable = value;
                NotifyPropertyChanged();
            }
        }

        private bool clearButton_enable;
        public bool EnableClearButton
        {
            get => clearButton_enable;
            set
            {
                clearButton_enable = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        public MainWindow ()
        {
            InitializeComponent();

            database.PropertyChanged += new PropertyChangedEventHandler(DatabasePropertyChanged);

            listView_plottedLines.ItemsSource = PlottedLines;

            plot.Plot.YAxis.Label("Primary");
            plot.Plot.YAxis2.Label("Secondary");
            plot.Plot.YAxis2.Ticks(true);

            plot.Plot.Palette = ScottPlot.Drawing.Palette.Category20;

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

        private void DatabasePropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(database.Connection))
            {
                if (!database.ConnectionSuccessful)
                {
                    EnableAddLineButton = false;
                    EnableDisconnectButton = false;
                    EnableConnectButton = true;
                }
            }
        }

        private bool TestConnection (string server, string userID, string password, string databaseName)
        {
            bool connectionSuccessful = database.TestConnection(server, userID, password, databaseName);

            if (connectionSuccessful)
            {
                database.AddTables();

                EnableAddLineButton = true;
                EnableDisconnectButton = true;
                EnableConnectButton = false;

                this.databaseName = databaseName;
            }

            return database.ConnectionSuccessful;
        }

        private void ButtonClick_Connect (object sender, RoutedEventArgs e)
        {
            ConnectionDialog connectionDialog = new(ref database);

            // get info for sql connection string
            connectionDialog.TestConnectionClicked += new ConnectionDialog.TestConnection(TestConnection);

            connectionDialog.ShowDialog();
        }

        private void ButtonClick_AddLine (object sender, RoutedEventArgs e)
        {
            if (comboBox_tables.SelectedItem != null)
            {
                if (comboBox_xColumn.SelectedItem != null)
                {
                    if (comboBox_yColumn.SelectedItem != null)
                    {
                        (string xMin, string xMax) = Assign_xMinMax();
                        if (Valid_xMinMax(xMin, xMax))
                        {
                            string tableName = comboBox_tables.SelectedItem.ToString();
                            string yAxis = comboBox_yColumn.SelectedItem.ToString();
                            string lineName = tableName + " (" + databaseName + "): " + yAxis;

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

                            MakeQuery(xMin, xMax);
                            (double[] x, double[] y) = ConvertListToArray(lineName);
                            if (x.Length != 0 && y.Length != 0)
                            {
                                AddLine(x, y, lineName);
                            }
                            else
                            {
                                string message = "No data found.";
                                string title = "Plot data";
                                MessageBox.Show(message, title);
                            }
                        }
                    }
                    else
                    {
                        string message = "Please select column.";
                        string title = "Y-Axis";
                        MessageBox.Show(message, title);
                    }
                }
                else
                {
                    string message = "Please select column.";
                    string title = "X-Axis";
                    MessageBox.Show(message, title);
                }
            }
            else
            {
                string message = "Please select table.";
                string title = "Missing table";
                MessageBox.Show(message, title);
            }
        }

        private (double[], double[]) ConvertListToArray(string lineName)
        {
            Line currentLine = null;
            foreach(Line line in PlottedLines)
            {
                if (line.Name == lineName)
                {
                    currentLine = line;
                }
            }
            return GetXY(currentLine);
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

        private (string, string) Assign_xMinMax()
        {
            string xMin = null;
            string xMax = null;
            if ((bool) checkBox_xMin.IsChecked)
            {
                xMin = textBox_xMin.Text;
            }
            if ((bool) checkBox_xMax.IsChecked)
            {
                xMax = textBox_xMax.Text;
            }
            return (xMin, xMax);
        }

        private void MakeQuery (string xMin, string xMax)
        {
            string tableName = comboBox_tables.SelectedItem.ToString();
            string xColumn = comboBox_xColumn.SelectedItem.ToString();
            string yColumn = comboBox_yColumn.SelectedItem.ToString();
            string name = tableName + " (" + databaseName + "): " + yColumn;

            Line line = new();
            line.Name = name;

            if ((bool) radioButton_primary.IsChecked)
            {
                line.YAxis = "primary";
            }
            else if ((bool) radioButton_secondary.IsChecked)
            {
                line.YAxis = "secondary";
            }

            Axis yAxis = yAxes.Find(axis => axis.Name == line.YAxis);
            yAxis.Lines.Add(line);

            if ((bool) radioButton_linear.IsChecked)
            {
                yAxis.Scale = "linear";
            }
            else if ((bool) radioButton_logarithmic.IsChecked)
            {
                yAxis.Scale = "logarithmic";
            }

            line.Points = database.GetData(tableName, xColumn, yColumn, xMin, xMax);
            PlottedLines.Add(line);
        }

        private static bool Valid_xMinMax (string xMin, string xMax)
        {
            bool xMin_ok;
            bool xMax_ok;
            double xMin_value = 0;
            double xMax_value = 0;
            if (xMin == null)
            {
                xMin_ok = true;
            }
            else
            {
                if (double.TryParse(xMin, out xMin_value))
                {
                    xMin_ok = true;
                }
                else
                {
                    xMin_ok = false;
                    string message = "The min value must be a number.";
                    string title = "Filter x-axis";
                    MessageBox.Show(message, title);
                }
            }

            if (xMax == null)
            {
                xMax_ok = true;
            }
            else
            {
                if (double.TryParse(xMax, out xMax_value))
                {
                    xMax_ok = true;
                }
                else
                {
                    xMax_ok = false;
                    string message = "The max value must be a number.";
                    string title = "Filter x-axis";
                    MessageBox.Show(message, title);
                }
            }
            
            if (xMin_ok && xMax_ok)
            {
                if (xMax != null && xMin != null)
                {
                    if (xMax_value > xMin_value)
                    {
                        return true;
                    }
                    else
                    {
                        string message = "The max value must be greater than the min value.";
                        string title = "X-Axis";
                        MessageBox.Show(message, title);
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        private void AddLine (double[] x, double[] y, string lineName)
        {
            Line line = null;
            string yAxis = null;
            string yScale = null;
            foreach(Line currentLine in PlottedLines)
            {
                if (currentLine.Name == lineName)
                {
                    line = currentLine;
                    yAxis = currentLine.YAxis;
                    yScale = yAxes.Find(axis => axis.Name == yAxis).Scale;
                }
            }

            Color color = WindowsMediaColor(plot.Plot.GetNextColor());
            ScottPlot.Drawing.Palette palette = ScottPlot.Drawing.Palette.Category20;
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
                plottable = plot.Plot.AddScatter(x, y, markerSize: 0, label: lineName, color: DrawingColor(color));
                plot.Plot.YAxis.MinorLogScale(true);
            }
            else if (yScale == "logarithmic")
            {
                plottable = plot.Plot.AddScatter(x, ScottPlot.Tools.Log10(y), markerSize: 0, label: lineName, color: DrawingColor(color));
                plot.Plot.YAxis.MinorLogScale(false);
            }
            line.Plottable = plottable;
            line.Color = color;

            if (yAxis == "Primary")
            {
                plottable.YAxisIndex = 1;
                plot.Plot.YAxis.Color(plottable.Color);
            }
            else if (yAxis == "Secondary")
            {
                plottable.YAxisIndex = 1;
                plot.Plot.YAxis2.Ticks(true);
                plot.Plot.YAxis2.Color(plottable.Color);
            }

            EnableClearButton = true;
        }

        private static Color WindowsMediaColor (System.Drawing.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        private static System.Drawing.Color DrawingColor (Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        private void ButtonClick_Disconnect (object sender, RoutedEventArgs e)
        {
            database.Disconnect();

            EnableConnectButton = true;
            EnableDisconnectButton = false;
            EnableAddLineButton = false;

            comboBox_tables.Text = "Select table...";
            comboBox_xColumn.Text = "Select column...";
            comboBox_yColumn.Text = "Select column...";
        }

        private void ButtonClick_Clear (object sender, RoutedEventArgs e)
        {
            plot.Plot.Clear();
            EnableClearButton = false;
            PlottedLines.Clear();

            foreach(Axis yAxis in yAxes)
            {
                yAxis.Lines.Clear();
            }

            plot.Plot.YAxis.Color(System.Drawing.Color.Black);
            plot.Plot.YAxis2.Color(System.Drawing.Color.Black);
        }

        private void TableSelected (object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && comboBox_tables.SelectedItem != null)
            {
                database.AddColumns(comboBox_tables.SelectedItem.ToString());
            }
        }

        private void Window_Closing (object sender, CancelEventArgs e)
        {
            string message = "Close window?";
            string title = "Confirm close";
            MessageBoxResult result = MessageBox.Show(message, title, MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        private void TextBoxFocus_xMin (object sender, RoutedEventArgs e)
        {
            if (textBox_xMin.Text == "Enter min...")
            {
                textBox_xMin.Text = "";
                textBox_xMin.Foreground = Brushes.Black;
            }
        }

        private void TextBoxFocus_xMax (object sender, RoutedEventArgs e)
        {
            if (textBox_xMax.Text == "Enter max...")
            {
                textBox_xMax.Text = "";
                textBox_xMax.Foreground = Brushes.Black;
            }
        }

        private void TextBoxNotFocus_xMin (object sender, RoutedEventArgs e)
        {
            if (textBox_xMin.Text == "")
            {
                textBox_xMin.Text = "Enter min...";
                textBox_xMin.Foreground = Brushes.Gray;
            }
        }

        private void TextBoxNotFocus_xMax (object sender, RoutedEventArgs e)
        {
            if (textBox_xMax.Text == "")
            {
                textBox_xMax.Text = "Enter max...";
                textBox_xMax.Foreground = Brushes.Gray;
            }
        }

        private void ButtonClick_deleteLine (object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Line line = button.DataContext as Line;
            string name = line.Name;

            foreach (Line currentLine in PlottedLines)
            {
                if (currentLine.Name == name)
                {
                    plot.Plot.Remove(line.Plottable);
                    plot.Render();

                    PlottedLines.Remove(currentLine);
                    colorsUsed.Remove(currentLine.Color);
                    Axis yAxis = yAxes.Find(axis => axis.Name == currentLine.YAxis);
                    yAxis.Lines.Remove(currentLine);
                    break;
                }
            }

            if (PlottedLines.Count == 0)
            {
                plot.Plot.Clear();
                plot.Plot.YAxis.Color(System.Drawing.Color.Black);
                plot.Plot.YAxis2.Color(System.Drawing.Color.Black);
                EnableClearButton = false;
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
                if ((bool) radioButton_primary.IsChecked)
                {
                    axisName = "primary";
                }
                else if ((bool) radioButton_secondary.IsChecked)
                {
                    axisName = "secondary";
                }
                Axis yAxis = yAxes.Find(axis => axis.Name == axisName);
                if (yAxis.Scale != scale)
                {
                    yAxis.Scale = scale;
                    foreach(Line line in yAxis.Lines)
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

        private void MouseMove_plot (object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (PlottedLines.Count != 0)
            {
                if ((bool) showCoordinates_on.IsChecked)
                {
                    (double mouseX, double mouseY) = plot.GetMouseCoordinates();
                    double minDistance = double.MaxValue;
                    double minX = 0;
                    double minY = 0;
                    foreach (Line line in PlottedLines)
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
                    hoveredPoint.Xs[0] = minX;
                    hoveredPoint.Ys[0] = minY;
                    hoveredPoint.IsVisible = true;
                    plot.Render();

                    hoveredPoint_xyText.Text = $"(x, y): ({ minX:N2}, { minY:N2})";
                }
            }
        }

        private void DisableCoordinates (object sender, RoutedEventArgs e)
        {
            if (hoveredPoint != null)
            {
                hoveredPoint_xyText.Text = "";
                hoveredPoint.IsVisible = false;
                plot.Render();
            }
        }
    }
}
