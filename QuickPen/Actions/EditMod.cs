using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using Size = System.Windows.Size;
namespace QuickPen
{
    public struct EditMod
    {
        public bool IsPen;
        public bool IsHighlighter;
        public bool IsEraser;
        public InkCanvasEditingMode TypeOfTyping;
        public InkCanvasEditingMode EraseBy;
        public BrushSizeType sizeOfBursh;
        public BrushSize SetBrushSize;
        public System.Windows.Media.Color PenColor;
        public System.Windows.Media.Color? LastColor;

        public EditMod()
        {
            IsPen = true;
            IsHighlighter = false;
            IsEraser = false;
            EraseBy = InkCanvasEditingMode.EraseByStroke;
            PenColor = Color.FromRgb(255, 255, 255);
            LastColor = null;

            sizeOfBursh = BrushSizeType.Small;
            SetBrushSize = new(sizeOfBursh);
        }
    }

    public struct BrushSize
    {
        public Size Small;
        public Size Medium;
        public Size Large;
        public Size VeryLarge;
        public Size Defualtbrushsize;
        public BrushSize(BrushSizeType brushSizeType)
        {
            Small = new(5, 5);
            Medium = new(10, 10);
            Large = new(20, 20);
            VeryLarge = new(40, 40);
            if (brushSizeType == BrushSizeType.Small) Defualtbrushsize = Small;
            if (brushSizeType == BrushSizeType.Medium) Defualtbrushsize = Medium;
            if (brushSizeType == BrushSizeType.Large) Defualtbrushsize = Large;
            if (brushSizeType == BrushSizeType.VeryLarge) Defualtbrushsize = VeryLarge;

        }
    }
    public enum BrushSizeType
    {
        Small, Medium, Large, VeryLarge
    }
    public enum ErasingTypes
    {
        EraseByPoint, EraseByStroke
    }

    public struct RGB
    {
        public float R, G, B;
        public RGB(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }
    }

    public class SetInkCanvasMode
    {
        public Color CurrentColor;
        Color? LastColor;


        private BrushSize _brushsize = new(BrushSizeType.Medium);

        public SetInkCanvasMode()
        {
            CurrentColor = Color.FromRgb(255, 255, 255);
        }
        public static DrawingAttributes GetDefualtAttributes()
        {
            EditMod editMod = new();

            DrawingAttributes DefualtAtrribute = new()
            {
                Color = editMod.PenColor,
                IsHighlighter = editMod.IsHighlighter,
                Width = editMod.SetBrushSize.Defualtbrushsize.Width,
                Height = editMod.SetBrushSize.Defualtbrushsize.Height

            };

            return DefualtAtrribute;
        }

        public void SetNewBrushAttribute(ref DrawingAttributes drawingAttributes, EditMod editMod)
        {

            if (editMod.IsPen)
            {
                drawingAttributes.Color = editMod.PenColor;
            }

            drawingAttributes.IsHighlighter = editMod.IsHighlighter;

            if (editMod.sizeOfBursh == BrushSizeType.Small)
            {
                drawingAttributes.Width = editMod.SetBrushSize.Small.Width;
                drawingAttributes.Height = editMod.SetBrushSize.Small.Height;
            }
            if (editMod.sizeOfBursh == BrushSizeType.Medium)
            {
                drawingAttributes.Width = editMod.SetBrushSize.Medium.Width;
                drawingAttributes.Height = editMod.SetBrushSize.Medium.Height;
            }
            if (editMod.sizeOfBursh == BrushSizeType.Large)
            {
                drawingAttributes.Width = editMod.SetBrushSize.Large.Width;
                drawingAttributes.Height = editMod.SetBrushSize.Large.Height;
            }
            if (editMod.sizeOfBursh == BrushSizeType.VeryLarge)
            {
                drawingAttributes.Width = editMod.SetBrushSize.VeryLarge.Width;
                drawingAttributes.Height = editMod.SetBrushSize.VeryLarge.Height;
            }

        }
        public void ChangeCanvasBehave(ref InkCanvas inkCanvas, ref DrawingAttributes drawingAttributes, EditMod editMod)
        {
            SetNewBrushAttribute(ref drawingAttributes, editMod);

            if (editMod.IsEraser)
            {
                inkCanvas.EditingMode = editMod.EraseBy;
            }

            else
            {
                inkCanvas.DefaultDrawingAttributes = drawingAttributes;
                inkCanvas.EditingMode = editMod.TypeOfTyping;
            }
        }
        public void ChangeBrushColor(ref InkCanvas inkCanvas, Color color)
        {
            inkCanvas.DefaultDrawingAttributes.Color = color;
        }

        public void ChangeInkCanvasModeToPen(ref InkCanvas inkCanvas)
        {
            inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            inkCanvas.DefaultDrawingAttributes.IsHighlighter = false;
        }

        public void SetOrRemoveHighlightPen(ref InkCanvas inkCanvas, bool SetHighLightMode)
        {
            inkCanvas.DefaultDrawingAttributes.IsHighlighter = SetHighLightMode;
            inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
        }

        public void ChangeBrushToSelectMode(ref InkCanvas inkCanvas)
        {
            inkCanvas.EditingMode = InkCanvasEditingMode.Select;
        }

        public void ResetInkCanvas(ref InkCanvas inkCanvas)
        {
            inkCanvas.Strokes.Clear();
        }
        public void ChangeInkCanvasToEraserMode(ref InkCanvas inkCanvas, ErasingTypes type)
        {
            if (type == ErasingTypes.EraseByPoint)
            {
                inkCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
            }
            else
            {
                inkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
            }
        }
        public void ChangeBrushSize(ref InkCanvas inkCanvas, int Size)
        {
            try
            {
                if (Size <= 0)
                {
                    inkCanvas.DefaultDrawingAttributes.Height = 1;
                    inkCanvas.DefaultDrawingAttributes.Width = 1;
                }
                else if (Size >= 100)
                {
                    inkCanvas.DefaultDrawingAttributes.Height = 99;
                    inkCanvas.DefaultDrawingAttributes.Width = 99;
                }
                inkCanvas.DefaultDrawingAttributes.Height = Size;
                inkCanvas.DefaultDrawingAttributes.Width = Size;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + " value was : " + Size);
            }
        }


        public System.Windows.Media.Color GetColorFromSlider(int value, BitmapSource bitmapImage)
        {
            byte[] _ColorBuffer = new byte[4];
            Int32Rect rect = new(value, 10, 1, 1);
            bitmapImage.CopyPixels(rect, _ColorBuffer, 4, 0);
            var color = System.Windows.Media.Color.FromArgb(_ColorBuffer[3], _ColorBuffer[2], _ColorBuffer[1], _ColorBuffer[0]);



            return color;
        }

        public void ReAddLastStroke(ref InkCanvas inkCanvas,Stroke stroke )
        {
            if (inkCanvas.Strokes.All(f => f != stroke))
            {
                
                inkCanvas.Strokes.Add(stroke);
            }
        }
        public void DeleteLastStroke(ref InkCanvas inkCanvas ,ref StrokeCollection strokes)
        {
            var last = inkCanvas.Strokes.LastOrDefault();
            if (last != null)
            {
                inkCanvas.Strokes.RemoveAt(inkCanvas.Strokes.IndexOf(last));
                strokes.Add(last);
            }
        }
    }
}
