using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using Microsoft.Win32;

namespace StopWatch
{
    public static class Theme
    {
        public static Color Primary { get => Color.FromArgb(0, 172, 193); }
        public static Color WindowBackground { get => DarkTheme ? Color.FromArgb(35, 40, 49) : SystemColors.Window; }
        public static Color TextBackground { get => WindowBackground; }
        public static Color IssueBackgroundSelected { get => DarkTheme ? Color.FromArgb(13,26,40) : SystemColors.GradientInactiveCaption; }
        public static Color TimeBackgroundRunning { get => DarkTheme ? Color.FromArgb(4,50,4) : Color.PaleGreen; }
        public static Color ButtonBackground { get => Color.Transparent; } // DarkTheme ? Color.FromArgb(30,30,30) : SystemColors.ControlLight; }
        public static Color ButtonBackgroundDisabled { get => Color.Transparent; } // DarkTheme ? Color.FromArgb(51,51,51) : Color.FromArgb(204,204,204); }
        public static Color Text { get => DarkTheme ? Color.White : SystemColors.WindowText; }
        public static Color TextMuted { get => DarkTheme ? Color.LightGray : Color.FromArgb(68, 68, 69); }
        public static Color Border { get => DarkTheme ? Color.FromArgb(68, 68, 69) : Color.DarkGray; }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
            IntPtr pdv, [System.Runtime.InteropServices.In] ref uint pcFonts);

        private static FontFamily _regularFont;
        private static FontFamily _boldFont;
        public static FontFamily RegularFont
        {
            get
            {
                if (_regularFont == null) _regularFont = LoadFontFromResource(Properties.Resources.Rubik_Medium);
                return _regularFont;
            }
        }

        public static FontFamily BoldFont
        {
            get
            {
                if (_boldFont == null) _boldFont = LoadFontFromResource(Properties.Resources.Rubik_ExtraBold);
                return _boldFont;
            }
        }

        private static FontFamily LoadFontFromResource(byte[] fontData)
        {
            PrivateFontCollection fonts = new PrivateFontCollection();
            IntPtr fontPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(fontData.Length);
            System.Runtime.InteropServices.Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
            uint dummy = 0;
            fonts.AddMemoryFont(fontPtr, fontData.Length);
            AddFontMemResourceEx(fontPtr, (uint)fontData.Length, IntPtr.Zero, ref dummy);
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(fontPtr);

            return fonts.Families[0];
        }

        private static WindowsTheme? windowsTheme { get; set; }
        public static bool DarkTheme
        {
            get
            {
                if (windowsTheme.HasValue) return windowsTheme == WindowsTheme.Dark;
                else
                {
                    /*
                    return false;
                    /*/
                    windowsTheme = GetWindowsTheme();
                    return windowsTheme == WindowsTheme.Dark;
                    //*/
                }
            }
        }

        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string RegistryValueName = "AppsUseLightTheme";

        // https://engy.us/blog/2018/10/20/dark-theme-in-wpf/#:~:text=windowstheme%20getwindowstheme()
        private static WindowsTheme GetWindowsTheme()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
            {
                object registryValueObject = key?.GetValue(RegistryValueName);
                if (registryValueObject == null)
                {
                    return WindowsTheme.Light;
                }
             
                int registryValue = (int)registryValueObject;
                
                return registryValue > 0 ? WindowsTheme.Light : WindowsTheme.Dark;
            }
        }
        public static Image ColorReplace(this Image inputImage, int tolerance, Color oldColor, Color NewColor)
        {
            Bitmap outputImage = new Bitmap(inputImage.Width, inputImage.Height, inputImage.PixelFormat);
            Graphics G = Graphics.FromImage(outputImage);
            G.DrawImage(inputImage, 0, 0, inputImage.Width, inputImage.Height);
            for (Int32 y = 0; y < outputImage.Height; y++)
                for (Int32 x = 0; x < outputImage.Width; x++)
                {
                    Color PixelColor = outputImage.GetPixel(x, y);
                    if (PixelColor.R > oldColor.R - tolerance && PixelColor.R < oldColor.R + tolerance && PixelColor.G > oldColor.G - tolerance && PixelColor.G < oldColor.G + tolerance && PixelColor.B > oldColor.B - tolerance && PixelColor.B < oldColor.B + tolerance)
                    {
                        int RColorDiff = oldColor.R - PixelColor.R;
                        int GColorDiff = oldColor.G - PixelColor.G;
                        int BColorDiff = oldColor.B - PixelColor.B;

                        if (PixelColor.R > oldColor.R) RColorDiff = NewColor.R + RColorDiff;
                        else RColorDiff = NewColor.R - RColorDiff;
                        if (RColorDiff > 255) RColorDiff = 255;
                        if (RColorDiff < 0) RColorDiff = 0;
                        if (PixelColor.G > oldColor.G) GColorDiff = NewColor.G + GColorDiff;
                        else GColorDiff = NewColor.G - GColorDiff;
                        if (GColorDiff > 255) GColorDiff = 255;
                        if (GColorDiff < 0) GColorDiff = 0;
                        if (PixelColor.B > oldColor.B) BColorDiff = NewColor.B + BColorDiff;
                        else BColorDiff = NewColor.B - BColorDiff;
                        if (BColorDiff > 255) BColorDiff = 255;
                        if (BColorDiff < 0) BColorDiff = 0;

                        outputImage.SetPixel(x, y, Color.FromArgb(PixelColor.A, RColorDiff, GColorDiff, BColorDiff));
                    }
                }
            return outputImage;
        }
    }

    public enum WindowsTheme
    {
        Light,
        Dark
    }
}
