using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ObjectAreaLibrary
{
    using AreaItems = List<IAreaContents>;
    using AreaItemsContainer = Dictionary<string, List<IAreaContents>>;

    /// <summary>
    /// ContentsAreaのアイテムinterface
    /// </summary>
    public interface IAreaContents
    {
        ContentsArea ParentArea { get; }
        double Left { get; set; }
        double Top { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        int ZIndex { get; set; }
        string Group { get; set; }
        bool Select { get; set; }
        Rect Bounds { get; }
        event Action<IAreaContents, bool> SelectChangedEvent;
        event Action<IAreaContents, string> GroupChangedEvent;
        void OnGroupChanged(IAreaContents areaItem, string value);
        void OnSecteChanged(IAreaContents areaItem, bool value);
    }

    /// <summary>
    /// IAreaContentsのモッククラス
    /// </summary>
    public class MockAreaContents : TextBox, IAreaContents
    {
        public MockAreaContents()
        {
            IsReadOnly = true;
            Binding textBinding = new Binding("TextSetter")
            {
                Source = this,
                // TextSetterProperty > TextProperty
                Mode = BindingMode.OneWay,
            };
            BindingOperations.SetBinding(this, TextBox.TextProperty, textBinding);

            Binding backgroundBinding = new Binding("BackgroundListener")
            {
                Source = this,
                // BackgroundListenerProperty < BackgroundProperty
                Mode = BindingMode.OneWayToSource,
            };
            BindingOperations.SetBinding(this, Control.BackgroundProperty, backgroundBinding);
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
                        color = ((double)background.Color.R * 0.3 + (double)background.Color.G * 0.59 + (double)background.Color.B * 0.11) / 255;
                    }
                    areaItem.Foreground = new SolidColorBrush(color > 0.5 ? Colors.Black : Colors.White);
                }
            }));

        public static readonly DependencyProperty TextSetterProperty = DependencyProperty.Register(
            "TextSetter",
            typeof(string),
            typeof(MockAreaContents),
            new FrameworkPropertyMetadata(default(string)));
        void SetTextSetter()
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine("Group = '" + Group?.ToString() + "'");
            text.AppendLine("Select = " + Select.ToString());
            SetValue(TextSetterProperty, text.ToString());
        }

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

    /// <summary>
    /// ContentsArea.xaml の相互作用ロジック
    /// </summary>
    public partial class ContentsArea : UserControl
    {
        public ContentsArea()
        {
            InitializeComponent();

            Selected = new AreaItems();
            Grouped = new AreaItemsContainer();
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

        #region CanvasProperty
        public Canvas ContentsCanvas { get => contentsCanvas; }

        public void AddContents(IAreaContents areaItem)
        {
            if (areaItem is UIElement uIElement)
            {
                if (!contentsCanvas.Children.Contains(uIElement))
                {
                    contentsCanvas.Children.Add(uIElement);
                    ItemGrouped(areaItem, areaItem.Group);
                    ItemSelected(areaItem, areaItem.Select);
                }
            }
        }

        public void RemoveContents(IAreaContents areaItem)
        {
            if (areaItem is UIElement uIElement)
            {
                if (contentsCanvas.Children.Contains(uIElement))
                {
                    ItemSelected(areaItem, false);
                    ItemGrouped(areaItem, "");
                    contentsCanvas.Children.Remove(uIElement);
                }
            }
        }
        #endregion

        #region GroupProperty
        public static readonly DependencyProperty GroupProperty = DependencyProperty.RegisterAttached(
            "Group",
            typeof(string),
            typeof(ContentsArea),
            new FrameworkPropertyMetadata(default(string), (d, e) => {
                if (d is IAreaContents areaItem)
                {
                    var parent = areaItem.ParentArea;
                    var value = (string)e.NewValue;
                    parent?.ItemGrouped(areaItem, value);
                    areaItem.OnGroupChanged(areaItem, (string)e.NewValue);
                }
            }));
        #endregion

        #region SelectProperty
        public static readonly DependencyProperty SelectProperty = DependencyProperty.RegisterAttached(
            "Select",
            typeof(bool),
            typeof(ContentsArea),
            new FrameworkPropertyMetadata(false, (d, e) => {
                if (d is IAreaContents areaItem)
                {
                    var parent = areaItem.ParentArea;
                    var value = (bool)e.NewValue;
                    parent?.ItemSelected(areaItem, value);
                    areaItem.OnSecteChanged(areaItem, value);
                }
            }));
        #endregion

        #region GroupedProperty
        private static readonly DependencyPropertyKey GroupedPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Grouped),
            typeof(AreaItemsContainer),
            typeof(ContentsArea),
            new FrameworkPropertyMetadata(default));

        public static readonly DependencyProperty GroupedProperty = GroupedPropertyKey.DependencyProperty;

        public AreaItemsContainer Grouped { get => (AreaItemsContainer)GetValue(GroupedProperty); private set => SetValue(GroupedPropertyKey, value); }

        internal void ItemGrouped(IAreaContents areaItem, string value)
        {
            foreach (var group in Grouped)
            {
                if (group.Value.IndexOf(areaItem) > 0)
                {
                    group.Value.Remove(areaItem);
                    if (group.Value.Count == 0)
                    {
                        Grouped.Remove(value);
                    }
                    break;
                }
            }
            if (!string.IsNullOrEmpty(value))
            {
                if (!Grouped.ContainsKey(value))
                {
                    Grouped[value] = new AreaItems();
                }
                Grouped[value].Add(areaItem);
            }
        }
        #endregion

        #region SelectedProperty
        private static readonly DependencyPropertyKey SelectedPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Selected),
            typeof(AreaItems),
            typeof(ContentsArea),
            new FrameworkPropertyMetadata(default));

        public static readonly DependencyProperty SelectedProperty = SelectedPropertyKey.DependencyProperty;

        public AreaItems Selected { get => (AreaItems)GetValue(SelectedProperty); private set => SetValue(SelectedPropertyKey, value); }

        #region SelectedOperatorFunction
        private List<ContentsOperator> SelectOperators { get; } = new List<ContentsOperator>();

        private void AddSelectOperator(IAreaContents areaItem)
        {
            var selectOperator = new ContentsOperator()
            {
                Contents = areaItem,
            };
            SelectOperators.Add(selectOperator);
            contentsCanvas.Children.Add(selectOperator);
            if (SelectOperators.Count == 1)
            {
                selectOperator.Edit = true;
            }
            else
            {
                SelectOperators.ForEach(item => item.Edit = false);
            }
        }

        private void RemoveSelectOperator(IAreaContents areaItem)
        {
            var selectOperator = GetSelectOperator(areaItem);
            if (selectOperator != null)
            {
                contentsCanvas.Children.Remove(selectOperator);
                SelectOperators.Remove(selectOperator);
            }
            if (SelectOperators.Count == 1)
            {
                SelectOperators[0].Edit = true;
            }
        }

        private ContentsOperator GetSelectOperator(IAreaContents areaItem)
        {
            return SelectOperators.Where(x => x.Contents == areaItem).FirstOrDefault();
        }
        #endregion

        internal void ItemSelected(IAreaContents areaItem, bool value)
        {
            if (!string.IsNullOrEmpty(areaItem.Group))
            {
                Debug.Assert(Grouped.ContainsKey(areaItem.Group));
                Grouped[areaItem.Group].ForEach(item =>
                {
                    if (item.Select != value)
                    {
                        item.Select = value;
                    }
                    if (value)
                    {
                        Selected.Add(item);
                        AddSelectOperator(item);
                    }
                    else
                    {
                        RemoveSelectOperator(item);
                        Selected.Remove(item);
                    }
                });
            }
            else
            {
                if (value)
                {
                    Selected.Add(areaItem);
                    AddSelectOperator(areaItem);
                }
                else
                {
                    RemoveSelectOperator(areaItem);
                    Selected.Remove(areaItem);
                }
            }
        }

        public void ClearSelected()
        {
            foreach (var item in Selected.OfType<IAreaContents>().Reverse())
            {
                item.Select = false;
            }
        }
        #endregion

        #region AreaItemsProperty
        public void ItemMoving(HandleType handleType, Point location)
        {
            SelectOperators.ForEach(x => x.Resizing(handleType, location));
        }

        public void ItemMove(HandleType handleType, Point location)
        {
            SelectOperators.ForEach(x => x.Resize(handleType, location));
        }

        public IAreaContents FindItem(Point location, bool select = false)
        {
            if (select && SelectOperators.Count > 0)
            {
                foreach (var child in contentsCanvas.Children.OfType<ContentsOperator>())
                {
                    if (child is ContentsOperator selectOperator)
                    {
                        var hit = selectOperator.HitHandle(location);
                        if (hit != HandleType.None)
                        {
                            return child.Contents;
                        }
                    }
                }
            }
            foreach (var child in contentsCanvas.Children.OfType<UIElement>().Reverse())
            {
                if (child is IAreaContents areaItem)
                {
                    if (areaItem.Bounds.Contains(location))
                    {
                        return areaItem;
                    }
                }
            }
            return null;
        }

        private void SelectFill(Rect fill, bool remove)
        {
            foreach (var child in contentsCanvas.Children.OfType<UIElement>().Reverse())
            {
                if (child is IAreaContents areaItem)
                {
                    var intersect = Rect.Intersect(areaItem.Bounds, fill);
                    if (intersect.Width > 0 || intersect.Height > 0)
                    {
                        areaItem.Select = !remove;
                    }
                }
            }
        }
        #endregion

        #region ItemResize
        Point? _downPos = null;
        HandleType _handleType;
        ContentsSelector _contentsSelector;

        public HandleType FindAreaItem(Point location)
        {
            if (!_downPos.HasValue)
            {
                _handleType = HandleType.None;
                IAreaContents areaItem = FindItem(location, true);
                if (areaItem != null)
                {
                    var select = GetSelectOperator(areaItem);
                    if (select != null)
                    {
                        _handleType = select.HitHandle(location);
                    }
                    else
                    {
                        _handleType = HandleType.Fill;
                    }
                }
            }
            else if (_handleType != HandleType.None)
            {
                _downPos = location;
                ItemMove(_handleType, location);
            }
            else
            {
                _contentsSelector.SelectedBounds = new Rect(_downPos.Value, location);
            }
            return _handleType;
        }

        public void ConfirmAreaItem(Point location, bool ctrlKey)
        {
            if (_handleType != HandleType.None && _handleType != HandleType.Fill)
            {
                _downPos = location;
                ItemMoving(_handleType, location);
            }
            else
            {
                IAreaContents areaItem = FindItem(location);
                if (areaItem != null)
                {
                    if (ctrlKey)
                    {
                        areaItem.Select = !areaItem.Select;
                    }
                    else
                    {
                        if (!areaItem.Select)
                        {
                            ClearSelected();
                        }
                        _downPos = location;
                        areaItem.Select = true;
                        ItemMoving(_handleType, location);
                    }
                }
                else
                {
                    if (!ctrlKey)
                    {
                        ClearSelected();
                    }
                    _downPos = location;
                    _contentsSelector = new ContentsSelector();
                    contentsCanvas.Children.Add(_contentsSelector);
                }
            }
        }

        public void ReleaseAreaItem(bool ctrlKey)
        {
            _downPos = null;
            if (_contentsSelector != null)
            {
                contentsCanvas.Children.Remove(_contentsSelector);
                var selectBounds = _contentsSelector.SelectedBounds;
                _contentsSelector = null;
                SelectFill(selectBounds, ctrlKey);
            }
        }
        #endregion

        #region MouseFunction
        private void contentsCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var handleType = FindAreaItem(e.GetPosition(sender as IInputElement));
            switch (handleType)
            {
                case HandleType.None:
                    Mouse.OverrideCursor = null;
                    break;
                case HandleType.Fill:
                    Mouse.OverrideCursor = Cursors.Hand;
                    break;
                case HandleType.TopLeft:
                    Mouse.OverrideCursor = Cursors.SizeNWSE;
                    break;
                case HandleType.TopRight:
                    Mouse.OverrideCursor = Cursors.SizeNESW;
                    break;
                case HandleType.BottomLeft:
                    Mouse.OverrideCursor = Cursors.SizeNESW;
                    break;
                case HandleType.BottomRight:
                    Mouse.OverrideCursor = Cursors.SizeNWSE;
                    break;
            }
        }

        private void contentsCanvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Mouse.Capture(sender as IInputElement);
                bool ctrlKey = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                ConfirmAreaItem(e.GetPosition(sender as IInputElement), ctrlKey);
            }
        }

        private void contentsCanvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                bool ctrlKey = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                ReleaseAreaItem(ctrlKey);
                Mouse.Capture(null);
            }
        }
        #endregion
    }
}
