using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ObjectAreaLibrary
{
    /// <summary>
    /// ContentsAreaItem.xaml の相互作用ロジック
    /// </summary>
    public partial class ContentsContainer : UserControl
    {
        public ContentsContainer()
        {
            InitializeComponent();
            Width = 150;
            Height = 80;
            Left = 0;
            Right = 0;
            ZIndex = 0;
            Selected = (Visibility)Resources["selecting"] == Visibility.Visible;
            Edit = (Visibility)Resources["editing"] == Visibility.Visible;

            BorderThicknessProperty.OverrideMetadata(
                typeof(ContentsContainer),
                new FrameworkPropertyMetadata(
                    default(Thickness),
                    (d, e) => {
                        if (d is ContentsContainer areaItem)
                        {
                            Thickness thickness = (Thickness)e.NewValue;
                            CanvasMargin = new Thickness(-(thickness.Left + 5), -(thickness.Top + 5), -(thickness.Right + 5), -(thickness.Bottom + 5));
                        }
                    }));
        }

        #region CanvasMarginProperty
        public static readonly DependencyProperty CanvasMarginProperty = DependencyProperty.RegisterAttached(
            nameof(CanvasMargin),
            typeof(Thickness),
            typeof(ContentsContainer),
            new FrameworkPropertyMetadata(default(Thickness)));
        public Thickness CanvasMargin
        {
            get { return (Thickness)GetValue(CanvasMarginProperty); }
            set { SetValue(CanvasMarginProperty, value); }
        }
        #endregion

        #region SelectedProperty
        public static readonly DependencyProperty SelectedProperty = DependencyProperty.RegisterAttached(
            nameof(Selected),
            typeof(bool),
            typeof(ContentsContainer),
            new FrameworkPropertyMetadata(false, (d, e) => {
                if (d is ContentsContainer areaItem)
                {
                    areaItem.OnSelectChanged((bool)e.NewValue);
                }
            }));
        public bool Selected
        {
            get { return (bool)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }

        public event Action<bool> OnSelectChangedEvent;
        public void OnSelectChanged(bool value)
        {
            OnSelectChangedEvent?.Invoke(value);
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
                    areaItem.OnEditChanged((bool)e.NewValue);
                }
            }));
        public bool Edit
        {
            get { return (bool)GetValue(EditProperty); }
            set { SetValue(EditProperty, value); }
        }

        public event Action<bool> OnEditChangedEvent;
        public void OnEditChanged(bool value)
        {
            OnEditChangedEvent?.Invoke(value);
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

        #region RightProperty
        public double Right
        {
            get { return Canvas.GetRight(this); }
            set
            {
                Canvas.SetRight(this, value);
                OnRightChanged(value);
            }
        }

        public event Action<double> OnRightChangedEvent;
        public void OnRightChanged(double value)
        {
            OnRightChangedEvent?.Invoke(value);
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
    }
}
