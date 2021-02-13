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
    /// ContentsAreaItem.xaml の相互作用ロジック
    /// </summary>
    public partial class ContentsContainer : UserControl
    {
        public ContentsContainer()
        {
            InitializeComponent();

            SetCanvasMarginFromParentThickness(BorderThickness);
            Left = 0;
            Top = 0;
            ZIndex = 0;

            BorderThicknessProperty.OverrideMetadata(
                typeof(ContentsContainer),
                new FrameworkPropertyMetadata(default(Thickness), (d, e) => {
                    if (d is ContentsContainer areaItem)
                    {
                        SetCanvasMarginFromParentThickness((Thickness)e.NewValue);
                    }
                }));
        }

        #region CanvasMarginProperty
        /// <summary>
        /// UserControlのBorderThicknessが変化するとDockSizeが変化するためそれを補正する
        /// </summary>
        public static readonly DependencyProperty CanvasMarginProperty = DependencyProperty.RegisterAttached(
            nameof(CanvasMargin),
            typeof(Thickness),
            typeof(ContentsContainer),
            new FrameworkPropertyMetadata(default(Thickness)));

        public Thickness CanvasMargin
        {   get { return (Thickness)GetValue(CanvasMarginProperty); }
            set { SetValue(CanvasMarginProperty, value); }
        }

        private void SetCanvasMarginFromParentThickness(Thickness thickness)
        {
            double diff = (double)Resources["contentRectangleDiff"];
            CanvasMargin = new Thickness()
            {
                Left = -(thickness.Left + diff),
                Top = -(thickness.Top + diff),
                Right = -(thickness.Right + diff),
                Bottom = -(thickness.Bottom + diff),
            };
        }
        #endregion

        #region GroupProperty
        public static readonly DependencyProperty GroupProperty = DependencyProperty.RegisterAttached(
            nameof(Group),
            typeof(string),
            typeof(ContentsContainer),
            new FrameworkPropertyMetadata(default(string), (d, e) => {
                if (d is ContentsContainer areaItem)
                {
                    areaItem.OnGroupChanged(areaItem, (string)e.NewValue);
                }
            }));

        public string Group
        {
            get { return (string)GetValue(GroupProperty); }
            set { SetValue(GroupProperty, value); }
        }

        public event Action<ContentsContainer, string> GroupChangedEvent;
        public void OnGroupChanged(ContentsContainer areaItem, string value)
        {
            GroupChangedEvent?.Invoke(areaItem, value);
        }
        #endregion

        #region SelectProperty
        public static readonly DependencyProperty SelectProperty = DependencyProperty.RegisterAttached(
            nameof(Select),
            typeof(bool),
            typeof(ContentsContainer),
            new FrameworkPropertyMetadata(false, (d, e) => {
                if (d is ContentsContainer areaItem)
                {
                    areaItem.OnSelectChanged(areaItem, (bool)e.NewValue);
                }
            }));

        public bool Select
        {
            get { return (bool)GetValue(SelectProperty); }
            set { SetValue(SelectProperty, value); }
        }

        public event Action<ContentsContainer, bool> SelectChangedEvent;
        public void OnSelectChanged(ContentsContainer areaItem, bool value)
        {
            SelectChangedEvent?.Invoke(areaItem, value);
        }
        #endregion

        #region EditProperty
        public static readonly DependencyProperty EditProperty = DependencyProperty.RegisterAttached(
            nameof(Edit),
            typeof(bool),
            typeof(ContentsContainer),
            new FrameworkPropertyMetadata(false, (d, e) => {
                if (d is ContentsContainer areaItem)
                {
                    areaItem.OnEditChanged(areaItem, (bool)e.NewValue);
                }
            }));
        public bool Edit
        {
            get { return (bool)GetValue(EditProperty); }
            set { SetValue(EditProperty, value); }
        }

        public event Action<ContentsContainer, bool> OnEditChangedEvent;
        public void OnEditChanged(ContentsContainer areaItem, bool value)
        {
            OnEditChangedEvent?.Invoke(areaItem, value);
        }
        #endregion

        #region LeftProperty
        public double Left
        {
            get { return Canvas.GetLeft(this); }
            set
            {
                Canvas.SetLeft(this, value);
                OnLeftChanged(value);
            }
        }

        public event Action<double> OnLeftChangedEvent;
        public void OnLeftChanged(double value)
        {
            OnLeftChangedEvent?.Invoke(value);
        }
        #endregion

        #region TopProperty
        public double Top
        {
            get { return Canvas.GetTop(this); }
            set
            {
                Canvas.SetTop(this, value);
                OnTopChanged(value);
            }
        }

        public event Action<double> OnTopChangedEvent;
        public void OnTopChanged(double value)
        {
            OnTopChangedEvent?.Invoke(value);
        }
        #endregion

        #region ZIndexProperty
        public int ZIndex
        {
            get { return Canvas.GetZIndex(this); }
            set
            {
                Canvas.SetZIndex(this, value);
                OnZIndexChanged(value);
            }
        }

        public event Action<int> OnZIndexChangedEvent;
        public void OnZIndexChanged(int value)
        {
            OnZIndexChangedEvent?.Invoke(value);
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
            else if (Select && HitFill(parentLocation))
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
                X = Left - (double)Resources["contentRectangleDiff"],
                Y = Top - (double)Resources["contentRectangleDiff"],
                Width = (double)Resources["handleSize"],
                Height = (double)Resources["handleSize"],
            };
            return handleRect.Contains(parentLocation);
        }

        private bool HitTopRight(Point parentLocation)
        {
            Rect handleRect = new Rect()
            {
                X = Left + ActualWidth - (double)Resources["contentRectangleDiff"],
                Y = Top - (double)Resources["contentRectangleDiff"],
                Width = (double)Resources["handleSize"],
                Height = (double)Resources["handleSize"],
            };
            return handleRect.Contains(parentLocation);
        }

        private bool HitBottomLeft(Point parentLocation)
        {
            Rect handleRect = new Rect()
            {
                X = Left - (double)Resources["contentRectangleDiff"],
                Y = Top + ActualHeight - (double)Resources["contentRectangleDiff"],
                Width = (double)Resources["handleSize"],
                Height = (double)Resources["handleSize"],
            };
            return handleRect.Contains(parentLocation);
        }

        private bool HitBottomRight(Point parentLocation)
        {
            Rect handleRect = new Rect()
            {
                X = Left + ActualWidth - (double)Resources["contentRectangleDiff"],
                Y = Top + ActualHeight - (double)Resources["contentRectangleDiff"],
                Width = (double)Resources["handleSize"],
                Height = (double)Resources["handleSize"],
            };
            return handleRect.Contains(parentLocation);
        }
        #endregion
    }
}
