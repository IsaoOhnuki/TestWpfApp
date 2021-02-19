using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ContentsCanvas
{
    /// <summary>
    /// ContentsArea.xaml の相互作用ロジック
    /// </summary>
    public partial class ContentsArea : UserControl
    {
        public Canvas ContentsCanvas { get => ContentsAreaBase.ContentsCanvas; }

        private ContentsAreaBase ContentsAreaBase { get; set; }

        public ContentsArea()
        {
            InitializeComponent();

            ContentsAreaBase = new ContentsAreaBase(contentsCanvas);
        }

        #region AreaItemFunction
        public static ContentsArea GetItemParentArea(IAreaContents areaItem)
        {
            Debug.Assert(areaItem is FrameworkElement);
            return ((areaItem as FrameworkElement).Parent as Canvas)?.Parent as ContentsArea;
        }

        public static Rect GetItemBounds(IAreaContents areaItem)
        {
            Debug.Assert(areaItem is FrameworkElement);
            return new Rect(areaItem.Left, areaItem.Top, areaItem.Width, areaItem.Height);
        }

        public static string GetItemGroup(IAreaContents areaItem)
        {
            Debug.Assert(areaItem is DependencyObject);
            return (string)(areaItem as DependencyObject).GetValue(GroupProperty);
        }

        public static void SetItemGroup(IAreaContents areaItem, string value)
        {
            Debug.Assert(areaItem is DependencyObject);
            (areaItem as DependencyObject).SetValue(GroupProperty, value);
        }

        public static bool GetItemSelect(IAreaContents areaItem)
        {
            Debug.Assert(areaItem is DependencyObject);
            return (bool)(areaItem as DependencyObject).GetValue(SelectProperty);
        }

        public static void SetItemSelect(IAreaContents areaItem, bool value)
        {
            Debug.Assert(areaItem is DependencyObject);
            (areaItem as DependencyObject).SetValue(SelectProperty, value);
        }

        public static double GetItemLeft(IAreaContents areaItem)
        {
            Debug.Assert(areaItem is DependencyObject);
            return (double)(areaItem as DependencyObject).GetValue(Canvas.LeftProperty);
        }

        public static void SetItemLeft(IAreaContents areaItem, double value)
        {
            Debug.Assert(areaItem is DependencyObject);
            (areaItem as DependencyObject).SetValue(Canvas.LeftProperty, value);
        }

        public static void SetItemTop(IAreaContents areaItem, double value)
        {
            Debug.Assert(areaItem is DependencyObject);
            (areaItem as DependencyObject).SetValue(Canvas.TopProperty, value);
        }

        public static double GetItemTop(IAreaContents areaItem)
        {
            Debug.Assert(areaItem is DependencyObject);
            return (double)(areaItem as DependencyObject).GetValue(Canvas.TopProperty);
        }

        public static void SetItemZIndex(IAreaContents areaItem, int value)
        {
            Debug.Assert(areaItem is DependencyObject);
            (areaItem as DependencyObject).SetValue(Canvas.ZIndexProperty, value);
        }

        public static int GetItemZIndex(IAreaContents areaItem)
        {
            Debug.Assert(areaItem is DependencyObject);
            return (int)(areaItem as DependencyObject).GetValue(Canvas.ZIndexProperty);
        }
        #endregion

        #region DependencyProperty
        public static readonly DependencyProperty GroupProperty = DependencyProperty.RegisterAttached(
            "Group",
            typeof(string),
            typeof(ContentsArea),
            new FrameworkPropertyMetadata(default(string), (d, e) => {
                if (d is IAreaContents areaItem)
                {
                    var parent = areaItem.ParentArea;
                    var value = (string)e.NewValue;
                    parent?.ContentsAreaBase.SetGroupe(areaItem, value);
                    areaItem.OnGroupChanged(areaItem, (string)e.NewValue);
                }
            }));

        public static readonly DependencyProperty SelectProperty = DependencyProperty.RegisterAttached(
            "Select",
            typeof(bool),
            typeof(ContentsArea),
            new FrameworkPropertyMetadata(false, (d, e) => {
                if (d is IAreaContents areaItem)
                {
                    var parent = areaItem.ParentArea;
                    var value = (bool)e.NewValue;
                    parent?.ContentsAreaBase.SetSelect(areaItem, value);
                    areaItem.OnSecteChanged(areaItem, value);
                }
            }));
        #endregion

        public void Add(IAreaContents areaItem)
        {
            ContentsAreaBase.Add(areaItem);
        }

        public void Remove(IAreaContents areaItem)
        {
            ContentsAreaBase.Remove(areaItem);
        }

        public void Clear()
        {
            ContentsAreaBase.Clear();
        }

        public IAreaContents Find(Point location, bool select = false)
        {
            return ContentsAreaBase.Find(location, select);
        }

        public void ClearAllSelect()
        {
            ContentsAreaBase.ClearAllSelect();
        }

        public void SelectFill(Rect fill, bool revers)
        {
            ContentsAreaBase.SelectFill(fill, revers);
        }

        public void StartRelocation(HandleType handleType, Point location)
        {
            ContentsAreaBase.StartRelocation(handleType, location);
        }

        public void Relocation(HandleType handleType, Point location)
        {
            ContentsAreaBase.Relocation(handleType, location);
        }

        private void ContentsCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            ContentsAreaBase.CanvasMouseMove(sender as IInputElement, e);
        }

        private void ContentsCanvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ContentsAreaBase.CanvasMouseDown(sender as IInputElement, e);
        }

        private void ContentsCanvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ContentsAreaBase.CanvasMouseUp(e);
        }
    }

    /// <summary>
    /// IAreaContentsのモッククラス
    /// </summary>
    public class MockAreaContents : TextBox, IAreaContents
    {
        public MockAreaContents()
        {
            SnapsToDevicePixels = true;
            IsReadOnly = true;
            Binding textBinding = new Binding("TextSetter")
            {
                Source = this,
                // TextSetterProperty > TextProperty
                Mode = BindingMode.OneWay,
            };
            BindingOperations.SetBinding(this, TextProperty, textBinding);

            Binding backgroundBinding = new Binding("BackgroundListener")
            {
                Source = this,
                // BackgroundListenerProperty < BackgroundProperty
                Mode = BindingMode.OneWayToSource,
            };
            BindingOperations.SetBinding(this, BackgroundProperty, backgroundBinding);
        }

        public static readonly DependencyProperty BackgroundListenerProperty = DependencyProperty.Register(
            "BackgroundListener",
            typeof(Brush),
            typeof(MockAreaContents),
            new FrameworkPropertyMetadata(default(Brush), (d, e) => {
                if (d is MockAreaContents areaItem)
                {
                    double color = 0;
                    if (e.NewValue is SolidColorBrush background)
                    {
                        color = ((double)background.Color.R * 0.3 + (double)background.Color.G * 0.59 + (double)background.Color.B * 0.11) / 256;
                    }
                    areaItem.Foreground = new SolidColorBrush(color > 0.5 ? Colors.Black : Colors.White);
                }
            }));

        public static readonly DependencyProperty TextSetterProperty = DependencyProperty.Register(
            "TextSetter",
            typeof(string),
            typeof(MockAreaContents),
            new FrameworkPropertyMetadata(default(string)));

        public ContentsArea ParentArea { get => ContentsArea.GetItemParentArea(this); }

        public double Left { get => ContentsArea.GetItemLeft(this); set => ContentsArea.SetItemLeft(this, value); }

        public double Top { get => ContentsArea.GetItemTop(this); set => ContentsArea.SetItemTop(this, value); }

        public int ZIndex { get => ContentsArea.GetItemZIndex(this); set => ContentsArea.SetItemZIndex(this, value); }

        public string Group
        {
            get => ContentsArea.GetItemGroup(this);
            set
            {
                ContentsArea.SetItemGroup(this, value);
                SetTextSetter();
            }
        }
        public new bool Select
        {
            get => ContentsArea.GetItemSelect(this);
            set
            {
                ContentsArea.SetItemSelect(this, value);
                SetTextSetter();
            }
        }

        void SetTextSetter()
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine("Group = '" + Group?.ToString() + "'");
            text.AppendLine("Select = " + Select.ToString());
            SetValue(TextSetterProperty, text.ToString());
        }

        public Rect Bounds { get => ContentsArea.GetItemBounds(this); }

        public event Action<IAreaContents, bool> SelectChangedEvent;
        public event Action<IAreaContents, string> GroupChangedEvent;

        public void OnGroupChanged(IAreaContents areaItem, string value)
        {
            GroupChangedEvent?.Invoke(areaItem, value);
        }

        public void OnSecteChanged(IAreaContents areaItem, bool value)
        {
            SelectChangedEvent?.Invoke(areaItem, value);
        }
    }
}
