using System;
using System.Windows;
using System.Windows.Controls;

namespace ObjectAreaLibrary
{
    public enum HandleType
    {
        None,
        Fill,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }

    /// <summary>
    /// ContentsOperator.xaml の相互作用ロジック
    /// </summary>
    public partial class ContentsOperator : UserControl
    {
        private readonly double HandleSize;
        private readonly double ContentRectangleDiff;
        private readonly Thickness TopLeftHandleMargin;
        private readonly Thickness TopRightHandleMargin;
        private readonly Thickness BottomLeftHandleMargin;
        private readonly Thickness BottomRightHandleMargin;

        public ContentsOperator()
        {
            InitializeComponent();

            HandleSize = (double)Resources["handleSize"];
            ContentRectangleDiff = (double)Resources["contentRectangleDiff"];
            TopLeftHandleMargin = ((Thickness)Resources["topLeftHandleMargin"]);
            TopRightHandleMargin = ((Thickness)Resources["topRightHandleMargin"]);
            BottomLeftHandleMargin = ((Thickness)Resources["bottomLeftHandleMargin"]);
            BottomRightHandleMargin = ((Thickness)Resources["bottomRightHandleMargin"]);
        }

        private IAreaItem _contents;

        public IAreaItem Contents
        {
            get { return _contents; }
            set
            {
                _contents = value;
                if (_contents != null)
                {
                    Left = _contents.Left;
                    Top = _contents.Top;
                    Width = _contents.Width;
                    Height = _contents.Height;
                }
            }
        }

        #region EditProperty
        public static readonly DependencyProperty EditProperty = DependencyProperty.Register(
            nameof(Edit),
            typeof(bool),
            typeof(ContentsOperator),
            new FrameworkPropertyMetadata(false));

        public bool Edit { get => (bool)GetValue(EditProperty); set => SetValue(EditProperty, value); }
        #endregion

        #region LeftProperty
        public double Left
        {
            get { return Canvas.GetLeft(this); }
            set
            {
                Canvas.SetLeft(this, value);
                _contents.Left = value;
            }
        }
        #endregion

        #region TopProperty
        public double Top
        {
            get { return Canvas.GetTop(this); }
            set
            {
                Canvas.SetTop(this, value);
                _contents.Top = value;
            }
        }
        #endregion

        #region SizeChanged
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (sizeInfo.WidthChanged)
            {
                _contents.Width = sizeInfo.NewSize.Width;
            }
            if (sizeInfo.HeightChanged)
            {
                _contents.Height = sizeInfo.NewSize.Height;
            }
        }
        #endregion

        #region ResizeFunction
        private Point _topLeft;
        private Point _bottomRight;
        private Vector _offset;

        public void Resizing(HandleType handleType, Point location)
        {
            _topLeft = new Point(Left, Top);
            _bottomRight = new Point(Left + Width, Top + Height);
            switch (handleType)
            {
                case HandleType.TopLeft:
                    _offset = location - _topLeft;
                    break;
                case HandleType.TopRight:
                    _offset = new Vector(location.X - _topLeft.X - Width, location.Y - _topLeft.Y);
                    break;
                case HandleType.BottomLeft:
                    _offset = new Vector(location.X - _topLeft.X, location.Y - _topLeft.Y - Height);
                    break;
                case HandleType.BottomRight:
                    _offset = new Vector(location.X - _topLeft.X - Width, location.Y - _topLeft.Y - Height);
                    break;
                case HandleType.Fill:
                    _offset = location - _topLeft;
                    break;
            }
        }

        public void Resize(HandleType handleType, Point location)
        {
            switch (handleType)
            {
                case HandleType.TopLeft:
                    ResizeTopLeft(location - _offset);
                    break;
                case HandleType.TopRight:
                    ResizeTopRight(location - _offset);
                    break;
                case HandleType.BottomLeft:
                    ResizeBottomLeft(location - _offset);
                    break;
                case HandleType.BottomRight:
                    ResizeBottomRight(location - _offset);
                    break;
                case HandleType.Fill:
                    MoveFill(location - _topLeft - _offset);
                    _topLeft = new Point(Left, Top);
                    break;
            }
        }

