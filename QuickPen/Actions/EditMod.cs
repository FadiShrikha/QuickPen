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

    public enum InkCanvasState
    {
        Pen, Eraser, Select, Highlighter, colorchange, CursorMode
    }
    public class SetInkCanvasMode
    {
        public Color CurrentColor;
        Color? LastColor;
        public InkCanvasState? _CurrentCanvasState;

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
                _CurrentCanvasState = InkCanvasState.Pen;
            }
        }
        public void ChangeBrushColor(ref InkCanvas inkCanvas, Color color)
        {
            inkCanvas.DefaultDrawingAttributes.Color = color;

            _CurrentCanvasState = InkCanvasState.colorchange;
        }

        public void ChangeInkCanvasModeToPen(ref InkCanvas inkCanvas)
        {
            inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            inkCanvas.DefaultDrawingAttributes.IsHighlighter = false;

            _CurrentCanvasState = InkCanvasState.Pen;
        }

        public void SetOrRemoveHighlightPen(ref InkCanvas inkCanvas, bool SetHighLightMode)
        {
            inkCanvas.DefaultDrawingAttributes.IsHighlighter = SetHighLightMode;
            inkCanvas.EditingMode = InkCanvasEditingMode.Ink;

            _CurrentCanvasState = InkCanvasState.Highlighter;
        }

        public void ChangeBrushToSelectMode(ref InkCanvas inkCanvas)
        {
            inkCanvas.EditingMode = InkCanvasEditingMode.Select;

            _CurrentCanvasState = InkCanvasState.Select;
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
            _CurrentCanvasState = InkCanvasState.Eraser;
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

        public void SetLastState(ref InkCanvas inkCanvas)
        {
            if (_CurrentCanvasState != null)
            {
                switch (_CurrentCanvasState)
                {
                    case InkCanvasState.Pen:
                        {
                            ChangeInkCanvasModeToPen(ref inkCanvas);
                        }
                        break;
                    case InkCanvasState.Highlighter:
                        {
                            SetOrRemoveHighlightPen(ref inkCanvas, true);
                        }
                        break;
                    case InkCanvasState.Select:
                        {
                            ChangeBrushToSelectMode(ref inkCanvas);
                        }
                        break;
                    case InkCanvasState.Eraser:
                        {
                            ChangeInkCanvasToEraserMode(ref inkCanvas, ErasingTypes.EraseByStroke);
                        }
                        break;
                    case InkCanvasState.CursorMode:
                        {
                            inkCanvas.EditingMode = InkCanvasEditingMode.None;
                        }
                        break;
                }
            }
        }
        public void Undo(ref InkCanvas inkCanvas, StrokeCollection strokes)
        {

            foreach(var S in strokes)
            {
               
                if (inkCanvas.Strokes.Any(f => f == S))
                {
                    inkCanvas.Strokes.Remove(S);
                }
            }
        }
        public void Redo(ref InkCanvas inkCanvas, StrokeCollection strokes)
        {

            foreach (var S in strokes)
            {
                if (inkCanvas.Strokes.All(f => f != S))
                {
                    inkCanvas.Strokes.Add(strokes);
                }
            }
        }
    }
}
