using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ObjectAreaLibrary
{
    using AreaItems = List<IAreaItem>;
    using AreaItemsContainer = Dictionary<string, List<IAreaItem>>;

    public interface IAreaItem
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
        void OnGroupChanged(IAreaItem areaItem, string value);
        void OnSecteChanged(IAreaItem areaItem, bool value);
    }

    public class MockElement : UserControl, IAreaItem
    {
        public MockElement()
        {
        }

        public ContentsArea ParentArea => ContentsArea.GetItemParentArea(this);

        public double Left { get => ContentsArea.GetItemLeft(this); set => ContentsArea.SetItemLeft(this, value); }
        public double Top { get => ContentsArea.GetItemTop(this); set => ContentsArea.SetItemTop(this, value); }
        public int ZIndex { get => ContentsArea.GetItemZIndex(this); set => ContentsArea.SetItemZIndex(this, value); }
        public string Group { get => ContentsArea.GetItemGroup(this); set => ContentsArea.SetItemGroup(this, value); }
        public bool Select { get => ContentsArea.GetItemSelect(this); set => ContentsArea.SetItemSelect(this, value); }

        public Rect Bounds => ContentsArea.GetItemBounds(this);

        public event Action<IAreaItem, bool> SelectChangedEvent;
        public event Action<IAreaItem, string> GroupChangedEvent;

        public void OnGroupChanged(IAreaItem areaItem, string value)
        {
            GroupChangedEvent?.Invoke(areaItem, value);
        }

        public void OnSecteChanged(IAreaItem areaItem, bool value)
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
        public static ContentsArea GetItemParentArea(IAreaItem areaItem)
        {
            Debug.Assert(areaItem is FrameworkElement);
            return ((areaItem as FrameworkElement).Parent as Canvas).Parent as ContentsArea;
        }

        public static Rect GetItemBounds(IAreaItem areaItem)
        {
            Debug.Assert(areaItem is FrameworkElement);
            return new Rect(areaItem.Left, areaItem.Top, areaItem.Width, areaItem.Height);
        }

        public static string GetItemGroup(IAreaItem areaItem)
        {
            Debug.Assert(areaItem is DependencyObject);
            return (string)(areaItem as DependencyObject).GetValue(GroupProperty);
        }

        public static void SetItemGroup(IAreaItem areaItem, string value)
        {
            Debug.Assert(areaItem is DependencyObject);
            (areaItem as DependencyObject).SetValue(GroupProperty, value);
        }

        public static bool GetItemSelect(IAreaItem areaItem)
        {
            Debug.Assert(areaItem is DependencyObject);
            return (bool)(areaItem as DependencyObject).GetValue(SelectProperty);
        }

        public static void SetItemSelect(IAreaItem areaItem, bool value)
        {
            Debug.Assert(areaItem is DependencyObject);
            (areaItem as DependencyObject).SetValue(SelectProperty, value);
        }

        public static double GetItemLeft(IAreaItem areaItem)
        {
            Debug.Assert(areaItem is DependencyObject);
            return (double)(areaItem as DependencyObject).GetValue(Canvas.LeftProperty);
        }

        public static void SetItemLeft(IAreaItem areaItem, double value)
        {
            Debug.Assert(areaItem is DependencyObject);
            (areaItem as DependencyObject).SetValue(Canvas.LeftProperty, value);
        }

        public static void SetItemTop(IAreaItem areaItem, double value)
        {
            Debug.Assert(areaItem is DependencyObject);
            (areaItem as DependencyObject).SetValue(Canvas.TopProperty, value);
        }

        public static double GetItemTop(IAreaItem areaItem)
        {
            Debug.Assert(areaItem is DependencyObject);
            return (double)(areaItem as DependencyObject).GetValue(Canvas.TopProperty);
        }

        public static void SetItemZIndex(IAreaItem areaItem, int value)
        {
            Debug.Assert(areaItem is DependencyObject);
            (areaItem as DependencyObject).SetValue(Canvas.ZIndexProperty, value);
        }

        public static int GetItemZIndex(IAreaItem areaItem)
        {
            Debug.Assert(areaItem is DependencyObject);
            return (int)(areaItem as DependencyObject).GetValue(Canvas.ZIndexProperty);
        }
        #endregion

        #region CanvasProperty
        public Canvas ContentsCanvas { get => contentsCanvas; }

        public void AddContents(IAreaItem areaItem)
        {
            if (areaItem is UIElement uIElement)
            {
                if (!contentsCanvas.Children.Contains(uIElement))
                {
                    contentsCanvas.Children.Add(uIElement);
                    ItemSelected(areaItem, areaItem.Select);
                    ItemGrouped(areaItem, areaItem.Group);
                }
            }
        }

        public void RemoveContents(IAreaItem areaItem)
        {
            if (areaItem is UIElement uIElement)
            {
                if (contentsCanvas.Children.Contains(uIElement))
                {
                    ItemGrouped(areaItem, "");
                    ItemSelected(areaItem, false);
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
                if (d is IAreaItem areaItem)
                {
                    var parent = areaItem.ParentArea;
                    var value = (string)e.NewValue;
                    parent.ItemGrouped(areaItem, value);
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
                if (d is IAreaItem areaItem)
                {
                    var parent = areaItem.ParentArea;
                    var value = (bool)e.NewValue;
                    parent.ItemSelected(areaItem, value);
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

        internal void ItemGrouped(IAreaItem areaItem, string value)
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
                if (!Grouped.Keys.Contains(value))
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
        private List<ContentsOperator> SelectOperator { get; } = new List<ContentsOperator>();

        private void AddSelectOperator(IAreaItem areaItem)
        {
            var selectOperator = new ContentsOperator()
            {
                Contents = areaItem,
            };
            SelectOperator.Add(selectOperator);
            contentsCanvas.Children.Add(selectOperator);
            if (SelectOperator.Count == 1)
            {
                selectOperator.Edit = true;
            }
            else
            {
                SelectOperator.ForEach(item => item.Edit = false);
            }
        }

        private void RemoveSelectOperator(IAreaItem areaItem)
        {
            var selectOperator = GetSelectOperator(areaItem);
            if (selectOperator != null)
            {
                contentsCanvas.Children.Remove(selectOperator);
                SelectOperator.Remove(selectOperator);
            }
        }

        private ContentsOperator GetSelectOperator(IAreaItem areaItem)
        {
            return SelectOperator.Where(x => x.Contents == areaItem).FirstOrDefault();
        }
        #endregion

        internal void ItemSelected(IAreaItem areaItem, bool value)
        {
            if (!string.IsNullOrEmpty(areaItem.Group))
            {
                Grouped[areaItem.Group].ForEach(item => {
                    if (value)
                    {
                        Selected.Add(item);
                        AddSelectOperator(item);
                    }
                    else
                    {
                        Selected.Remove(item);
                        RemoveSelectOperator(item);
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
                    Selected.Remove(areaItem);
                    RemoveSelectOperator(areaItem);
                }
            }
        }

        public void ClearSelected()
        {
            foreach (var item in Selected.OfType<IAreaItem>().Reverse())
            {
                item.Select = false;
            }
        }
        #endregion

        #region AreaItemsProperty
        public void ItemMoving(HandleType handleType, Point location)
        {
            SelectOperator.ForEach(x => x.Resizing(handleType, location));
        }

        public void ItemMove(HandleType handleType, Point location)
        {
            SelectOperator.ForEach(x => x.Resize(handleType, location));
        }

        public IAreaItem FindItem(Point location, bool select = false)
        {
            if (select && SelectOperator.Count > 0)
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
                if (child is IAreaItem areaItem)
                {
                    if (areaItem.Bounds.Contains(location))
                    {
                        return areaItem;
                    }
                }
            }
            return null;
        }

        private void SelectFill(Rect fill)
        {
            foreach (var child in contentsCanvas.Children.OfType<UIElement>().Reverse())
            {
                if (child is IAreaItem areaItem)
                {
                    var intersect = Rect.Intersect(areaItem.Bounds, fill);
                    if (intersect.Width > 0 || intersect.Height > 0)
                    {
                        areaItem.Select = true;
                    }
                }
            }
        }
        #endregion

        #region ItemResize
        Point? _downPos = null;
        HandleType _handleType;

        private void contentsCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var location = e.GetPosition(sender as IInputElement);
            if (!_downPos.HasValue)
            {
                _handleType = HandleType.None;
                IAreaItem areaItem = FindItem(location, true);
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
                switch (_handleType)
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
            else
            {
                if (_handleType != HandleType.None)
                {
                    _downPos = location;
                    ItemMove(_handleType, location);
                }
                else
                {
                    var bounds = new Rect(_downPos.Value, location);
                    _contentsSelector.SelectedBounds = bounds;
                }
            }
        }

        ContentsSelector _contentsSelector;

        private void contentsCanvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var location = e.GetPosition(sender as IInputElement);
            if (_handleType != HandleType.None && _handleType != HandleType.Fill)
            {
                _downPos = location;
                ItemMoving(_handleType, location);
            }
            else
            {
                IAreaItem areaItem = FindItem(location);
                if (areaItem != null)
                {
                    if (!areaItem.Select)
                    {
                        ClearSelected();
                    }
                    _downPos = location;
                    areaItem.Select = true;
                    ItemMoving(_handleType, location);
                }
                else
                {
                    ClearSelected();
                    _downPos = location;
                    _contentsSelector = new ContentsSelector();
                    contentsCanvas.Children.Add(_contentsSelector);
                }
            }
        }

        private void contentsCanvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _downPos = null;
            if (_contentsSelector != null)
            {
                contentsCanvas.Children.Remove(_contentsSelector);
                var selectBounds = _contentsSelector.SelectedBounds;
                _contentsSelector = null;
                SelectFill(selectBounds);
            }
        }
        #endregion
    }
}
