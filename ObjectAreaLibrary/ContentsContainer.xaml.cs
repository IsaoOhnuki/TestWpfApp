using System;
using System.Windows;
using System.Windows.Controls;

namespace ObjectAreaLibrary
{
    /// <summary>
    /// ContentsAreaItem.xaml の相互作用ロジック
    /// </summary>
    public partial class ContentsContainer : UserControl
    {
        public ContentsContainer()
        {
            // InitializeComponent前
            BorderThicknessProperty.OverrideMetadata(
                typeof(ContentsContainer),
                new FrameworkPropertyMetadata(default(Thickness), (d, e) => {
                    if (d is ContentsContainer areaItem)
                    {
                        Thickness thickness = (Thickness)e.NewValue;
                        CanvasMargin = new Thickness(-(thickness.Left + 5), -(thickness.Top + 5), -(thickness.Right + 5), -(thickness.Bottom + 5));
                    }
                }));

            InitializeComponent();

            Left = 0;
            Top = 0;
            ZIndex = 0;
            Select = (Visibility)Resources["selecting"] == Visibility.Visible;
            Edit = (Visibility)Resources["editing"] == Visibility.Visible;
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
        {
            get { return (Thickness)GetValue(CanvasMarginProperty); }
            set { SetValue(CanvasMarginProperty, value); }
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
    }
}
