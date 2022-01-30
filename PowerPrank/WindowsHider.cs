using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HWND = System.IntPtr;

namespace dwm
{
    class WindowsHider
    {
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        [DllImport("User32")]
        private static extern int ShowWindow(int hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();


        public static IntPtr GetActiveWindow()
        {
            IntPtr handle = IntPtr.Zero;
            return GetForegroundWindow();
        }
        static List<IntPtr> HiddenWindows=new List<IntPtr>() {  };
        public static void  hideWindow(string processName)
        {
            Process[] processRunning = Process.GetProcessesByName(processName);
            foreach (Process pr in processRunning)
            {
                try
                {
                    if((pr.MainWindowHandle != IntPtr.Zero)&&pr.SessionId==Process.GetCurrentProcess().SessionId)
                    {
                        ShowWindow(pr.MainWindowHandle.ToInt32(), SW_HIDE);
                      
                        HiddenWindows.Add(pr.MainWindowHandle);
                    }
                    
                }
                catch
                {

                }
            }
        }
        public static void hideWindow(Process process)
        {
            try
            {
                if ((process.MainWindowHandle != IntPtr.Zero) && process.SessionId == Process.GetCurrentProcess().SessionId)
                {
                    ShowWindow(process.MainWindowHandle.ToInt32(), SW_HIDE);

                    HiddenWindows.Add(process.MainWindowHandle);
                }

            }
            catch
            {

            }
        }
        public static void hideWindow(IntPtr handle)
        {
            try
            {
                ShowWindow(handle.ToInt32(), SW_HIDE);
                HiddenWindows.Add(handle);

            }
            catch
            {

            }
        }
        public static void hideAll()
        {
            Process[] processRunning = Process.GetProcesses();
            foreach (Process pr in processRunning)
            {
                hideWindow(pr.ProcessName);
            }
        }
        public static void HideActiveWindow()
        {
            try
            {
                HWND hwnd = GetActiveWindow();
                ShowWindow(hwnd.ToInt32(), SW_HIDE);
                HiddenWindows.Add(hwnd);
            }
            catch { }
        }
        public static void RestoreAll()
        {
            foreach( IntPtr intPtr in HiddenWindows.ToArray())
            {
                try
                {

                    ShowWindow(intPtr.ToInt32(), SW_SHOW);
                }
                catch { }
            }
            HiddenWindows.Clear();
        }
    }

    public static class OpenWindowGetter
    {
        /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
        /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
        public static IDictionary<HWND, string> GetOpenWindows()
        {
            HWND shellWindow = GetShellWindow();
            Dictionary<HWND, string> windows = new Dictionary<HWND, string>();

            EnumWindows(delegate (HWND hWnd, int lParam)
            {
                if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;

                StringBuilder builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);

                windows[hWnd] = builder.ToString();
                return true;

            }, 0);

            return windows;
        }

        private delegate bool EnumWindowsProc(HWND hWnd, int lParam);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(HWND hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();
    }
}
