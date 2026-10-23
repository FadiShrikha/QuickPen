using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QuickPen.Actions
{
    /// <summary>
    /// Takes an instant Screenshot for the screen
    /// </summary>
    public static class FreezFrame
    {
  
        private static (int Width , int Height) GetScreenWidthAndHeight()
        {
            int ScreenWidth = SystemInformation.VirtualScreen.Width;
            int ScreenHeight = SystemInformation.VirtualScreen.Height;
            return (ScreenWidth, ScreenHeight);
        }
        /// <summary>
        /// Create an image form the screen
        /// </summary>
        public static Bitmap GetAScreenShot()
        {
            var SizeOfDisplay = GetScreenWidthAndHeight();
            Size s = new Size { Width=SizeOfDisplay.Width, Height = SizeOfDisplay.Height };

            Bitmap memoryImage = new Bitmap(s.Width, s.Height);
            
            using Graphics memoryGraphics = Graphics.FromImage(memoryImage);
            memoryGraphics.CopyFromScreen(Point.Empty, Point.Empty, s);
            
            memoryGraphics.Flush();

            return memoryImage;
        }

    }
    
}
