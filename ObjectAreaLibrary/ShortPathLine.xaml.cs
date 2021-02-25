using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
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

        public void SetLine(Point startPos, Point endPos, Rect limitRect, IEnumerable<Rect> obstacles)
        {
            double diff = shortPathLine.StrokeThickness;
            double step = 10;
            var bounds = new Rect(startPos, endPos);
            var lineData = new PathGeometry();
            var result = AStar.Instance.Exec(startPos, endPos, limitRect, step, obstacles, AStar.Viewpoint, AStar.Heuristic);
            if (result)
            {
                var linePos = AStar.Instance.AdoptList();
                if (linePos.Count() > 0)
                {
                    bounds = new Rect(new Point(linePos.Min(_ => _.X), linePos.Min(_ => _.Y)), new Point(linePos.Max(_ => _.X), linePos.Max(_ => _.Y)));

                    PathFigure figure = new PathFigure();
                    var firstPos = linePos.FirstOrDefault();
                    if (firstPos != null)
                    {
                        var diffPos = new Vector(bounds.Left - diff, bounds.Top - diff);
                        figure.StartPoint = firstPos - diffPos;
                        foreach (var pos in linePos)
                        {
                            figure.Segments.Add(new LineSegment() { Point = pos - diffPos });
                        }
                        figure.Segments.RemoveAt(0);
                    }

                    lineData.Figures.Add(figure);
                }
            }
            ShortPathSetter = lineData;

            Left = bounds.Left - diff;
            Top = bounds.Top - diff;
            Width = bounds.Width + diff + diff;
            Height = bounds.Height + diff + diff;
        }
    }
}
