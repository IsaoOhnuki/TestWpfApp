using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ObjectAreaLibrary
{
    using AreaItems = List<ContentsContainer>;
    using AreaItemsContainer = Dictionary<string, List<ContentsContainer>>;

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
            foreach (var item in Selected.OfType<ContentsContainer>().Reverse())
            {
                item.Edit = false;
                item.Select = false;
            }
        }
        #endregion

        #region AreaItemsProperty
        public void AddAreaItem(ContentsContainer areaItem)
        {
            if (!contentsCanvas.Children.Contains(areaItem))
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
