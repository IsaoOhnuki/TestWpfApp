using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ObjectAreaLibrary
{
    /// <summary>
    /// ContentsArea.xaml の相互作用ロジック
    /// </summary>
    public partial class ContentsArea : UserControl
    {
        public ContentsArea()
        {
            InitializeComponent();
        }

        #region AreaItemsProperty
        public void AddAreaItem(ContentsContainer areaItem)
        {
            if (!contentsCanvas.Children.Contains(areaItem))
            {
                contentsCanvas.Children.Add(areaItem);
            }
        }

        public void RemoveAreaItem(ContentsContainer areaItem)
        {
            if (contentsCanvas.Children.Contains(areaItem))
            {
                contentsCanvas.Children.Remove(areaItem);
            }
        }
        #endregion
    }
}
