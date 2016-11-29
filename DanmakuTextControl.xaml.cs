using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DanmukuRPCServer
{
    /// <summary>
    /// DanmakuTextControl.xaml 的交互逻辑
    /// </summary>
    public partial class DanmakuTextControl : UserControl
    {
        public DanmakuTextControl()
        {
            InitializeComponent();
            base.Loaded += new RoutedEventHandler(this.DanmakuTextControl_Loaded);
            Storyboard storyboard = (Storyboard)base.Resources["Storyboard1"];
            Storyboard.SetTarget(storyboard.Children[2], this);
            (storyboard.Children[0] as DoubleAnimationUsingKeyFrames).KeyFrames[1].KeyTime = KeyTime.FromTimeSpan(new TimeSpan(Convert.ToInt64(Store.MainOverlayEffect1 * 10000000.0)));
            (storyboard.Children[1] as DoubleAnimationUsingKeyFrames).KeyFrames[1].KeyTime = KeyTime.FromTimeSpan(new TimeSpan(Convert.ToInt64(Store.MainOverlayEffect1 * 10000000.0)));
            (storyboard.Children[1] as DoubleAnimationUsingKeyFrames).KeyFrames[2].KeyTime = KeyTime.FromTimeSpan(new TimeSpan(Convert.ToInt64((Store.MainOverlayEffect2 + Store.MainOverlayEffect1) * 10000000.0)));
            (storyboard.Children[2] as DoubleAnimationUsingKeyFrames).KeyFrames[0].KeyTime = KeyTime.FromTimeSpan(new TimeSpan(Convert.ToInt64((Store.MainOverlayEffect3 + Store.MainOverlayEffect2 + Store.MainOverlayEffect1) * 10000000.0)));
            (storyboard.Children[2] as DoubleAnimationUsingKeyFrames).KeyFrames[1].KeyTime = KeyTime.FromTimeSpan(new TimeSpan(Convert.ToInt64((Store.MainOverlayEffect4 + Store.MainOverlayEffect3 + Store.MainOverlayEffect2 + Store.MainOverlayEffect1) * 10000000.0)));
        }

        public void ChangeHeight()
		{
			this.TextBox.FontSize = Store.MainOverlayFontsize;
			this.TextBox.Measure(new Size(Store.MainOverlayWidth, 2147483647.0));
			Storyboard storyboard = (Storyboard)base.Resources["Storyboard1"];
			DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames = storyboard.Children[0] as DoubleAnimationUsingKeyFrames;
			doubleAnimationUsingKeyFrames.KeyFrames[1].Value = this.TextBox.DesiredSize.Height;
		}

		private void DanmakuTextControl_Loaded(object sender, RoutedEventArgs e)
		{
			base.Loaded -= new RoutedEventHandler(this.DanmakuTextControl_Loaded);
		}
    }
}
