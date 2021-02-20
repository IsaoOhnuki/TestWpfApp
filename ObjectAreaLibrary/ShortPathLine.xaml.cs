using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ObjectAreaLibrary
{
    /// <summary>
    /// UserControl1.xaml の相互作用ロジック
    /// </summary>
    public partial class ShortPathLine : UserControl
    {
        public ShortPathLine()
        {
            InitializeComponent();
        }

        public void SetLine(IEnumerable<AStarNode> nodes)
        {

        }

        public static readonly DependencyProperty ShortPathProperty = DependencyProperty.Register(
            nameof(ShortPath),
            typeof(Geometry),
            typeof(ShortPathLine),
            new FrameworkPropertyMetadata(default(Geometry)));

        public Geometry ShortPath
        {
            get { return (Geometry)GetValue(ShortPathProperty); }
            set { SetValue(ShortPathProperty, value); }
        }
    }
}
