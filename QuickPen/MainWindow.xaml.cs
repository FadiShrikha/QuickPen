using QuickPen.Actions;
using System.Diagnostics;
using System.Drawing.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QuickPen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GlobalHotKey HideShowWindow;
        GlobalHotKey ClearAllHotkey;
        GlobalHotKey RedoHotKey;
        GlobalHotKey UndoHotKey;

        private DrawingAttributes _DrawingPen = SetInkCanvasMode.GetDefualtAttributes();
        private SetInkCanvasMode _InkCanvasMode = new();
        private LetAppRunAtStartup _runatstartup = new();
        public System.Windows.Media.Color _CurrentColor { get; set; }
        System.Drawing.Point WidthAndHeight;

        List<StrokeCollection> _AddedStrokes = new();
        List<StrokeCollection> _RemovedStrokes = new();
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
            ClearAllHotkey = new GlobalHotKey(Keys.C, Keys.Alt, handle, TypeOfHotKeyRegister.ClearAll);
            HideShowWindow = new GlobalHotKey(Keys.Q, Keys.Alt, handle, TypeOfHotKeyRegister.HideAndShow);
            RedoHotKey = new GlobalHotKey(Keys.Z, Keys.Alt, handle, TypeOfHotKeyRegister.Redo);
            UndoHotKey = new GlobalHotKey(Keys.X, Keys.Alt, handle, TypeOfHotKeyRegister.Undo);
            ClearAllHotkey.register();
            HideShowWindow.register();
            UndoHotKey.register();
            RedoHotKey.register();

        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // is this a HotKey ?

            var MSG = (WindowMessage)msg;
            if (MSG == WindowMessage.WM_HotKey)
            {
                var id = wParam.ToInt32();
                var TypeHotkey = (TypeOfHotKeyRegister)id;
                if (TypeHotkey == TypeOfHotKeyRegister.HideAndShow)
                {
                    if (this.IsVisible)
                    {
                        this.BrushSizeSliderPopup.IsOpen = false;
                        this.ChangeColorSliderPop.IsOpen = false;
                        this.Hide();
                    }
                    else
                    {
                        this.Show();
                    }
                }
                if (TypeHotkey == TypeOfHotKeyRegister.ClearAll)
                {
                    _InkCanvasMode.ResetInkCanvas(ref DrawCanvas);
                }
                if (TypeHotkey == TypeOfHotKeyRegister.Redo)
                {
                    ManageRedo();
                }
                if (TypeHotkey == TypeOfHotKeyRegister.Undo)
                {
                    ManageUndo();
                }

            }

            return IntPtr.Zero;
        }


        private void Btn_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var Btn = sender as ToggleButton;
            if (Btn == this.UsePen || Btn == this.Highlighter)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    this.BrushSizeSliderPopup.IsOpen = !BrushSizeSliderPopup.IsOpen;

                }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DrawCanvas.Width = this.Width;
            DrawCanvas.Height = this.Height;
            Wallpaper.Width = this.Width;
            Wallpaper.Height = this.Height;

            this.BrushSizeSliderPopup.IsOpen = false;
            this.ChangeColorSliderPop.IsOpen = false;




            this.Wallpaper.Opacity = 0;
            DrawCanvas.Strokes.StrokesChanged += Strokes_StrokesChanged;
            SetCurrentColor();
            BackToCursorState();

        }
        private void Window_Closed(object sender, EventArgs e)
        {
            if (HideShowWindow != null)
            {
                HideShowWindow.Unregister();
            }
            if (UndoHotKey != null)
            {
                UndoHotKey.Unregister();
            }
            if (RedoHotKey != null)
            {
                RedoHotKey.Unregister();
            }
            if (ClearAllHotkey != null)
            {
                ClearAllHotkey.Unregister();
            }
        }

        private void Strokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {

            if (e.Added.Count > 0)
            {
                _AddedStrokes.Add(e.Added);
            }
            if (e.Removed.Count > 0)
            {
                _RemovedStrokes.Add(e.Removed);
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            this.BrushSizeSliderPopup.IsOpen = false;
            this.ChangeColorSliderPop.IsOpen = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            var btn = sender as ToggleButton;

            if (btn?.IsChecked == true)
            {
                this.UseEraser.IsChecked = false;
                this.SelectButton.IsChecked = false;
                this.Highlighter.IsChecked = false;
                this.UsePen.IsChecked = false;
                this.BackToCursor.IsChecked = false;
            }

            btn?.IsChecked = true;

            if (btn != this.BackToCursor)
            {
                BackToDrawState();
            }
            else BackToCursorState();

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



        }
        private void BackToCursorState()
        {

            this.BrushSizeSliderPopup.IsOpen = false;
            this.ChangeColorSliderPop.IsOpen = false;

            this.UseEraser.IsChecked = false;
            this.SelectButton.IsChecked = false;
            this.Highlighter.IsChecked = false;
            this.UsePen.IsChecked = false;
            this.BackToCursor.IsChecked = true;


            DrawCanvas.EditingMode = InkCanvasEditingMode.None;
            _InkCanvasMode._CurrentCanvasState = InkCanvasState.CursorMode;

            this.Wallpaper.Opacity = 0;


        }

        private void BackToDrawState()
        {
            this.Wallpaper.Opacity = 1;

        }

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

                if (this.ChangeColorSliderPop.IsMouseOver == false && this.ChangeColorSliderPop.IsOpen == true)
                {
                    this.ChangeColorSliderPop.IsOpen = false;
                }
            }
        }
        private void ManageUndo()
        {
            if (DrawCanvas.Strokes != null && _RemovedStrokes.Count > 0)
            {

                DrawCanvas.Strokes.Add(_RemovedStrokes.Last());

                _RemovedStrokes.RemoveAt(_RemovedStrokes.Count - 1);
            }
        }
        private void ManageRedo()
        {

            if (DrawCanvas.Strokes != null && _AddedStrokes.Count > 0)
            {

                var strokes = _AddedStrokes.Last();

                if (DrawCanvas.Strokes != null)
                {
                    _InkCanvasMode.Undo(ref DrawCanvas, strokes);
                    _AddedStrokes.RemoveAt(_AddedStrokes.Count - 1);
                }

            }
        }
        private void ManageResetCanva()
        {

            _InkCanvasMode.ResetInkCanvas(ref DrawCanvas);
        }
        private void ReDrawStroke_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            if (btn == this.ReDrawStroke)
            {
                ManageRedo();
            }
            else if (btn == this.UnDoLastStroke)
            {
                ManageUndo();
            }
            else if (btn == this.ReBuildCanvas)
            {
                ManageResetCanva();

            }
            else if (btn == this.SelectColor)
            {
                if (_InkCanvasMode != null)
                {
                    try
                    {
                        this.ChangeColorSliderPop.IsOpen = true;
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);
                    }
                }

            }

        }


        private void MainCanvasDrop(object sender, System.Windows.DragEventArgs e)
        {
            var location = e.GetPosition(MainCanvas);

            var deltaX = location.X - _grabeOffset.X;
            var deltaY = location.Y - _grabeOffset.Y;

            Canvas.SetLeft(ItemsBorder, deltaX );
            Canvas.SetTop(ItemsBorder, deltaY );

            if (_InkCanvasMode._CurrentCanvasState == InkCanvasState.CursorMode && BackToCursor.IsChecked == true)
            {
                BackToCursorState();
            }

            else
            {
                BackToDrawState();
            }
        }


        System.Windows.Point _grabeOffset;
        private void DragButton_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (_InkCanvasMode._CurrentCanvasState == InkCanvasState.CursorMode && BackToCursor.IsChecked==true)
                {
                    BackToDrawState();
                }
              
                _grabeOffset = e.GetPosition(ItemsBorder);
                 DragDrop.DoDragDrop(sender as Border, sender, System.Windows.DragDropEffects.All);
            }
        }

    }
}