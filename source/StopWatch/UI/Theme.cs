using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using Microsoft.Win32;

namespace StopWatch
{
    public static class Theme
    {
        #region colours
        private static Color? _primary;
        public static Color Primary { get => _primary.HasValue ? _primary.Value : (_primary = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["primarycolor"])).Value; }

        private static Color? _windowBackground;
        public static Color WindowBackground { get => _windowBackground.HasValue ? _windowBackground.Value : (_windowBackground = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["background" + DarkString])).Value; }
        public static Color TextBackground { get => WindowBackground; }

        private static Color? _issueBackgroundSelected;
        public static Color IssueBackgroundSelected { get => _issueBackgroundSelected.HasValue ? _issueBackgroundSelected.Value : (_issueBackgroundSelected = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["issuebackgroundselected" + DarkString])).Value; }

        private static Color? _timeBackgroundRunning;
        public static Color TimeBackgroundRunning { get => _timeBackgroundRunning.HasValue ? _timeBackgroundRunning.Value : (_timeBackgroundRunning = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["timebackgroundrunning" + DarkString])).Value; }

        private static Color? _text;
        public static Color Text { get => _text.HasValue ? _text.Value : (_text = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["text" + DarkString])).Value; }

        private static Color? _textMuted;
        public static Color TextMuted { get => _textMuted.HasValue ? _textMuted.Value : (_textMuted = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["textmuted" + DarkString])).Value; }

        private static Color? _border;
        public static Color Border { get => _border.HasValue ? _border.Value : (_border = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["border" + DarkString])).Value; }

        public static Color ButtonBackground { get => Color.Transparent; } // DarkTheme ? Color.FromArgb(30,30,30) : SystemColors.ControlLight; }
        public static Color ButtonBackgroundDisabled { get => Color.Transparent; } // DarkTheme ? Color.FromArgb(51,51,51) : Color.FromArgb(204,204,204); }
        #endregion

        #region fonts
        private static FontFamily _regularFont;
        public static FontFamily RegularFont
        {
            get
            {
                if (_regularFont == null) _regularFont = LoadFontFromResource(Properties.Resources.Rubik_Medium);
                return _regularFont;
            }
        }

        private static FontFamily _boldFont;
        public static FontFamily BoldFont
        {
            get
            {
                if (_boldFont == null) _boldFont = LoadFontFromResource(Properties.Resources.Rubik_ExtraBold);
                return _boldFont;
            }
        }
        #endregion

        #region images
        private static Color _blue = Color.FromArgb(0, 172, 193);
        private static Image _reset;
        public static Image imgReset {  get => _reset != null ? _reset : _reset = Theme.ColorReplace(Properties.Resources.reset24, 10, _blue, Theme.Primary); }
        
        private static Image _postTime;
        public static Image imgPostTime { get => _postTime != null ? _postTime : _postTime = Theme.ColorReplace(Properties.Resources.posttime26, 10, _blue, Theme.Primary); }

        private static Image _postTimeNote;
        public static Image imgPostTimeNote { get => _postTimeNote != null ? _postTimeNote : _postTimeNote = Theme.ColorReplace(Properties.Resources.posttimenote26, 10, _blue, Theme.Primary); }

        private static Image _play;
        public static Image imgPlay { get => _play != null ? _play : _play = Theme.ColorReplace(Properties.Resources.play26, 10, _blue, Theme.Primary); }

        private static Image _pause;
        public static Image imgPause { get => _pause != null ? _pause : _pause = Properties.Resources.pause26; }

        private static Image _openBrowser;
        public static Image imgOpenBrowser { get => _openBrowser != null ? _openBrowser : _openBrowser = Theme.ColorReplace(Properties.Resources.openbrowser26, 10, _blue, Theme.Primary); }

        private static Image _delete;
        public static Image imgDelete { get => _delete != null ? _delete : _delete = Theme.ColorReplace(Properties.Resources.delete24, 10, _blue, Theme.Primary); }

        private static Image _spinner;
        public static Image imgSpinner { get => _spinner != null ? _spinner : _spinner = DarkTheme ? Properties.Resources.spinner_dark : Properties.Resources.spinner_light; }

        #endregion

        #region helper functions
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
            IntPtr pdv, [System.Runtime.InteropServices.In] ref uint pcFonts);

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
                    string theme = ConfigurationManager.AppSettings["theme"];
                    if(string.Equals(theme, "dark", StringComparison.InvariantCultureIgnoreCase))
                        windowsTheme = WindowsTheme.Dark;
                    else if(string.Equals(theme, "light", StringComparison.InvariantCultureIgnoreCase))
                        windowsTheme = WindowsTheme.Light;
                    else
                        windowsTheme = GetWindowsTheme();

                    return windowsTheme == WindowsTheme.Dark;
                }
            }
        }
        public static string DarkString { get { return DarkTheme ? "dark" : string.Empty; } }

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
        #endregion
    }

    public enum WindowsTheme
    {
        Light,
        Dark
    }
}
