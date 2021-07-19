using System;
using System.Drawing;

namespace StopWatch
{
    public static class Theme
    {
        public static bool DarkTheme { get => true; }
        public static Color IssueBackground { get => DarkTheme ? Color.Black : SystemColors.Window; }
        public static Color IssueBackgroundSelected { get => DarkTheme ? Color.FromArgb(13,26,40) : SystemColors.GradientInactiveCaption; }
        public static Color TimeBackground { get => DarkTheme ? Color.FromArgb(15,15,15) : SystemColors.Control; }
        public static Color TimeBackgroundRunning { get => DarkTheme ? Color.FromArgb(4,103,4) : Color.PaleGreen; }
        public static Color ButtonBackground { get => DarkTheme ? Color.FromArgb(30,30,30) : SystemColors.ControlLight; }
        public static Color ButtonBackgroundDisabled { get => DarkTheme ? Color.FromArgb(51,51,51) : Color.FromArgb(204,204,204); }
        public static Color WindowBackground { get => DarkTheme ? Color.Black : SystemColors.Window; }
        public static Color TextBackground { get => DarkTheme ? Color.Black : SystemColors.Window; }
        public static Color Text { get => DarkTheme ? Color.White : SystemColors.WindowText; }
        public static Color Border { get => DarkTheme ? Color.FromArgb(68, 68, 69) : Color.FromArgb(68, 68, 69); }
    }
}
