using ContentsCanvas;
using ObjectAreaLibrary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using log4net;

namespace TestCoreApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));
        private static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        MockAreaContents _item1;
        MockAreaContents _item2;
        MockAreaContents _item3;

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
                Left = 200,
                Top = 200,
                Width = 200,
                Height = 150,
            };
            _item3 = new MockAreaContents()
            {
                Background = new SolidColorBrush(Colors.Green),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Left = 300,
                Top = 300,
                Width = 200,
                Height = 150,
            };
            contentsArea.Add(_item1);
            contentsArea.Add(_item2);
            contentsArea.Add(_item3);

            _shortPathLine = new ShortPathLine();
            contentsArea.ContentsCanvas.Children.Add(_shortPathLine);
            _shortPathLine.SetLine(new Point(_item1.Left, _item1.Top), new Point(_item3.Left, _item3.Top), new Point(0, 0), new Point(1000, 1000));

            _item1.LocationChangedEvent += Item_LocationChanged;
            _item1.SizeChanged += Item_SizeChanged;
            _item3.LocationChangedEvent += Item_LocationChanged;
            _item3.SizeChanged += Item_SizeChanged;
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
                _shortPathLine.SetLine(new Point(_item1.Left, _item1.Top), new Point(_item3.Left, _item3.Top), new Point(0, 0), new Point(1000, 1000));
            }
        }
    }
}
