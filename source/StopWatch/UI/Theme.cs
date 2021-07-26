using System;
using System.Drawing;
using System.Drawing.Text;
using Microsoft.Win32;

namespace StopWatch
{
    public static class Theme
    {
        public static Color IssueBackground { get => DarkTheme ? Color.Black : SystemColors.Window; }
        public static Color IssueBackgroundSelected { get => DarkTheme ? Color.FromArgb(13,26,40) : SystemColors.GradientInactiveCaption; }
        public static Color TimeBackground { get => DarkTheme ? Color.FromArgb(15,15,15) : SystemColors.Control; }
        public static Color TimeBackgroundRunning { get => DarkTheme ? Color.FromArgb(4,50,4) : Color.PaleGreen; }
        public static Color ButtonBackground { get => Color.Transparent; } // DarkTheme ? Color.FromArgb(30,30,30) : SystemColors.ControlLight; }
        public static Color ButtonBackgroundDisabled { get => Color.Transparent; } // DarkTheme ? Color.FromArgb(51,51,51) : Color.FromArgb(204,204,204); }
        public static Color WindowBackground { get => DarkTheme ? Color.Black : SystemColors.Window; }
        public static Color ModalBackground {  get => DarkTheme ? Color.FromArgb(15, 15, 15) : SystemColors.Control; }
        public static Color TextBackground { get => DarkTheme ? Color.Black : SystemColors.Window; }
        public static Color Text { get => DarkTheme ? Color.White : SystemColors.WindowText; }
        public static Color TextMuted { get => DarkTheme ? Color.LightGray : Color.FromArgb(68, 68, 69); }
        public static Color Border { get => DarkTheme ? Color.FromArgb(68, 68, 69) : Color.DarkGray; }
        public static Color Blue { get => Color.FromArgb(0, 172, 193); }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
            IntPtr pdv, [System.Runtime.InteropServices.In] ref uint pcFonts);

        private static FontFamily _regularFont;
        private static FontFamily _boldFont;
        public static FontFamily RegularFont
        {
            get
            {
                if (_regularFont == null) _regularFont = LoadFontFromResource(Properties.Resources.Rubik_Regular);
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
                    //*
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
    }

    public enum WindowsTheme
    {
        Light,
        Dark
    }
}
