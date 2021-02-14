using System.Windows;
using System.Windows.Controls;

namespace ObjectAreaLibrary
{
    /// <summary>
    /// ContentsSelector.xaml の相互作用ロジック
    /// </summary>
    public partial class ContentsSelector : UserControl
    {
        public ContentsSelector()
        {
            InitializeComponent();
        }

        #region LeftProperty
        public double Left { get => Canvas.GetLeft(this); set => Canvas.SetLeft(this, value); }
        #endregion

        #region TopProperty
        public double Top { get => Canvas.GetTop(this); set => Canvas.SetTop(this, value); }
        #endregion

        public Rect SelectedBounds
        {
            get => new Rect(Left, Top, Width, Height);
            set
            {
                var bounds = value;
                if (bounds.Width < 0 || bounds.Height < 0)
                {
                    bounds = new Rect(
                        new Point()
                        {
                            X = bounds.Width < 0 ? bounds.Left + bounds.Width : bounds.Left,
                            Y = bounds.Height < 0 ? bounds.Top + bounds.Height : bounds.Top,
                        },
                        new Size()
                        {
                            Width = bounds.Width < 0 ? -bounds.Width : bounds.Width,
                            Height = bounds.Height < 0 ? -bounds.Height : bounds.Height,
                        });
                }
                Left = bounds.Left;
                Top = bounds.Top;
                Width = bounds.Width;
                Height = bounds.Height;
            }
        }
    }
}
