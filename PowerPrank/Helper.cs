using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using Windows.UI.ViewManagement;
using System.Windows.Media;
using System.Windows.Forms;
using WindowScrape.Static;
namespace PowerPrank
{
    static  class Helper
    {
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);
        public static IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

        const long WS_EX_TOPMOST = 0x00000008L;

        public enum GWL : int
        {
            GWL_WNDPROC = (-4),
            GWL_HINSTANCE = (-6),
            GWL_HWNDPARENT = (-8),
            GWL_STYLE = (-16),
            GWL_EXSTYLE = (-20),
            GWL_USERDATA = (-21),
            GWL_ID = (-12)
        }

        public static void SetToolWindow(Window window)
        {
            var wih = new WindowInteropHelper(window);
            var style = GetWindowLongPtr(wih.Handle, (int)GWL.GWL_EXSTYLE);
            style = new IntPtr(style.ToInt64() | WS_EX_TOPMOST);
            SetWindowLongPtr(new HandleRef(wih, wih.Handle), (int)GWL.GWL_EXSTYLE, style);
        }
        public static bool IsMenuOpen()
        {
            return false;
        }
        
        public static System.Windows.Media.Color GetAccentColor()
        {
            var uiSettings = new UISettings();
            var accentColor = uiSettings.GetColorValue(UIColorType.Accent);
            return System.Windows.Media.Color.FromArgb(accentColor.A, accentColor.R, accentColor.G, accentColor.B);
        }
        static public string GetActiveWindow(out string title,out string location)
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                uint pid;
                GetWindowThreadProcessId(handle, out pid);
                Process p = Process.GetProcessById((int)pid);
                string name;
                try
                {
                    name = Path.GetFileName(p.MainModule.FileName);

                }
                catch (Exception ex)
                {
                    name = "unable to show file name";
                }
                try
                {
                    location = p.MainModule.FileName;
                }
                catch (Exception ex)
                {
                    location = "unable to show file path";
                }
                title = Buff.ToString();
                return name + "---" + Buff.ToString();
            }
            else
            {

                location = "";
                title = "";
                return "null window";
            }
        }
        public static string ColorToHex(this Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        public static string ColorToRgb(this Color c) => $"RGB({c.R}, {c.G}, {c.B})";
        public static Keys[] StringsToKeys(string[] KeyStrs)
        {
            List<Keys> keys = new List<Keys>() { };
            foreach (string s in KeyStrs) { keys.Add((Keys)Enum.Parse(typeof(Keys), s)); }
            return keys.ToArray();
        }
        public static Keys[] ButtonsToKeys(System.Windows.Controls.UIElementCollection bts)
        {
            List<Keys> keys = new List<Keys>() { };
            foreach (object bt in bts) { keys.Add((Keys)Enum.Parse(typeof(Keys), ((System.Windows.Controls.Button)bt).Content.ToString())); }
            return keys.ToArray();
        }
        public static string[] KeysToStrings(Keys[] keys)
        {
            List<string> keystrings = new List<string>() { };
            foreach (Keys key in keys)
            {
                keystrings.Add(key.ToString());
            }
            return keystrings.ToArray();
        }

        public static System.Windows.Controls.Button[] KeysToButtons(Keys[] keys)
        {
            List<System.Windows.Controls.Button> buts = new List<System.Windows.Controls.Button>() { };
            foreach (Keys key in keys)
            {
                System.Windows.Controls.Button button = new System.Windows.Controls.Button();
                button.Content = key.ToString();
                button.Padding=new Thickness(5,0,5,0);
                buts.Add(button);
            }
            return buts.ToArray();
        }
        public static void ActivateWindow(Window window)
        {
            HwndInterface.ActivateWindow(new WindowInteropHelper(window).Handle);
        }
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref Point lpPoint);
        
    }
}
