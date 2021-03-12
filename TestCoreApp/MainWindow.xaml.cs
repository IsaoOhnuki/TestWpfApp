using ContentsCanvas;
using ObjectAreaLibrary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using log4net;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Globalization;

namespace TestCoreApp
{
    public class BooleanToEnumerateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ParameterString = parameter as string;
            if (ParameterString == null)
            {
                return DependencyProperty.UnsetValue;
            }
            if (Enum.IsDefined(value.GetType(), value) == false)
            {
                return DependencyProperty.UnsetValue;
            }
            object paramvalue = Enum.Parse(value.GetType(), ParameterString);
            return (int)paramvalue == (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ParameterString = parameter as string;
            // ラジオボタン用のコンバータ。false→trueの変化のみ反応する。
            return ParameterString == null || !(bool)value ? DependencyProperty.UnsetValue : Enum.Parse(targetType, ParameterString);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));
        private static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        List<MockAreaContents> _obstacles = new List<MockAreaContents>();
        MockAreaContents _item1;
        MockAreaContents _item2;

        public MainWindow()
        {
            InitializeComponent();

            logger.Info("AStar");

            _item1 = new MockAreaContents()
            {
                Background = new SolidColorBrush(Colors.Red),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Left = 100,
                Top = 100,
                Width = 200,
                Height = 150,
            };
            _item2 = new MockAreaContents()
            {
                Background = new SolidColorBrush(Colors.Blue),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Left = 500,
                Top = 400,
                Width = 200,
                Height = 150,
            };
            contentsArea.Add(_item1);
            contentsArea.Add(_item2);

            var item31 = new MockAreaContents()
            {
                Background = new SolidColorBrush(Colors.Green),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Left = 320,
                Top = 80,
                Width = 200,
                Height = 150,
            };
            var item32 = new MockAreaContents()
            {
                Background = new SolidColorBrush(Colors.Green),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Left = 80,
                Top = 270,
                Width = 200,
                Height = 150,
            };
            var item33 = new MockAreaContents()
            {
                Background = new SolidColorBrush(Colors.Green),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Left = 320,
                Top = 270,
                Width = 200,
                Height = 150,
            };
            var item34 = new MockAreaContents()
            {
                Background = new SolidColorBrush(Colors.Green),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Left = 480,
                Top = 380,
                Width = 200,
                Height = 150,
            };
            var item35 = new MockAreaContents()
            {
                Background = new SolidColorBrush(Colors.Green),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Left = 480,
                Top = 380,
                Width = 200,
                Height = 150,
            };
            var item36 = new MockAreaContents()
            {
                Background = new SolidColorBrush(Colors.Green),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Left = 480,
                Top = 380,
                Width = 200,
                Height = 150,
            };
            var item37 = new MockAreaContents()
            {
                Background = new SolidColorBrush(Colors.Green),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Left = 480,
                Top = 380,
                Width = 200,
                Height = 150,
            };
            var item38 = new MockAreaContents()
            {
                Background = new SolidColorBrush(Colors.Green),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Left = 480,
                Top = 380,
                Width = 200,
                Height = 150,
            };
            contentsArea.Add(item31);
            contentsArea.Add(item32);
            contentsArea.Add(item33);
            contentsArea.Add(item34);
            contentsArea.Add(item35);
            contentsArea.Add(item36);
            contentsArea.Add(item37);
            contentsArea.Add(item38);

            _obstacles.Add(item31);
            _obstacles.Add(item32);
            _obstacles.Add(item33);
            _obstacles.Add(item34);
            _obstacles.Add(item35);
            _obstacles.Add(item36);
            _obstacles.Add(item37);
            _obstacles.Add(item38);

            _shortPathLine = new ShortPathLine();
            contentsArea.ContentsCanvas.Children.Add(_shortPathLine);
            _shortPathLine.SetLine(new Point(_item1.Left, _item1.Top), new Point(_item2.Left, _item2.Top), new Rect(new Point(0, 0), new Point(0, 0)), _obstacles.Select(_ => _.Bounds));

            _item1.LocationChangedEvent += Item_LocationChanged;
            _item1.SizeChanged += Item_SizeChanged;
            _item2.LocationChangedEvent += Item_LocationChanged;
            _item2.SizeChanged += Item_SizeChanged;

            InertiaValue = 10;
        }

        ShortPathLine _shortPathLine;

        private void Item_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is MockAreaContents mock)
            {
                if (mock.Name == "item1")
                {

                }
                else if (mock.Name == "item3")
                {

                }
            }
        }

        private async void Item_LocationChanged(IAreaContents areaitem, Point value)
        {
            if (areaitem is MockAreaContents mock)
            {
                await _shortPathLine.SetLineAsync(new Point(_item1.Left, _item1.Top), new Point(_item2.Left, _item2.Top), new Rect(), _obstacles.Select(_ => _.Bounds).ToArray());
            }
        }

        //private void Item_LocationChanged(IAreaContents areaitem, Point value)
        //{
        //    if (areaitem is MockAreaContents mock)
        //    {
        //        _shortPathLine.SetLine(new Point(_item1.Left, _item1.Top), new Point(_item2.Left, _item2.Top), new Rect(), _obstacles.Select(_ => _.Bounds).ToArray());
        //    }
        //}

        public static readonly DependencyProperty InertiaProperty = DependencyProperty.Register(
            nameof(Inertia),
            typeof(bool),
            typeof(MainWindow),
            new FrameworkPropertyMetadata(default(bool), (s, e) => {
                if (s is MainWindow mw)
                {
                    mw._shortPathLine.Inertia = (bool)e.NewValue;
                }
            }));

        public bool Inertia { get => (bool)GetValue(InertiaProperty); set => SetValue(InertiaProperty, value); }

        public static readonly DependencyProperty InertiaValueProperty = DependencyProperty.Register(
            nameof(InertiaValue),
            typeof(double),
            typeof(MainWindow),
            new FrameworkPropertyMetadata(default(double), (s, e) => {
                if (s is MainWindow mw)
                {
                    mw._shortPathLine.InertiaValue = (double)e.NewValue;
                }
            }));

        public double InertiaValue { get => (double)GetValue(InertiaValueProperty); set => SetValue(InertiaValueProperty, value); }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //SaveFileDialog sfd = new SaveFileDialog();
            //var filename = DateTime.Now.ToString("yyyyMMddHHmmss");
            //if (CSVType != AStarNode.ValueType.None)
            //{
            //    filename += "_" + CSVType.ToString();
            //}
            //sfd.FileName = filename + ".csv";
            //var ret = sfd.ShowDialog();
            //if (ret.HasValue && ret.Value)
            //{
            //    var csv = _shortPathLine.CSV;
            //    var fs = new StreamWriter(sfd.FileName, false, Encoding.UTF8);
            //    fs.WriteLine(csv);
            //    fs.Close();
            //}
            //MessageBox.Show("CSVファイルが出力されました。");
        }

        public static readonly DependencyProperty CSVTypeProperty = DependencyProperty.Register(
            nameof(CSVType),
            typeof(AStarNode.ValueType),
            typeof(MainWindow),
            new FrameworkPropertyMetadata(AStarNode.ValueType.None, (d, e) => {
                if (d is MainWindow mw)
                {
                    mw._shortPathLine.CsvType = (AStarNode.ValueType)e.NewValue;
                }
            }));

        public AStarNode.ValueType CSVType { get => (AStarNode.ValueType)GetValue(CSVTypeProperty); set => SetValue(CSVTypeProperty, value); }
    }
}
