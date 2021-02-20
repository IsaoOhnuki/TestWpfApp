using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ContentsCanvas
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
        event Action<IAreaContents, Point> LocationChangedEvent;

        void OnGroupChanged(IAreaContents areaItem, string value);

        void OnSecteChanged(IAreaContents areaItem, bool value);

        void OnLocationChanged(IAreaContents areaItem, Point value);
    }

    /// <summary>
    /// ContentsAreaの動作定義クラス
    /// </summary>
    public class ContentsAreaBase
    {
        public ContentsAreaBase(Canvas canvas)
        {
            ContentsCanvas = canvas;
            AreaItemSelectOperator = new AreaItemSelectOperator(canvas);
            AreaItemOperator = new AreaItemOperator(canvas);
        }

        #region CanvasProperty
        public Canvas ContentsCanvas { get; private set; }

        public void Add(IAreaContents areaItem)
        {
            if (areaItem is UIElement uIElement)
            {
                if (!ContentsCanvas.Children.Contains(uIElement))
                {
                    ContentsCanvas.Children.Add(uIElement);
                    SetGroupe(areaItem, areaItem.Group);
                    SetSelect(areaItem, areaItem.Select);
                }
            }
        }

        public void Remove(IAreaContents areaItem)
        {
            if (areaItem is UIElement uIElement)
            {
                if (ContentsCanvas.Children.Contains(uIElement))
                {
                    ContentsCanvas.Children.Remove(uIElement);
                    if (string.IsNullOrEmpty(areaItem.Group))
                    {
                        Grouped[areaItem.Group].Remove(areaItem);
                        if (Grouped[areaItem.Group].Count == 0)
                        {
                            Grouped.Remove(areaItem.Group);
                        }
                    }
                    if (areaItem.Select)
                    {
                        AreaItemSelectOperator.Remove(areaItem);
                        Selected.Remove(areaItem);
                    }
                }
            }
        }

        public void Clear()
        {
            foreach (var child in ContentsCanvas.Children.OfType<UIElement>().Reverse())
            {
                if (child is IAreaContents areaItem)
                {
                    Remove(areaItem);
                }
            }
        }
        #endregion

        #region GroupedProperty
        public AreaItemsContainer Grouped { get; } = new AreaItemsContainer();

        public void SetGroupe(IAreaContents areaItem, string value)
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
        public AreaItems Selected { get; } = new AreaItems();

        private AreaItemSelectOperator AreaItemSelectOperator { get; set; }

        public void SetSelect(IAreaContents areaItem, bool value)
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
        #endregion

        #region AreaItemsFunction
        private AreaItemOperator AreaItemOperator { get; set; }

        public IAreaContents Find(Point location, bool select = false)
        {
            if (select && AreaItemSelectOperator.SelectOperators.Count > 0)
            {
                foreach (var child in ContentsCanvas.Children.OfType<ContentsOperator>())
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
            foreach (var child in ContentsCanvas.Children.OfType<UIElement>().Reverse())
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

        public void ClearAllSelect()
        {
            foreach (var item in Selected.OfType<IAreaContents>().Reverse())
            {
                item.Select = false;
            }
        }

        public void SelectFill(Rect fill, bool revers)
        {
            foreach (var child in ContentsCanvas.Children.OfType<UIElement>().Reverse())
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

        public void StartRelocation(HandleType handleType, Point location)
        {
            AreaItemSelectOperator.SelectOperators.ForEach(x => x.Resizing(handleType, location));
        }

        public void Relocation(HandleType handleType, Point location)
        {
            AreaItemSelectOperator.SelectOperators.ForEach(x => x.Resize(handleType, location));
        }
        #endregion

        #region MouseFunction
        public void CanvasMouseMove(IInputElement sender, MouseEventArgs e)
        {
            var handleType = AreaItemOperator.IsCatchAreaItem
                ? AreaItemOperator.Move(e.GetPosition(sender), Relocation)
                : AreaItemOperator.Find(e.GetPosition(sender), Find, AreaItemSelectOperator.GetSelectOperator);

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

        public void CanvasMouseDown(IInputElement sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Mouse.Capture(sender);
                bool ctrlKey = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                AreaItemOperator.Catch(e.GetPosition(sender), ctrlKey, Find, StartRelocation, ClearAllSelect);
            }
        }

        public void CanvasMouseUp(MouseButtonEventArgs e)
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

    /// <summary>
    /// ContentsArea内で選択矩形を表示するクラス
    /// </summary>
    internal class AreaItemSelectOperator
    {
        public Canvas ContentsCanvas { get; private set; }

        public List<ContentsOperator> SelectOperators { get; } = new List<ContentsOperator>();

        public AreaItemSelectOperator(Canvas canvas)
        {
            ContentsCanvas = canvas;
        }

        public void Add(IAreaContents areaItem)
        {
            var selectOperator = new ContentsOperator()
            {
                Contents = areaItem,
            };
            SelectOperators.Add(selectOperator);
            ContentsCanvas.Children.Add(selectOperator);
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
                ContentsCanvas.Children.Remove(selectOperator);
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

    /// <summary>
    /// ContentsArea内でContentsを操作するためのクラス
    /// </summary>
    internal class AreaItemOperator
    {
        public Canvas ContentsCanvas { get; private set; }

        private ContentsSelector _contentsSelector;
        private Point? _downPos = null;

        public HandleType HandleType { get; private set; }

        public bool IsCatchAreaItem { get => _downPos.HasValue; }

        public AreaItemOperator(Canvas canvas)
        {
            ContentsCanvas = canvas;
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
                    ContentsCanvas.Children.Add(_contentsSelector);
                }
            }
        }

        public void Release(bool function, Action<Rect, bool> selectFill)
        {
            _downPos = null;
            if (_contentsSelector != null)
            {
                var selectBounds = _contentsSelector.SelectedBounds;
                ContentsCanvas.Children.Remove(_contentsSelector);
                _contentsSelector = null;
                selectFill(selectBounds, function);
            }
        }
    }
}
