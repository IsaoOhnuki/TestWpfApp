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

            double diff = shortPathLine.StrokeThickness;
            Left = bounds.Left - diff;
            Top = bounds.Top - diff;
            Width = bounds.Width + diff + diff;
            Height = bounds.Height + diff + diff;

            var linePos = AStar.Instance.Exec(startPos, endPos, minPos, maxPos, new List<Rect>(), AStar.Viewpoint, AStar.Heuristic);

            var lineData = new PathGeometry();
            if (linePos.Count() > 0)
            {
                PathFigure figure = new PathFigure();
                var firstPos = linePos.FirstOrDefault();
                if (firstPos != null)
                {
                    figure.StartPoint = firstPos + new Vector(diff, diff);
                    foreach (var pos in linePos)
                    {
                        figure.Segments.Add(new LineSegment() { Point = pos + new Vector(diff, diff) });
                    }
                    figure.Segments.RemoveAt(0);
                }
                lineData.Figures.Add(figure);
            }
            ShortPathSetter = lineData;
        }
    }
}
