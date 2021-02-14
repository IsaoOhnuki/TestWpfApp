using ObjectAreaLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            contentsArea.AddContents(new MockAreaItem()
            {
                Background = new SolidColorBrush(Colors.Red),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Left = 0,
                Top = 0,
                Width = 200,
                Height = 150,
            });
            contentsArea.AddContents(new MockAreaItem()
            {
                Background = new SolidColorBrush(Colors.Blue),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                Left = 100,
                Top = 0,
                Width = 200,
                Height = 150,
            });
            contentsArea.AddContents(new MockAreaItem()
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
