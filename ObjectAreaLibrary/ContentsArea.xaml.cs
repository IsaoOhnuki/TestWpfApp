using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
                }
            }
            else
            {
                areaItem.Select = value;
            }
        }
        #endregion
    }
}
