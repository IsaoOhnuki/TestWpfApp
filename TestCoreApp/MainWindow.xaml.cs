using ContentsCanvas;
using ObjectAreaLibrary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace TestCoreApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var item1 = new MockAreaContents()
            {
                Name = "item1",
                Background = new SolidColorBrush(Colors.Red),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Left = 500,
                Top = 500,
                Width = 200,
                Height = 150,
            };
            var item2 = new MockAreaContents()
            {
                Name = "item2",
                Background = new SolidColorBrush(Colors.Blue),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Left = 100,
                Top = 0,
                Width = 200,
                Height = 150,
            };
            var item3 = new MockAreaContents()
            {
                Name = "item3",
                Background = new SolidColorBrush(Colors.Green),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Left = 200,
                Top = 200,
                Width = 200,
                Height = 150,
            };
            contentsArea.Add(item1);
            contentsArea.Add(item2);
            contentsArea.Add(item3);

            _shortPathLine = new ShortPathLine();
            contentsArea.ContentsCanvas.Children.Add(_shortPathLine);
            _shortPathLine.SetLine(new Point(item1.Left, item1.Top), new Point(item3.Left, item3.Top), new Point(-100, -100), new Point(1000, 1000));

            item1.LocationChangedEvent += Item_LocationChanged;
            item1.SizeChanged += Item_SizeChanged;
            item3.LocationChangedEvent += Item_LocationChanged;
            item3.SizeChanged += Item_SizeChanged;
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
                if (mock.Name == "item1")
                {

                }
                else if (mock.Name == "item3")
                {

                }
            }
        }
    }
}
