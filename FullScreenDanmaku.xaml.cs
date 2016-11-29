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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DanmukuRPCServer
{
    /// <summary>
    /// FullScreenDanmaku.xaml 的交互逻辑
    /// </summary>
    public partial class FullScreenDanmaku : UserControl
    {
        public FullScreenDanmaku()
        {
            InitializeComponent();
        }

        public void ChangeHeight()
        {
            this.Text.FontSize = Store.FullOverlayFontsize;
            this.Text.Measure(new Size(2147483647.0, 2147483647.0));
        }
    }
}
