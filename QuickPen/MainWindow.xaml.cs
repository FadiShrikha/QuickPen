using QuickPen.Actions;
using QuickPen.WindowAdj;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuickPen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GlobalHotKey Q_Alt_HotKey;
        Window CanvasWindow = new() { Topmost =false,WindowState = WindowState.Maximized, WindowStyle= WindowStyle.None};
        bool CanvasWindowIsnopen = false;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            SourceInitialized += MainWindow_SourceInitialized;
            //this.Hide();
        }
        public ImageSource WallImage { get; set; }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            var source = (HwndSource)PresentationSource.FromVisual(this);
            source.AddHook(WndProc);


            var handle = new WindowInteropHelper(this).Handle;

            Q_Alt_HotKey = new GlobalHotKey(Keys.Q, Keys.Alt, handle, GetHashCode());
            Q_Alt_HotKey.register();

        }



        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // is this a HotKey ?

            var MSG = (WindowMessage)msg;
            if (MSG == WindowMessage.WM_HotKey)
            {
                
                this.Dispatcher.Invoke(() =>
                {
                    var GetBitMapFromScreen = FreezFrame.GetAScreenShot();
                    this.Wallpaper.Source = BitmapToImageSource(GetBitMapFromScreen);

                });
                CanvasWindowIsnopen = !CanvasWindowIsnopen;
                if (CanvasWindowIsnopen)
                {
                    this.Show();
                }
                else 
                {
                    this.Hide();
                }
            }

            return IntPtr.Zero;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {


            //DragMove();
        }
        
        private BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }
    }
}