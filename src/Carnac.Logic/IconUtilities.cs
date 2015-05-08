using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Carnac.Logic
{
    internal static class IconUtilities
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        private static readonly Dictionary<string, ImageSource> icons = new Dictionary<string, ImageSource>();


        private static Icon GetProcessIcon(string processFileName)
        {
            Icon icon = Icon.ExtractAssociatedIcon(processFileName);
            return icon;
        }
       
        private static ImageSource IconToImageSource(Icon icon)
        {
            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new Win32Exception();
            }

            return wpfBitmap;
        }

        public static ImageSource GetProcessIconAsImageSource(string processFileName)
        {
            if (icons.ContainsKey(processFileName))
            {
                return icons[processFileName];
            }
            else
            {
                Icon icon = GetProcessIcon(processFileName);
                ImageSource image = IconToImageSource(icon);
                icons.Add(processFileName, image);
                return image;
            }
        }
    }
}