using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ObjectAreaLibrary
{
    using AreaItems = List<ContentsContainer>;
    using AreaItemsContainer = Dictionary<string, List<ContentsContainer>>;

    public interface IAreaItem
    {
        ContentsArea ParentArea { get; }
        string Group { get; set; }
        bool Select { get; set; }
        bool Edit { get; set; }
        event Action<ContentsContainer, bool> SelectChangedEvent;
        event Action<ContentsContainer, string> GroupChangedEvent;
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
            ItemGroup = new AreaItemsContainer();
        }

        #region AreaItemFunction
        public static ContentsArea GetItemParentArea(object areaItem)
        {
            return (areaItem as FrameworkElement)?.Parent as ContentsArea;
        }

        public static string GetItemGroup(object areaItem)
        {
            return (string)(areaItem as DependencyObject)?.GetValue(GroupProperty);
        }

        public static void SetItemGroup(object areaItem, string value)
        {
            (areaItem as DependencyObject)?.SetValue(GroupProperty, value);
        }

        public static bool GetItemSelect(object areaItem)
        {
            return (bool)(areaItem as DependencyObject)?.GetValue(SelectProperty);
        }

        public static void SetItemSelect(object areaItem, bool value)
        {
            (areaItem as DependencyObject)?.SetValue(SelectProperty, value);
        }

        public static bool GetItemEdit(object areaItem)
        {
            return (bool)(areaItem as DependencyObject)?.GetValue(EditProperty);
        }

        public static void SetItemEdit(object areaItem, bool value)
        {
            (areaItem as DependencyObject)?.SetValue(EditProperty, value);
        }
        #endregion

        #region GroupProperty
        public static readonly DependencyProperty GroupProperty = DependencyProperty.RegisterAttached(
            nameof(Group),
            typeof(string),
            typeof(ContentsContainer),
            new FrameworkPropertyMetadata(default(string), (d, e) => {
                if (d is IAreaItem areaItem)
                {
                    areaItem.ParentArea?.OnGroupChanged(areaItem, (string)e.NewValue);
                }
            }));

        public string Group
        {
            get { return (string)GetValue(GroupProperty); }
            set { SetValue(GroupProperty, value); }
        }

        public event Action<IAreaItem, string> GroupChangedEvent;
        public void OnGroupChanged(IAreaItem areaItem, string value)
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
                if (d is IAreaItem areaItem)
                {
                    areaItem.ParentArea?.OnSelectChanged(areaItem, (bool)e.NewValue);
                }
            }));

        public bool Select
        {
            get { return (bool)GetValue(SelectProperty); }
            set { SetValue(SelectProperty, value); }
        }

        public event Action<IAreaItem, bool> SelectChangedEvent;
        public void OnSelectChanged(IAreaItem areaItem, bool value)
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
                if (d is IAreaItem areaItem)
                {
                    areaItem.ParentArea?.OnEditChanged(areaItem, (bool)e.NewValue);
                }
            }));
        public bool Edit
        {
            get { return (bool)GetValue(EditProperty); }
            set { SetValue(EditProperty, value); }
        }

        public event Action<IAreaItem, bool> EditChangedEvent;
        public void OnEditChanged(IAreaItem areaItem, bool value)
        {
            EditChangedEvent?.Invoke(areaItem, value);
        }
        #endregion

        #region ItemGroupProperty
        private static readonly DependencyPropertyKey ItemGroupPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(ItemGroup),
            typeof(AreaItemsContainer),
            typeof(ContentsArea),
            new FrameworkPropertyMetadata(default));

        public static readonly DependencyProperty ItemGroupProperty = ItemGroupPropertyKey.DependencyProperty;

        public AreaItemsContainer ItemGroup
        {
            get { return (AreaItemsContainer)GetValue(ItemGroupProperty); }
            private set { SetValue(ItemGroupPropertyKey, value); }
        }

        private void ItemGrouped(ContentsContainer areaItem, string value)
        {
            foreach (var group in ItemGroup)
            {
                if (group.Value.IndexOf(areaItem) > 0)
                {
                    group.Value.Remove(areaItem);
                    break;
                }
            }
            if (!string.IsNullOrEmpty(value))
            {
                ItemGroup[value].Add(areaItem);
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

        public AreaItems Selected
        {
            get { return (AreaItems)GetValue(SelectedProperty); }
            private set { SetValue(SelectedPropertyKey, value); }
        }

        private void ItemSelected(ContentsContainer areaItem, bool value)
        {
            if (value)
            {
                Selected.Add(areaItem);
            }
            else
            {
                Selected.Remove(areaItem);
            }
        }

        public void ClearSelected()
        {
            foreach (var item in Selected.OfType<IAreaItem>().Reverse())
            {
                item.Edit = false;
                item.Select = false;
            }
        }
        #endregion

        #region AreaItemsProperty
        public void AddAreaItem(IAreaItem areaItem)
        {
            if (!contentsCanvas.Children.Contains(areaItem as FrameworkElement))
            {
                areaItem.SelectChangedEvent += ItemSelected;
                areaItem.GroupChangedEvent += ItemGrouped;
                contentsCanvas.Children.Add(areaItem);
                ItemSelected(areaItem, areaItem.Select);
                ItemGrouped(areaItem, areaItem.Group);
            }
        }

        public void RemoveAreaItem(ContentsContainer areaItem)
        {
            if (contentsCanvas.Children.Contains(areaItem))
            {
                areaItem.Select = false;
                areaItem.Group = "";
                contentsCanvas.Children.Remove(areaItem);
                areaItem.SelectChangedEvent -= ItemSelected;
                areaItem.GroupChangedEvent -= ItemGrouped;
            }
        }

        public void ItemMove(ContentsContainer areaItem, Point delta)
        {
            if (!string.IsNullOrEmpty(areaItem.Group))
            {
                foreach (var item in ItemGroup[areaItem.Group])
                {
                    item.Left += delta.X;
                    item.Top += delta.Y;
                }
            }
            else
            {
                areaItem.Left += delta.X;
                areaItem.Top += delta.Y;
            }
        }

        public void ItemSelect(ContentsContainer areaItem, bool value)
        {
            if (!string.IsNullOrEmpty(areaItem.Group))
            {
                foreach (var item in ItemGroup[areaItem.Group])
                {
                    item.Select = value;
                    item.Edit = value;
                }
            }
            else
            {
                areaItem.Select = value;
                areaItem.Edit = value;
            }
        }

        public ContentsContainer FindItem(Point location)
        {
            foreach (var child in contentsCanvas.Children.OfType<UIElement>().Reverse())
            {
                if (child is ContentsContainer areaItem)
                {
                    var rect = new Rect(areaItem.Left, areaItem.Top, areaItem.Width, areaItem.Height);
                    if (rect.Contains(location))
                    {
                        return areaItem;
                    }
                }
            }
            return null;
        }
        #endregion

        #region ItemResize
        Point? _downPos = null;
        HandleType _handleType;
        ContentsContainer _editItem;

        private void contentsCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var location = e.GetPosition(sender as IInputElement);
            if (!_downPos.HasValue)
            {
                _handleType = HandleType.None;
                ContentsContainer areaItem = FindItem(location);
                if (areaItem != null)
                {
                    _handleType = areaItem.HitHandle(location);
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
                    _editItem.Resize(_handleType, location);
                }
            }
        }

        private void contentsCanvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var location = e.GetPosition(sender as IInputElement);
            if (_handleType == HandleType.None)
            {
                ClearSelected();
                _editItem = FindItem(location);
                if (_editItem != null)
                {
                    _downPos = location;
                    _handleType = HandleType.Fill;
                    ItemSelect(_editItem, true);
                    _editItem.Resizing(_handleType, location);
                }
            }
            else
            {
                if (_editItem != null)
                {
                    _downPos = location;
                    _editItem.Resizing(_handleType, location);
                }
            }
        }

        private void contentsCanvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _downPos = null;
        }
        #endregion
    }
}
