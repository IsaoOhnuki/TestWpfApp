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
    /// このカスタム コントロールを XAML ファイルで使用するには、手順 1a または 1b の後、手順 2 に従います。
    ///
    /// 手順 1a) 現在のプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ObjectAreaLibrary"
    ///
    ///
    /// 手順 1b) 異なるプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ObjectAreaLibrary;assembly=ObjectAreaLibrary"
    ///
    /// また、XAML ファイルのあるプロジェクトからこのプロジェクトへのプロジェクト参照を追加し、
    /// リビルドして、コンパイル エラーを防ぐ必要があります:
    ///
    ///     ソリューション エクスプローラーで対象のプロジェクトを右クリックし、
    ///     [参照の追加] の [プロジェクト] を選択してから、このプロジェクトを参照し、選択します。
    ///
    ///
    /// 手順 2)
    /// コントロールを XAML ファイルで使用します。
    ///
    ///     <MyNamespace:ContentsAreaItemControl/>
    ///
    /// </summary>
    public class ContentsAreaItemControl : Control
    {
        static ContentsAreaItemControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentsAreaItemControl), new FrameworkPropertyMetadata(typeof(ContentsAreaItemControl)));
        }

        #region ContentsCanvasProperty
        private Canvas _contentPanel;
        public Canvas ContentPanel { get { return _contentPanel ??= (Canvas)Template?.FindName("canvas", this); } }
        #endregion

        #region SelectedProperty
        public static readonly DependencyProperty SelectedProperty = DependencyProperty.RegisterAttached(
            nameof(Selected),
            typeof(bool),
            typeof(ContentsAreaItem),
            new FrameworkPropertyMetadata(default(bool)));

        public bool Selected
        {
            get { return (bool)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }
        #endregion

        #region EditProperty
        public static readonly DependencyProperty EditProperty = DependencyProperty.RegisterAttached(
            nameof(Edit),
            typeof(bool),
            typeof(ContentsAreaItem),
            new FrameworkPropertyMetadata(default(bool)));

        public bool Edit
        {
            get { return (bool)GetValue(EditProperty); }
            set { SetValue(EditProperty, value); }
        }
        #endregion

        #region LeftProperty
        public double Left
        {
            get { return Canvas.GetLeft(this); }
            set { Canvas.SetLeft(this, value); }
        }

        public delegate void LeftChangedEvent(double value);
        public event LeftChangedEvent OnLeftChangedEvent;

        public void OnLeftChanged(double value)
        {
            OnLeftChangedEvent(value);
        }
        #endregion

        #region RightProperty
        public double Right
        {
            get { return Canvas.GetRight(this); }
            set { Canvas.SetRight(this, value); }
        }

        public delegate void RightChangedEvent(double value);
        public event RightChangedEvent OnRightChangedEvent;

        public void OnRightChanged(double value)
        {
            OnRightChangedEvent(value);
        }
        #endregion

        #region ZIndexProperty
        public int ZIndex
        {
            get { return Canvas.GetZIndex(this); }
            set { Canvas.SetZIndex(this, value); }
        }

        public delegate void ZIndexChangedEvent(int value);
        public event ZIndexChangedEvent OnZIndexChangedEvent;

        public void OnZIndexChanged(int value)
        {
            OnZIndexChangedEvent(value);
        }
        #endregion
    }
}