        private void ResizeTopLeft(Point location)
        {
            Left = _bottomRight.X > location.X ? location.X : _bottomRight.X;
            Top = _bottomRight.Y > location.Y ? location.Y : _bottomRight.Y;
            Width = _bottomRight.X > location.X ? _bottomRight.X - location.X : location.X - _bottomRight.X;
            Height = _bottomRight.Y > location.Y ? _bottomRight.Y - location.Y : location.Y - _bottomRight.Y;
        }

        private void ResizeBottomLeft(Point location)
        {
            Left = _bottomRight.X > location.X ? location.X : _bottomRight.X;
            Top = _topLeft.Y > location.Y ? location.Y : _topLeft.Y;
            Width = _bottomRight.X > location.X ? _bottomRight.X - location.X : location.X - _bottomRight.X;
            Height = _topLeft.Y > location.Y ? _topLeft.Y - location.Y : location.Y - _topLeft.Y;
        }

        private void ResizeTopRight(Point location)
        {
            Left = _topLeft.X > location.X ? location.X : _topLeft.X;
            Top = _bottomRight.Y > location.Y ? location.Y : _bottomRight.Y;
            Width = _topLeft.X > location.X ? _topLeft.X - location.X : location.X - _topLeft.X;
            Height = _bottomRight.Y > location.Y ? _bottomRight.Y - location.Y : location.Y - _bottomRight.Y;
        }

        private void ResizeBottomRight(Point location)
        {
            Left = _topLeft.X > location.X ? location.X : _topLeft.X;
            Top = _topLeft.Y > location.Y ? location.Y : _topLeft.Y;
            Width = _topLeft.X > location.X ? _topLeft.X - location.X : location.X - _topLeft.X;
            Height = _topLeft.Y > location.Y ? _topLeft.Y - location.Y : location.Y - _topLeft.Y;
        }

        private void MoveFill(Vector delta)
        {
            Left += delta.X;
            Top += delta.Y;
        }
        #endregion

        #region HitHandleFunction
        public HandleType HitHandle(Point parentLocation)
        {
            if (Edit)
            {
                if (HitTopLeft(parentLocation))
                {
                    return HandleType.TopLeft;
                }
                else if (HitTopRight(parentLocation))
                {
                    return HandleType.TopRight;
                }
                else if (HitBottomLeft(parentLocation))
                {
                    return HandleType.BottomLeft;
                }
                else if (HitBottomRight(parentLocation))
                {
                    return HandleType.BottomRight;
                }
                else if (HitFill(parentLocation))
                {
                    return HandleType.Fill;
                }
            }
            else if (HitFill(parentLocation))
            {
                return HandleType.Fill;
            }
            return HandleType.None;
        }

        public bool HitFill(Point parentLocation)
        {
            Rect handleRect = new Rect()
            {
                X = Left,
                Y = Top,
                Width = Width,
                Height = Height,
            };
            return handleRect.Contains(parentLocation);
        }

        private bool HitTopLeft(Point parentLocation)
        {
            Rect handleRect = new Rect()
            {
                X = Left - TopLeftHandleMargin.Left - ContentRectangleDiff,
                Y = Top - TopLeftHandleMargin.Top - ContentRectangleDiff,
                Width = HandleSize,
                Height = HandleSize,
            };
            return handleRect.Contains(parentLocation);
        }

        private bool HitTopRight(Point parentLocation)
        {
            Rect handleRect = new Rect()
            {
                X = Left + ActualWidth - TopRightHandleMargin.Left - ContentRectangleDiff,
                Y = Top - TopRightHandleMargin.Top - ContentRectangleDiff,
                Width = HandleSize,
                Height = HandleSize,
            };
            return handleRect.Contains(parentLocation);
        }

        private bool HitBottomLeft(Point parentLocation)
        {
            Rect handleRect = new Rect()
            {
                X = Left - BottomLeftHandleMargin.Left - ContentRectangleDiff,
                Y = Top + ActualHeight - BottomLeftHandleMargin.Top - ContentRectangleDiff,
                Width = HandleSize,
                Height = HandleSize,
            };
            return handleRect.Contains(parentLocation);
        }

        private bool HitBottomRight(Point parentLocation)
        {
            Rect handleRect = new Rect()
            {
                X = Left + ActualWidth - BottomRightHandleMargin.Left - ContentRectangleDiff,
                Y = Top + ActualHeight - BottomRightHandleMargin.Top - ContentRectangleDiff,
                Width = HandleSize,
                Height = HandleSize,
            };
            return handleRect.Contains(parentLocation);
        }
        #endregion
    }
}
