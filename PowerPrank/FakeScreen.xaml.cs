using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Interop;
using System.Threading;
using WindowsInput.Native;
using WindowsInput;
namespace PowerPrank
{
    /// <summary>
    /// Interaction logic for FakeScreen.xaml
    /// </summary>
    public partial class FakeScreen : Window
    {
        InputSimulator sim = new InputSimulator();
        public FakeScreen()
        {
            InitializeComponent();

        }

        

        private void OnClosing(object sender, CancelEventArgs e)
        {

            e.Cancel = true;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            BringIntoView();
            Topmost = true;
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!Topmost) { Topmost=true; }
            
        }

        private void ProgressRing_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Activated(object sender, EventArgs e)
        {
            
        }
        public void Begin(PrankInfo prank)
        {
            if (Program.ShutdownConfirmed)
            {
                Mouse.OverrideCursor = Cursors.None;

                

                Thread t2 = new Thread(() =>
                {
                    
                    while (Program.ShutdownConfirmed)
                    {

                        try { Helper.ActivateWindow(this); } catch { }
                        try { dwm.WindowsHider.hideWindow("explorer"); } catch { }
                        if (prank.KillProgram) {
                            foreach (Process p in Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(prank.MenuPath)))
                            {
                                if (p.SessionId == Process.GetCurrentProcess().SessionId)
                                {
                                    try { p.Kill(); } catch { }
                                }
                            }
                        }
                        foreach (Process p in Process.GetProcessesByName("explorer"))
                        {
                            if (p.SessionId == Process.GetCurrentProcess().SessionId)
                            {
                                try { p.Kill(); } catch { }
                            }
                        }
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            Topmost=true;

                        }));
                        Thread.Sleep(100);
                    }
                });
                t2.Start();
                Show(); BringIntoView(); WindowState = WindowState.Maximized;
                
                Ring.IsActive = true;
                Helper.SetToolWindow(this);
                Background = new SolidColorBrush(prank.BackColor);
                lbShutdownText.Content = prank.ShutDownText;
                Loader.Visibility = Visibility.Visible;
                Thread t = new Thread(() =>
                {
                    
                    Thread.Sleep(prank.ShutDownTime*1000);
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        Background = new SolidColorBrush(Colors.Black);
                        Loader.Visibility = Visibility.Hidden;

                    }));

                });
                t.Start();
            }
        }
        public void End()
        {

            Ring.IsActive = false;
            dwm.WindowsHider.RestoreAll();

            IntPtr windowHandle = new WindowInteropHelper(this).Handle;
            dwm.WindowsHider.hideWindow(windowHandle);
            Process.Start("cmd.exe", "/c start explorer");
            Hide();
        }
        public void ShowWindow(Window window)
        {
            window.Show();
        }
    }
}
