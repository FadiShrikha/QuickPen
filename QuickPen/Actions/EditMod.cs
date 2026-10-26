using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
namespace QuickPen
{
    public struct EditMod
    {
        public bool IsPen;
        public bool IsHighlighter;
        public bool IsEraser;
        public InkCanvasEditingMode TypeOfTyping;
        public InkCanvasEditingMode EraseBy;

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

        }
    }


    public enum ErasingTypes
    {
        EraseByPoint, EraseByStroke
    }


    public class SetInkCanvasMode
    {
        public Color CurrentColor;
        Color? LastColor;

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
                Width = 5,
                Height = 5

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

            drawingAttributes.Width = 5;
            drawingAttributes.Height = 5;

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
    }
}
