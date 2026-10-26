using QuickPen.Actions;
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
        private LetAppRunAtStartup _runatstartup = new();
        public System.Windows.Media.Color _CurrentColor { get; set; }
        System.Drawing.Point WidthAndHeight;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            SourceInitialized += MainWindow_SourceInitialized;

            _runatstartup.RegisterApp();

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
                    EraserPop.IsOpen = false;
                    this.BrushSizeSliderPopup.IsOpen = false;
                    this.ChangeColorSliderPop.IsOpen = false;
                    this.Hide();
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




        private void Btn_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var Btn = sender as ToggleButton;
            if (Btn == this.UseEraser)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    EraserPop.IsOpen = !EraserPop.IsOpen;

                }
            }
            else if (Btn == this.UsePen)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    this.BrushSizeSliderPopup.IsOpen = !BrushSizeSliderPopup.IsOpen;

                }
            }



        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Enter || e.Key == Key.Enter || e.Key == Key.LWin || e.Key == Key.RWin)
            {
                this.Hide();

            }
            if (this.Visibility == Visibility.Hidden)
            {
                EraserPop.IsOpen = false;
                this.BrushSizeSliderPopup.IsOpen = false;
                this.ChangeColorSliderPop.IsOpen = false;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DrawCanvas.Width = this.Width;
            DrawCanvas.Height = this.Height;
            EraserPop.IsOpen = false;
            this.BrushSizeSliderPopup.IsOpen = false;
            this.ChangeColorSliderPop.IsOpen = false;
            SetCurrentColor();
            this.Hide();

        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {

            this.Hide();
            EraserPop.IsOpen = false;
            this.BrushSizeSliderPopup.IsOpen = false;
            Bitmap bitmap = new(30, 30);
            using Graphics g = Graphics.FromImage(bitmap);


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            var btn = sender as ToggleButton;
            if (btn?.IsChecked == true)
            {
                this.UseEraser.IsChecked = false;
                this.SelectColor.IsChecked = false;
                this.SelectButton.IsChecked = false;
                this.Highlighter.IsChecked = false;
                this.ReBuildCanvas.IsChecked = false;
                this.UsePen.IsChecked = false;
            }

            btn?.IsChecked = true;

            if (btn == this.UsePen)
            {
                if (UsePen.IsChecked == true)
                {
                    _InkCanvasMode.ChangeInkCanvasModeToPen(ref DrawCanvas);

                }
            }
            else if (btn == this.UseEraser)
            {

                if (UseEraser.IsChecked == true)
                {
                    if (EraseByPoint.IsChecked == true)
                    {
                        _InkCanvasMode.ChangeInkCanvasToEraserMode(ref DrawCanvas, ErasingTypes.EraseByPoint);
                    }
                    else
                    {
                        _InkCanvasMode.ChangeInkCanvasToEraserMode(ref DrawCanvas, ErasingTypes.EraseByStroke);
                    }
                }
            }
            else if (btn == this.SelectColor)
            {
                if (SelectColor.IsChecked == true)
                    if (_InkCanvasMode != null)
                    {
                        try
                        {
                            this.ChangeColorSliderPop.IsOpen = true;
                            _InkCanvasMode.ChangeInkCanvasModeToPen(ref DrawCanvas);



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
                    _InkCanvasMode.ChangeInkCanvasToEraserMode(ref DrawCanvas, ErasingTypes.EraseByPoint);
                }
            }
            else if (btn == this.EraseByStroke)
            {
                if (EraseByPoint.IsChecked == false && EraseByStroke.IsChecked == false) EraseByStroke.IsChecked = true;
                EraseByPoint.IsChecked = false;

                if (EraseByStroke.IsChecked == true)
                {
                    _InkCanvasMode.ChangeInkCanvasToEraserMode(ref DrawCanvas, ErasingTypes.EraseByStroke);

                }
            }
            else if (btn == this.Highlighter)
            {
                if (Highlighter.IsChecked == true)
                {

                    if (Highlighter.IsChecked == true)
                    {
                        _InkCanvasMode.SetOrRemoveHighlightPen(ref DrawCanvas, true);
                    }
                    else
                    {
                        _InkCanvasMode.SetOrRemoveHighlightPen(ref DrawCanvas, false);
                    }

                }
            }
            else if (btn == this.SelectButton)
            {

                if (SelectButton.IsChecked == true)
                {
                    _InkCanvasMode.ChangeBrushToSelectMode(ref DrawCanvas);
                }

            }
            else if (btn == this.ReBuildCanvas)
            {

                if (ReBuildCanvas.IsChecked == true)
                {
                    _InkCanvasMode.ResetInkCanvas(ref DrawCanvas);
                    _InkCanvasMode.ChangeInkCanvasModeToPen(ref DrawCanvas);
                }

            }

        }
        StrokeCollection _DeletedStrokesContainer = new();
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            if (slider == this.BrushSizeSlider)
            {
                slider.Value = (int)slider.Value;
                _InkCanvasMode.ChangeBrushSize(ref DrawCanvas, (int)slider.Value);

            }
            else if (slider == this.AllColorsSlider)
            {
                if (RgbColorsImage != null)
                {
                    SetCurrentColor();
                }
            }

        }
        private void SetCurrentColor()
        {
            BitmapSource RGBImage = (BitmapSource)this.RgbColorsImage.Source;

            _CurrentColor = _InkCanvasMode.GetColorFromSlider((int)AllColorsSlider.Value, RGBImage);

            if (DrawCanvas != null)
                _InkCanvasMode.ChangeBrushColor(ref DrawCanvas, _CurrentColor);

            this.Dispatcher.Invoke(() =>
            {
                if (CurrentColorBorder != null)
                    this.CurrentColorBorder.Background = new SolidColorBrush(_CurrentColor);
            });
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.BrushSizeSliderPopup.IsMouseOver == false && this.BrushSizeSliderPopup.IsOpen == true)
                {
                    this.BrushSizeSliderPopup.IsOpen = false;
                }
                if (this.EraserPop.IsMouseOver == false && this.EraserPop.IsOpen == true)
                {
                    this.EraserPop.IsOpen = false;
                }
                if (this.ChangeColorSliderPop.IsMouseOver == false && this.ChangeColorSliderPop.IsOpen == true)
                {
                    this.ChangeColorSliderPop.IsOpen = false;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (Q_Alt_HotKey != null)
            {
                Q_Alt_HotKey.Unregister();
            }
        }


        private void DrawCanvas_StrokeErasing(object sender, InkCanvasStrokeErasingEventArgs e)
        {
            if (_DeletedStrokesContainer != null)
            {
                if (_DeletedStrokesContainer.All(f => f != e.Stroke))
                {
                    _DeletedStrokesContainer.Add(e.Stroke);
                }
            }
        }

        private void ReDrawStroke_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            if (btn == this.ReDrawStroke)
            {
                if (_DeletedStrokesContainer != null && _DeletedStrokesContainer.Count > 0)
                {
                    try
                    {
                        var LastStroke = _DeletedStrokesContainer.LastOrDefault();

                        if (LastStroke != null)
                        {
                            _InkCanvasMode.ReAddLastStroke(ref DrawCanvas, LastStroke);

                            _DeletedStrokesContainer.RemoveAt(_DeletedStrokesContainer.IndexOf(LastStroke));
                        }
                    }
                    finally
                    {
                        
                    }
                }
            }
            if (btn == this.UnDoLastStroke)
            {
                if(DrawCanvas.Strokes!=null && DrawCanvas.Strokes.Count > 0)
                {
                    
                    var Last = DrawCanvas.Strokes.Last();
                    if ( _DeletedStrokesContainer!=null&&_DeletedStrokesContainer.All(f => f != Last))
                    {
                        _InkCanvasMode.DeleteLastStroke(ref DrawCanvas, ref _DeletedStrokesContainer);
                    }
                    
                    
                }
            }
        }
    }
}