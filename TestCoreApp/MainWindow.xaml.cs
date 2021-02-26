using ContentsCanvas;
using ObjectAreaLibrary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using log4net;
using System.Collections.Generic;
using System.Linq;

namespace TestCoreApp
{
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
            contentsArea.Add(item31);
            contentsArea.Add(item32);
            contentsArea.Add(item33);
            contentsArea.Add(item34);

            _obstacles.Add(item31);
            _obstacles.Add(item32);
            _obstacles.Add(item33);
            _obstacles.Add(item34);

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

        private void Item_LocationChanged(IAreaContents areaitem, Point value)
        {
            if (areaitem is MockAreaContents mock)
            {
                _shortPathLine.SetLine(new Point(_item1.Left, _item1.Top), new Point(_item2.Left, _item2.Top), new Rect(new Point(0, 0), new Point(0, 0)), _obstacles.Select(_ => _.Bounds));
            }
        }

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
    }
}
