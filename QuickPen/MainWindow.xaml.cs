using QuickPen.Actions;
using QuickPen.WindowAdj;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace QuickPen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GlobalHotKey Q_Alt_HotKey;
        private readonly DrawingAttributes DrawingPen = new() { Color = System.Windows.Media.Color.FromRgb(255, 255, 255), Width = 5, Height = 5 };
        private readonly DrawingAttributes Eraser = new DrawingAttributes() { Height = 30, Width = 30 };

        Bitmap Framebmb;
        Graphics graphics;

        System.Drawing.Point WidthAndHeight;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            SourceInitialized += MainWindow_SourceInitialized;
            WidthAndHeight = new() { X = SystemInformation.VirtualScreen.Width, Y = SystemInformation.VirtualScreen.Height };

            this.Dispatcher.Invoke(async () =>
            {
                using (Bitmap bmp = new(WidthAndHeight.X, WidthAndHeight.Y))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        System.Drawing.Size s = new() { Width = WidthAndHeight.X, Height = WidthAndHeight.Y };

                        g.CopyFromScreen(System.Drawing.Point.Empty, System.Drawing.Point.Empty, s);

                        g.Flush();

                        this.Wallpaper.Source = await BitmapToImageSource(bmp);
                    }

                }
            });
            this.DrawCanvas.DefaultDrawingAttributes = DrawingPen;
            this.Visibility = Visibility.Visible;
        }

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


                if (this.IsVisible)
                {
                    this.Visibility = Visibility.Hidden;
                }
                else
                {
                    this.Dispatcher.Invoke(async () =>
                    {

                        using (Bitmap bmp = new(WidthAndHeight.X, WidthAndHeight.Y))
                        {
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                System.Drawing.Size s = new() { Width = WidthAndHeight.X, Height = WidthAndHeight.Y };

                                g.CopyFromScreen(System.Drawing.Point.Empty, System.Drawing.Point.Empty, s);

                                g.Flush();

                                this.Wallpaper.Source = await BitmapToImageSource(bmp);
                            }

                        }
                    });

                    this.Visibility = Visibility.Visible;
                }
            }

            return IntPtr.Zero;
        }

        private async Task<BitmapImage> BitmapToImageSource(System.Drawing.Bitmap bitmap)
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
                bitmapimage.Freeze();
                return bitmapimage;
            }
        }
        Slider ColorSlider = new() { Width = 30, Height = 30 };
        private void SelectColor_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectColor.IsChecked == false) return;
            if (this.UseEraser.IsChecked == true) this.UseEraser.IsChecked = false;
            if (this.UsePen.IsChecked == true) this.UsePen.IsChecked = false;
 
        }

        private void UsePen_Click(object sender, RoutedEventArgs e)
        {
            //If you clicked the pen button
            if (this.UsePen.IsChecked == false) return;
            if (this.UseEraser.IsChecked == true) this.UseEraser.IsChecked = false;
            if (this.SelectColor.IsChecked == true) this.SelectColor.IsChecked = false;
            this.DrawCanvas.EditingMode = InkCanvasEditingMode.Ink;
            this.DrawCanvas.DefaultDrawingAttributes = DrawingPen;
        }

        private void UseEraser_Click(object sender, RoutedEventArgs e)
        {
            if (this.UseEraser.IsChecked == false) return;
            if (this.UsePen.IsChecked == true) this.UsePen.IsChecked = false;
            if (this.SelectColor.IsChecked == true) this.SelectColor.IsChecked = false;
            if(EraseByStroke.IsChecked==true) this.DrawCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
            else
            this.DrawCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
            
        }

        private void UseEraser_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                EraserPop.IsOpen = !EraserPop.IsOpen;

            }
        }

        private void EraseByPoint_Click(object sender, RoutedEventArgs e)
        {
            if (EraseByPoint.IsChecked == false && EraseByStroke.IsChecked == false) EraseByPoint.IsChecked = true;
            EraseByStroke.IsChecked = false;
            if (EraseByPoint.IsChecked == true)
            {
                this.DrawCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                this.EraserPop.IsOpen = false;
            }
        }

        private void EraseByStroke_Click(object sender, RoutedEventArgs e)
        {
            if (EraseByPoint.IsChecked == false && EraseByStroke.IsChecked == false) EraseByStroke.IsChecked = true;
            EraseByPoint.IsChecked = false;
            if (EraseByStroke.IsChecked == true)
            {
                this.DrawCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
                this.EraserPop.IsOpen = false;
                
            }
        }
    }
}