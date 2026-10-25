using System.Windows.Controls;
using System.Windows.Ink;
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
        public Size brushsize;
        public BrushSize(BrushSizeType brushSizeType)
        {
            Small = new(10,10);
            Medium = new(20, 20);
            Large = new(40, 40);
            VeryLarge = new(60, 60);
            if (brushSizeType == BrushSizeType.Small) brushsize = Small;
            if (brushSizeType == BrushSizeType.Medium) brushsize = Medium;
            if (brushSizeType == BrushSizeType.Large) brushsize = Large;
            if (brushSizeType == BrushSizeType.VeryLarge) brushsize = VeryLarge;

        }
    }
    public enum  BrushSizeType
    {
        Small,Medium,Large,VeryLarge
    }
    public enum ErasingTypes
    {
        EraseByPoint,EraseByStroke
    }


    public class SetInkCanvasMode
    {
        public Color CurrentColor;
        Color? LastColor;
        public ColorDialog ColorPicker;
        public Haley.WPF.Controls.ColorPickerButton _ColorPicker;
        private BrushSize _brushsize =new();

        public SetInkCanvasMode()
        {
            _ColorPicker = new() { Width = 40, Height = 40};
            
            ColorPicker = new() {  FullOpen=false,AllowFullOpen=false, ShowHelp=false , AnyColor=false, SolidColorOnly=true};
            ColorPicker.FullOpen = false;
            CurrentColor = Color.FromRgb(255, 255, 255);
        }
        public static DrawingAttributes GetDefualtAttributes()
        {
            EditMod editMod = new();

            DrawingAttributes DefualtAtrribute = new()
            {  Color=editMod.PenColor, IsHighlighter=editMod.IsHighlighter,Width=editMod.SetBrushSize.brushsize.Width, Height=editMod.SetBrushSize.brushsize.Height
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
        public void ChangeCanvasBehave(ref InkCanvas inkCanvas ,ref DrawingAttributes drawingAttributes, EditMod editMod)
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
        public void ChangeBrushColor(ref InkCanvas inkCanvas,Color color)
        {
            inkCanvas.DefaultDrawingAttributes.Color = color;
        }

        public void SetOrRemoveHighlightPen(ref InkCanvas inkCanvas, bool SetHighLightMode)
        {
            inkCanvas.DefaultDrawingAttributes.IsHighlighter = SetHighLightMode;
        }

        public void ChangeBrushToSelectMode(ref InkCanvas inkCanvas)
        {
            inkCanvas.EditingMode =InkCanvasEditingMode.Select;
        }

        public void ResetInkCanvas(ref InkCanvas inkCanvas)
        {
            inkCanvas.Strokes.Clear();
        }
        public void ChangeInkCanvasToEraserMode(ref InkCanvas inkCanvas ,ErasingTypes type)
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
        public void ChangeBrushSize(ref InkCanvas inkCanvas, BrushSizeType Size)
        {
            switch (Size)
            {
                case (BrushSizeType.Small):
                    {
                        inkCanvas.DefaultDrawingAttributes.Width =_brushsize.Small.Width;
                        inkCanvas.DefaultDrawingAttributes.Height = _brushsize.Small.Height;

                    }

                    break;
                case (BrushSizeType.Medium):
                    {
                        inkCanvas.DefaultDrawingAttributes.Width = _brushsize.Medium.Width;
                        inkCanvas.DefaultDrawingAttributes.Height = _brushsize.Medium.Height;
                    }
                    break;
                case (BrushSizeType.Large):
                    {
                        inkCanvas.DefaultDrawingAttributes.Width = _brushsize.Large.Width;
                        inkCanvas.DefaultDrawingAttributes.Height = _brushsize.Large.Height;
                    }
                    break;
                case (BrushSizeType.VeryLarge):
                    {
                        inkCanvas.DefaultDrawingAttributes.Width = _brushsize.VeryLarge.Width;
                        inkCanvas.DefaultDrawingAttributes.Height = _brushsize.VeryLarge.Height;
                    }
                    break;
            }
            
        }
    }
}
