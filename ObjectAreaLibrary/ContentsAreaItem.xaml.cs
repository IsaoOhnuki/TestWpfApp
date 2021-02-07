using System;
using System.Windows;
using System.Windows.Controls;

namespace ObjectAreaLibrary
{
    /// <summary>
    /// ContentsAreaItem.xaml の相互作用ロジック
    /// </summary>
    public partial class ContentsAreaItem : UserControl
    {
        public ContentsAreaItem()
        {
            InitializeComponent();
            Left = 0;
            Right = 0;
            ZIndex = 0;
            Selected = (Visibility)Resources["selecting"] == Visibility.Visible;
            Edit = (Visibility)Resources["editing"] == Visibility.Visible;
        }

        #region SelectedProperty
        public static readonly DependencyProperty SelectedProperty = DependencyProperty.RegisterAttached(
            nameof(Selected),
            typeof(bool),
            typeof(ContentsAreaItem),
            new FrameworkPropertyMetadata(false, (d, e) => {
                if (d is ContentsAreaItem areaItem)
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
            typeof(ContentsAreaItem),
            new FrameworkPropertyMetadata(false, (d, e) => {
                if (d is ContentsAreaItem areaItem)
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
