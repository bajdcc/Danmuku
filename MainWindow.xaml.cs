using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using CookComputing.XmlRpc;

namespace DanmukuRPCServer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WS_EX_TRANSPARENT = 32;

        private const int GWL_EXSTYLE = -20;

        public MainOverlay overlay;

        public FullOverlay fulloverlay;

        private DispatcherTimer timer;

        private DispatcherTimer renderTimer;

        private Danmu danmu;

        private bool showdanmu = true;

        private Queue<string> infos = new Queue<string>(50); 

        private static XmlRpcListenerService _svc = new DanmukuService();

        private System.Windows.Forms.NotifyIcon notifyIcon;

        [DllImport("user32")]
        private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);

        [DllImport("user32")]
        private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);

        public MainWindow()
        {
            InitializeComponent();
        }

        static void ProcessRequest(IAsyncResult result)
        {
            HttpListener listener = result.AsyncState as HttpListener;
            // 结束异步操作
            HttpListenerContext context = listener.EndGetContext(result);
            // 重新启动异步请求处理
            listener.BeginGetContext(ProcessRequest, listener);
            try
            {
                // 处理请求
                _svc.ProcessRequest(context);
            }
            catch (Exception)
            {
            }
        }

        private void FuckMicrosoft(object sender, EventArgs eventArgs)
        {
            if (this.fulloverlay != null)
            {
                this.fulloverlay.Topmost = false;
                this.fulloverlay.Topmost = true;
            }
            if (this.overlay != null)
            {
                this.overlay.Topmost = false;
                this.overlay.Topmost = true;
            }
            if (DanmukuService.close)
            {
                App.Current.Shutdown();
            }
            if (DanmukuService.danmu ^ showdanmu)
            {
                showdanmu = !showdanmu;
                infos.Enqueue("全屏弹幕：" + showdanmu + "\n");
            }
        }

        private void AddDanmu(object sender, EventArgs eventArgs)
        {
            if (DanmukuService.MsgQueue.TryTake(out danmu))
            {
                this.AddDMText(danmu.user, danmu.text, danmu.warn.Equals("true", StringComparison.OrdinalIgnoreCase));
            }
        }

        private void OpenOverlay()
        {
            this.overlay = new MainOverlay();
            this.overlay.Deactivated += new EventHandler(this.overlay_Deactivated);
            this.overlay.SourceInitialized += delegate (object param0, EventArgs param1)
            {
                IntPtr handle = new WindowInteropHelper(this.overlay).Handle;
                uint windowLong = MainWindow.GetWindowLong(handle, -20);
                MainWindow.SetWindowLong(handle, -20, windowLong | 32u);
            };
            this.overlay.Background = Brushes.Transparent;
            this.overlay.ShowInTaskbar = false;
            this.overlay.Topmost = true;
            this.overlay.Top = SystemParameters.WorkArea.Top + Store.MainOverlayXoffset;
            this.overlay.Left = SystemParameters.WorkArea.Right - Store.MainOverlayWidth + Store.MainOverlayYoffset;
            this.overlay.Height = SystemParameters.WorkArea.Height;
            this.overlay.Width = Store.MainOverlayWidth;
        }

        private void overlay_Deactivated(object sender, EventArgs e)
        {
            if (sender is MainOverlay)
            {
                (sender as MainOverlay).Topmost = true;
            }
        }

        private void OpenFullOverlay()
        {
            this.fulloverlay = new FullOverlay();
            this.fulloverlay.Deactivated += new EventHandler(this.fulloverlay_Deactivated);
            this.fulloverlay.Background = Brushes.Transparent;
            this.fulloverlay.SourceInitialized += delegate (object param0, EventArgs param1)
            {
                IntPtr handle = new WindowInteropHelper(this.fulloverlay).Handle;
                uint windowLong = MainWindow.GetWindowLong(handle, -20);
                MainWindow.SetWindowLong(handle, -20, windowLong | 32u);
            };
            this.fulloverlay.ShowInTaskbar = false;
            this.fulloverlay.Topmost = true;
            this.fulloverlay.Top = SystemParameters.WorkArea.Top;
            this.fulloverlay.Left = SystemParameters.WorkArea.Left;
            this.fulloverlay.Width = SystemParameters.WorkArea.Width;
            this.fulloverlay.Height = SystemParameters.WorkArea.Height;
        }

        private void fulloverlay_Deactivated(object sender, EventArgs e)
        {
            if (sender is FullOverlay)
            {
                (sender as FullOverlay).Topmost = true;
            }
        }

        private void s_Completed(object sender, EventArgs e)
        {
            ClockGroup clockGroup = sender as ClockGroup;
            if (clockGroup != null)
            {
                FullScreenDanmaku fullScreenDanmaku = Storyboard.GetTarget(clockGroup.Children[0].Timeline) as FullScreenDanmaku;
                if (fullScreenDanmaku != null)
                {
                    this.fulloverlay.LayoutRoot.Children.Remove(fullScreenDanmaku);
                }
            }
        }

        private void sb_Completed(object sender, EventArgs e)
        {
            ClockGroup clockGroup = sender as ClockGroup;
            if (clockGroup != null)
            {
                DanmakuTextControl danmakuTextControl = Storyboard.GetTarget(clockGroup.Children[2].Timeline) as DanmakuTextControl;
                if (danmakuTextControl != null)
                {
                    this.overlay.LayoutRoot.Children.Remove(danmakuTextControl);
                }
            }
        }

        public void AddDMText(string user, string text, bool warn)
        {
            if (user == null || text == null)
                return;
            if (Dispatcher.CheckAccess())
            {
                while (infos.Count >= 50)
                {
                    infos.Dequeue();
                }
                if (warn)
                {
                    infos.Enqueue($"! {text}\n");
                }
                else
                {
                    if (string.IsNullOrEmpty(user))
                    {
                        infos.Enqueue($"{text}\n");
                    }
                    else
                    {
                        infos.Enqueue($"{user}: {text}\n");
                    }
                }
                this.textBox.Text = string.Join("\n", infos);
                this.textBox.Focus();
                this.textBox.CaretIndex = this.textBox.Text.Length;
                this.textBox.SelectionStart = this.textBox.Text.Length;
                this.textBox.SelectionLength = 0;
                this.textBox.ScrollToEnd();
                if (true)
                {
                    DanmakuTextControl danmakuTextControl = new DanmakuTextControl();
                    danmakuTextControl.UserName.Text = user;
                    if (warn)
                    {
                        danmakuTextControl.UserName.Foreground = Brushes.Red;
                        danmakuTextControl.Text.Foreground = Brushes.LightBlue;
                    }
                    text = text.Trim("\r\n ".ToCharArray());
                    danmakuTextControl.Text.Text = text;
                    danmakuTextControl.ChangeHeight();
                    Storyboard storyboard = (Storyboard)danmakuTextControl.Resources["Storyboard1"];
                    storyboard.Completed += new EventHandler(this.sb_Completed);
                    this.overlay.LayoutRoot.Children.Add(danmakuTextControl);
                }
                if (!warn && showdanmu)
                {
                    lock (this.fulloverlay.LayoutRoot.Children)
                    {
                        FullScreenDanmaku fullScreenDanmaku = new FullScreenDanmaku();
                        fullScreenDanmaku.Text.Text = text;
                        fullScreenDanmaku.ChangeHeight();
                        double width = fullScreenDanmaku.Text.DesiredSize.Width;
                        Dictionary<double, bool> dictionary = new Dictionary<double, bool>();
                        dictionary.Add(0.0, true);
                        foreach (object current in this.fulloverlay.LayoutRoot.Children)
                        {
                            if (current is FullScreenDanmaku)
                            {
                                FullScreenDanmaku fullScreenDanmaku2 = current as FullScreenDanmaku;
                                if (!dictionary.ContainsKey((double)Convert.ToInt32(fullScreenDanmaku2.Margin.Top)))
                                {
                                    dictionary.Add((double)Convert.ToInt32(fullScreenDanmaku2.Margin.Top), true);
                                }
                                if (fullScreenDanmaku2.Margin.Left > SystemParameters.PrimaryScreenWidth - width - 50.0)
                                {
                                    dictionary[(double)Convert.ToInt32(fullScreenDanmaku2.Margin.Top)] = false;
                                }
                            }
                        }
                        double top;
                        if (dictionary.All(p => !p.Value))
                        {
                            top = dictionary.Max(p => p.Key) + fullScreenDanmaku.Text.DesiredSize.Height;
                        }
                        else
                        {
                            top = (from p in dictionary
                                   where p.Value
                                   select p).Min(p => p.Key);
                        }
                        Storyboard storyboard2 = new Storyboard();
                        Duration duration = new Duration(TimeSpan.FromTicks(Convert.ToInt64((SystemParameters.PrimaryScreenWidth + width) / Store.FullOverlayEffect1 * 10000000.0)));
                        ThicknessAnimation thicknessAnimation = new ThicknessAnimation(new Thickness(SystemParameters.PrimaryScreenWidth, top, 0.0, 0.0), new Thickness(-width, top, 0.0, 0.0), duration);
                        storyboard2.Children.Add(thicknessAnimation);
                        storyboard2.Duration = duration;
                        Storyboard.SetTarget(thicknessAnimation, fullScreenDanmaku);
                        Storyboard.SetTargetProperty(thicknessAnimation, new PropertyPath("(FrameworkElement.Margin)", new object[0]));
                        this.fulloverlay.LayoutRoot.Children.Add(fullScreenDanmaku);
                        storyboard2.Completed += new EventHandler(this.s_Completed);
                        storyboard2.Begin();
                    }
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                {
                    this.AddDMText(user, text, warn);
                }));
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, new EventHandler(this.FuckMicrosoft), base.Dispatcher);
            this.timer.Start();
            this.textBox.Text += ("Start timer\n");
            this.renderTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(200), DispatcherPriority.Render, new EventHandler(this.AddDanmu), base.Dispatcher);
            this.renderTimer.Start();
            this.textBox.Text += ("Start consumer\n");
            this.OpenOverlay();
            this.overlay.Show();
            this.textBox.Text += ("Start overlay\n");
            this.OpenFullOverlay();
            this.fulloverlay.Show();
            this.textBox.Text += ("Start full overlay\n");
            try
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://+:7773/");
                listener.Start();
                listener.BeginGetContext(ProcessRequest, listener);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
                App.Current.Shutdown();
            }
            this.textBox.Text += ("Start RPC server\n");


            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.BalloonTipText = "弹幕DA☆ZE";
            notifyIcon.BalloonTipTitle = "DanmukuApp";
            notifyIcon.Text = "DanmukuApp Tray";
            notifyIcon.Icon = System.Drawing.SystemIcons.WinLogo;
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(800);
            notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(notifyIcon_MouseClick);
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[]
            {
                new System.Windows.Forms.MenuItem("关于", (obj, args) =>
                    System.Windows.MessageBox.Show(this, "DanmukuApp by bajdcc", "NewsApp")),
                new System.Windows.Forms.MenuItem("-"),
                new System.Windows.Forms.MenuItem("显示全屏弹幕", (obj, args) =>
                    DanmukuService.danmu = true),
                new System.Windows.Forms.MenuItem("关闭全屏弹幕", (obj, args) =>
                    DanmukuService.danmu = false),
                new System.Windows.Forms.MenuItem("-"),
                new System.Windows.Forms.MenuItem("关闭", (obj, args) =>
                    Close()),
            });

            DelayHide();
        }

        private async void DelayHide()
        {
            await Task.Delay(TimeSpan.FromSeconds(2)).ContinueWith((task) =>
            {
                if (this.Dispatcher.CheckAccess())
                {
                    Hide();
                }
                else
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        Hide();
                    }));
                }
            });
        }

        private void notifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (IsVisible)
            {
                Activate();
            }
            else
            {
                Show();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notifyIcon.Dispose();
        }
    }
}
