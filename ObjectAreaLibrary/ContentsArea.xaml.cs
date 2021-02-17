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
            new FrameworkPropertyMetadata(new AreaItemsContainer()));

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
            new FrameworkPropertyMetadata(new AreaItems()));

        public static readonly DependencyProperty SelectedProperty = SelectedPropertyKey.DependencyProperty;

        public AreaItems Selected { get => (AreaItems)GetValue(SelectedProperty); private set => SetValue(SelectedPropertyKey, value); }

        AreaItemSelectOperator _areaItemSelectOperator;
        AreaItemSelectOperator AreaItemSelectOperator { get => _areaItemSelectOperator ??= new AreaItemSelectOperator(ContentsCanvas); }

        public void ItemSelected(IAreaContents areaItem, bool value)
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
                        AreaItemSelectOperator.Add(item);
                    }
                    else
                    {
                        AreaItemSelectOperator.Remove(item);
                        Selected.Remove(item);
                    }
                });
            }
            else
            {
                if (value)
                {
                    Selected.Add(areaItem);
                    AreaItemSelectOperator.Add(areaItem);
                }
                else
                {
                    AreaItemSelectOperator.Remove(areaItem);
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

        #region AreaItemsFunction
        public void StartMoveItem(HandleType handleType, Point location)
        {
            AreaItemSelectOperator.SelectOperators.ForEach(x => x.Resizing(handleType, location));
        }

        public void MoveItem(HandleType handleType, Point location)
        {
            AreaItemSelectOperator.SelectOperators.ForEach(x => x.Resize(handleType, location));
        }

        public IAreaContents FindItem(Point location, bool select = false)
        {
            if (select && AreaItemSelectOperator.SelectOperators.Count > 0)
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

        public void SelectFill(Rect fill, bool revers)
        {
            foreach (var child in contentsCanvas.Children.OfType<UIElement>().Reverse())
            {
                if (child is IAreaContents areaItem)
                {
                    var intersect = Rect.Intersect(areaItem.Bounds, fill);
                    if (intersect.Width > 0 || intersect.Height > 0)
                    {
                        areaItem.Select = !revers || !areaItem.Select;
                    }
                }
            }
        }
        #endregion

        #region MouseFunction
        AreaItemOperator _areaItemOperator;
        AreaItemOperator AreaItemOperator { get => _areaItemOperator ??= new AreaItemOperator(ContentsCanvas); }

        private void contentsCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var handleType = AreaItemOperator.IsCatchAreaItem
                ? AreaItemOperator.Move(e.GetPosition(sender as IInputElement), MoveItem)
                : AreaItemOperator.Find(e.GetPosition(sender as IInputElement), FindItem, AreaItemSelectOperator.GetSelectOperator);

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
                AreaItemOperator.Catch(e.GetPosition(sender as IInputElement), ctrlKey, FindItem, StartMoveItem, ClearSelected);
            }
        }

        private void contentsCanvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                bool ctrlKey = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                AreaItemOperator.Release(ctrlKey, SelectFill);
                Mouse.Capture(null);
            }
        }
        #endregion
    }

    internal class AreaItemSelectOperator
    {
        private readonly Canvas _canvas;

        public List<ContentsOperator> SelectOperators { get; } = new List<ContentsOperator>();

        public AreaItemSelectOperator(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void Add(IAreaContents areaItem)
        {
            var selectOperator = new ContentsOperator()
            {
                Contents = areaItem,
            };
            SelectOperators.Add(selectOperator);
            _canvas.Children.Add(selectOperator);
            if (SelectOperators.Count == 1)
            {
                selectOperator.Edit = true;
            }
            else
            {
                SelectOperators.ForEach(item => item.Edit = false);
            }
        }

        public void Remove(IAreaContents areaItem)
        {
            var selectOperator = GetSelectOperator(areaItem);
            if (selectOperator != null)
            {
                _canvas.Children.Remove(selectOperator);
                SelectOperators.Remove(selectOperator);
            }
            if (SelectOperators.Count == 1)
            {
                SelectOperators[0].Edit = true;
            }
        }

        public ContentsOperator GetSelectOperator(IAreaContents areaItem)
        {
            return SelectOperators.Where(x => x.Contents == areaItem).FirstOrDefault();
        }
    }

    internal class AreaItemOperator
    {
        private readonly Canvas _canvas;
        private ContentsSelector _contentsSelector;
        private Point? _downPos = null;

        public HandleType HandleType { get; private set; }

        public bool IsCatchAreaItem { get => _downPos.HasValue; }

        public AreaItemOperator(Canvas canvas)
        {
            _canvas = canvas;
        }

        public HandleType Find(Point location, Func<Point, bool, IAreaContents> findItem, Func<IAreaContents, ContentsOperator> getSelectOperator)
        {
            HandleType = HandleType.None;
            IAreaContents areaItem = findItem(location, true);
            if (areaItem != null)
            {
                var select = getSelectOperator(areaItem);
                if (select != null)
                {
                    HandleType = select.HitHandle(location);
                }
                else
                {
                    HandleType = HandleType.Fill;
                }
            }
            return HandleType;
        }

        public HandleType Move(Point location, Action<HandleType, Point> itemMove)
        {
            if (HandleType != HandleType.None)
            {
                _downPos = location;
                itemMove(HandleType, location);
            }
            else
            {
                _contentsSelector.SelectedBounds = new Rect(_downPos.Value, location);
            }
            return HandleType;
        }

        public void Catch(Point location, bool function, Func<Point, bool, IAreaContents> findItem, Action<HandleType, Point> itemRelocation,
            Action clearSelected)
        {
            if (HandleType != HandleType.None && HandleType != HandleType.Fill)
            {
                _downPos = location;
                itemRelocation(HandleType, location);
            }
            else
            {
                IAreaContents areaItem = findItem(location, false);
                if (areaItem != null)
                {
                    if (function)
                    {
                        areaItem.Select = !areaItem.Select;
                    }
                    else
                    {
                        if (!areaItem.Select)
                        {
                            clearSelected();
                        }
                        _downPos = location;
                        areaItem.Select = true;
                        itemRelocation(HandleType, location);
                    }
                }
                else
                {
                    if (!function)
                    {
                        clearSelected();
                    }
                    _downPos = location;
                    _contentsSelector = new ContentsSelector();
                    _canvas.Children.Add(_contentsSelector);
                }
            }
        }

        public void Release(bool function, Action<Rect, bool> selectFill)
        {
            _downPos = null;
            if (_contentsSelector != null)
            {
                var selectBounds = _contentsSelector.SelectedBounds;
                _canvas.Children.Remove(_contentsSelector);
                _contentsSelector = null;
                selectFill(selectBounds, function);
            }
        }
    }
}
