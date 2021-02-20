using System;
using System.Collections.Generic;
using System.Linq;
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
    using NodePoint = System.Drawing.Point;

    /// <summary>
    /// UserControl1.xaml の相互作用ロジック
    /// </summary>
    public partial class ShortPathLine : UserControl
    {
        public ShortPathLine()
        {
            InitializeComponent();

            Binding shortPathBinding = new Binding(nameof(ShortPathSetter))
            {
                Source = this,
                // ShortPathSetterProperty > DataProperty
                Mode = BindingMode.OneWay,
            };
            BindingOperations.SetBinding(shortPathLine, Path.DataProperty, shortPathBinding);
        }

        public static readonly DependencyProperty ShortPathSetterProperty = DependencyProperty.Register(
            nameof(ShortPathSetter),
            typeof(Geometry),
            typeof(ShortPathLine),
            new FrameworkPropertyMetadata(default(Geometry)));

        public Geometry ShortPathSetter
        {
            get { return (Geometry)GetValue(ShortPathSetterProperty); }
            set { SetValue(ShortPathSetterProperty, value); }
        }

        public double Left
        {
            get { return (double)GetValue(Canvas.LeftProperty); }
            set { SetValue(Canvas.LeftProperty, value); }
        }

        public double Top
        {
            get { return (double)GetValue(Canvas.TopProperty); }
            set { SetValue(Canvas.TopProperty, value); }
        }

        public void SetLine(Point startPos, Point endPos, Point minPos, Point maxPos)
        {
            var bounds = new Rect(startPos, endPos);

            Left = bounds.Left;
            Top = bounds.Top;
            Width = bounds.Width;
            Height = bounds.Height;

            AStar astar = new AStar();
            var linePos = astar.Exec(startPos, endPos, minPos, maxPos, new List<Rect>(), AStar.Viewpoint, AStar.Heuristic);

            PathFigure figure = new PathFigure();
            var firstPos = linePos.FirstOrDefault();
            if (firstPos != null)
            {
                figure.StartPoint = firstPos;
                foreach (var pos in linePos)
                {
                    figure.Segments.Add(new LineSegment() { Point = pos });
                }
                figure.Segments.RemoveAt(0);
            }

            var lineData = new PathGeometry();
            lineData.Figures.Add(figure);
            ShortPathSetter = lineData;
        }
    }
}
