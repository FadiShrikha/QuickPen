using Haley.WPF.Controls;
using QuickPen.Actions;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QuickPen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GlobalHotKey Q_Alt_HotKey;
        private DrawingAttributes _DrawingPen = SetInkCanvasMode.GetDefualtAttributes();
        private SetInkCanvasMode _InkCanvasMode = new();
        private EditMod _editmod = new EditMod();

        System.Drawing.Point WidthAndHeight;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            SourceInitialized += MainWindow_SourceInitialized;
            WidthAndHeight = new() { X = SystemInformation.VirtualScreen.Width, Y = SystemInformation.VirtualScreen.Height };

            this.DrawCanvas.DefaultDrawingAttributes = _DrawingPen;

        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            var source = (HwndSource)PresentationSource.FromVisual(this);
            source.AddHook(WndProc);


            var handle = new WindowInteropHelper(this).Handle;

            Q_Alt_HotKey = new GlobalHotKey(Keys.Q, Keys.Alt, handle, GetHashCode());
            Q_Alt_HotKey.register();

        }


        WriteableBitmap _wb;
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
                    SetBackgraound();
                }
            }

            return IntPtr.Zero;
        }
        private void SetBackgraound()
        {
            int w = WidthAndHeight.X, h = WidthAndHeight.Y;
            using var bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using var g = Graphics.FromImage(bmp);

            g.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(w, h));

            var data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            try
            {
                Dispatcher.Invoke(() =>
                {
                    if (_wb == null)
                    {
                        _wb = new WriteableBitmap(w, h, g.DpiX, g.DpiY, PixelFormats.Pbgra32, null);
                        Wallpaper.Source = _wb;

                    }
                    _wb.WritePixels(new Int32Rect(0, 0, w, h), data.Scan0, data.Stride * h, data.Stride);

                });
            }
            finally { bmp.UnlockBits(data); }
            this.Visibility = Visibility.Visible;
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



        private void UseEraser_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                EraserPop.IsOpen = !EraserPop.IsOpen;

            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Hide();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DrawCanvas.Width = this.Width;
            DrawCanvas.Height = this.Height;

            this.Hide();
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {

            this.Hide();
           
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as ToggleButton;
            if (btn == this.UsePen)
            {

                if (this.UsePen.IsChecked == false) return;
                this.UseEraser.IsChecked = false;
                this.SelectColor.IsChecked = false;
                this.SelectButton.IsChecked = false;
                this.Highlighter.IsChecked = false;
                this.ReBuildCanvas.IsChecked = false;

                if (UsePen.IsChecked == true)
                {
                    _editmod.IsPen = true;
                    _editmod.IsHighlighter = false;
                    _editmod.IsEraser = false;
                    _editmod.PenColor = _InkCanvasMode.CurrentColor;
                    _editmod.sizeOfBursh = BrushSizeType.Small;
                    _editmod.TypeOfTyping = InkCanvasEditingMode.Ink;
                    
                    _InkCanvasMode.ChangeCanvasBehave(ref this.DrawCanvas, ref _DrawingPen, _editmod);
                }
            }
            else if (btn == this.UseEraser)
            {
                if (this.UseEraser.IsChecked == false) return;
                this.UsePen.IsChecked = false;
                this.SelectColor.IsChecked = false;
                this.Highlighter.IsChecked = false;
                this.SelectButton.IsChecked = false;
                this.ReBuildCanvas.IsChecked = false;
                if (UseEraser.IsChecked == true)
                {
                    _editmod.IsEraser = true;

                    if (EraseByStroke.IsChecked == true)
                        _editmod.EraseBy = InkCanvasEditingMode.EraseByStroke;
                    else _editmod.EraseBy = InkCanvasEditingMode.EraseByPoint;

                    _editmod.IsPen = false;
                    _editmod.IsHighlighter = false;
                    _InkCanvasMode.ChangeCanvasBehave(ref this.DrawCanvas, ref _DrawingPen, _editmod);
                }
            }
            else if (btn == this.SelectColor)
            {
                if (this.SelectColor.IsChecked == false) return;
                this.UseEraser.IsChecked = false;
                this.UsePen.IsChecked = false;
                this.SelectButton.IsChecked = false;
                this.Highlighter.IsChecked = false;
                this.ReBuildCanvas.IsChecked = false;
                if (SelectColor.IsChecked==true)
                if (_InkCanvasMode != null && _InkCanvasMode.ColorPicker != null)
                {
                    try
                    {
                        _InkCanvasMode.ColorPicker.ShowDialog();
                        var ARBG = _InkCanvasMode.ColorPicker.Color;
                        _InkCanvasMode.CurrentColor = System.Windows.Media.Color.FromArgb
                            (ARBG.A, ARBG.R, ARBG.G, ARBG.B);

                        _editmod.IsPen = true;
                        _editmod.IsEraser = false;
                        _editmod.PenColor = _InkCanvasMode.CurrentColor;
                        _editmod.sizeOfBursh = BrushSizeType.Small;
                        _editmod.TypeOfTyping = InkCanvasEditingMode.Ink;
                        _InkCanvasMode.ChangeCanvasBehave(ref this.DrawCanvas, ref _DrawingPen, _editmod);

                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);
                    }
                }

            }
            else if (btn == this.EraseByPoint)
            {
                if (EraseByPoint.IsChecked == false && EraseByStroke.IsChecked == false) EraseByPoint.IsChecked = true;
                EraseByStroke.IsChecked = false;

                if (EraseByPoint.IsChecked == true)
                {
                    _editmod.IsEraser = true;
                    _editmod.IsHighlighter = false;
                    _editmod.EraseBy = InkCanvasEditingMode.EraseByPoint;
                    _editmod.IsPen = false;
                    _editmod.IsHighlighter = false;
                    _InkCanvasMode.ChangeCanvasBehave(ref this.DrawCanvas, ref _DrawingPen, _editmod);
                }
            }
            else if (btn == this.EraseByStroke)
            {
                if (EraseByPoint.IsChecked == false && EraseByStroke.IsChecked == false) EraseByStroke.IsChecked = true;
                EraseByPoint.IsChecked = false;

                if (EraseByStroke.IsChecked == true)
                {

                    _editmod.IsEraser = true;
                    _editmod.EraseBy = InkCanvasEditingMode.EraseByStroke;
                    _editmod.IsPen = false;
                    _editmod.IsHighlighter = false;
                    _InkCanvasMode.ChangeCanvasBehave(ref this.DrawCanvas, ref _DrawingPen, _editmod);

                }
            }
            else if(btn == this.Highlighter)
            {
                if (Highlighter.IsChecked == true)
                {
                    this.UseEraser.IsChecked = false;
                    this.UsePen.IsChecked = false;
                    this.SelectButton.IsChecked = false;
                    this.SelectColor.IsChecked = false;
                    this.ReBuildCanvas.IsChecked = false;
                    _editmod.IsPen = true;
                    _editmod.IsEraser = false;
                    _editmod.IsHighlighter = true;
                    _editmod.PenColor = _InkCanvasMode.CurrentColor;
                    _editmod.sizeOfBursh = BrushSizeType.Large;
                    _editmod.TypeOfTyping = InkCanvasEditingMode.Ink;
                    _InkCanvasMode.ChangeCanvasBehave(ref this.DrawCanvas, ref _DrawingPen, _editmod);
                }
            }
            else if (btn == this.SelectButton)
            {
                this.UseEraser.IsChecked = false;
                this.UsePen.IsChecked = false;
                this.SelectColor.IsChecked = false;
                this.Highlighter.IsChecked = false;
                this.ReBuildCanvas.IsChecked = false;
                _editmod.IsPen = true;
                _editmod.IsEraser = false;
                _editmod.IsHighlighter = true;
                _editmod.PenColor = _InkCanvasMode.CurrentColor;
                _editmod.sizeOfBursh = BrushSizeType.Large;
                _editmod.TypeOfTyping = InkCanvasEditingMode.Select;
                _InkCanvasMode.ChangeCanvasBehave(ref this.DrawCanvas, ref _DrawingPen, _editmod);
            }
            else if(btn == this.ReBuildCanvas)
            {
                this.UseEraser.IsChecked = false;
                this.UsePen.IsChecked = false;
                this.SelectButton.IsChecked = false;
                this.Highlighter.IsChecked = false;
                this.SelectColor.IsChecked = false;

                DrawCanvas.Strokes.Clear();
 
            }
        }
    }
}