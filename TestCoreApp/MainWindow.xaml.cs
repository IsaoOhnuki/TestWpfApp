using ContentsCanvas;
using System.Windows;
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

            contentsArea.Add(new MockAreaContents()
            {
                Background = new SolidColorBrush(Colors.Red),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Left = 0,
                Top = 0,
                Width = 200,
                Height = 150,
                Group = "a",
            });
            contentsArea.Add(new MockAreaContents()
            {
                Background = new SolidColorBrush(Colors.Blue),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Left = 100,
                Top = 0,
                Width = 200,
                Height = 150,
                Group = "a",
            });
            contentsArea.Add(new MockAreaContents()
            {
                Background = new SolidColorBrush(Colors.Green),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Left = 200,
                Top = 200,
                Width = 200,
                Height = 150,
            });
        }
    }
}
