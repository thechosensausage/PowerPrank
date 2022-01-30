using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobalHook;
using System.Windows.Forms;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;
using WindowsInput.Native;
using WindowsInput;
using SettingsProvider;
using WindowScrape;
using WindowScrape.Static;

namespace PowerPrank
{
    internal class Program
    {
        static InputSimulator sim = new InputSimulator();
        public static System.Windows.Application ConfigApp { get; private set; }
        public static System.Windows.Application WinApp { get; private set; }
        public static Window fakeScreen { get; private set; }
        public static bool ShutdownConfirmed = false;
        public static PrankInfo[] LoadedPranks;
        static CombinationHook Unlocker;
        static CombinationHook ShowConfig;
        static List<CombinationHook> ComboHooks=new List<CombinationHook>() { };
        static List<SequenceHook> SequenceHooks = new List<SequenceHook>() {};

        [STAThread]
        static void Main(string[] args)
        {
            SettingsDebug.DebugMessageReceived += (n, e) => Console.WriteLine("Settings Error:" + e);

            GlobalSettings.AddDefaultValues("MenuKeys", Helper.KeysToStrings(new Keys[] { Keys.F3,Keys.LMenu,Keys.S}));
            GlobalSettings.AddDefaultValues("UnlockKeys", Helper.KeysToStrings(new Keys[] { Keys.F3, Keys.LMenu, Keys.U }));
            GlobalSettings.AddDefaultValues("CaptureKeys", Helper.KeysToStrings(new Keys[] { Keys.F7 }));
            GlobalSettings.AddDefaultValues("CaptureCancelKeys", Helper.KeysToStrings(new Keys[] { Keys.F6 }));


            GlobalSettings.Load(SharedInfo.ConfigPath);
            SharedInfo.PrankUpdated += SharedInfo_PrankUpdated;
            SharedInfo.KeysChanged += SharedInfo_KeysChanged;
            HookManager.KeyDown += Intercept;
            HookManager.MouseClickExt += HookManager_MouseClickExt;

            Update();

            fakeScreen = new FakeScreen();
            fakeScreen.Visibility = Visibility.Hidden;
            WinApp = new System.Windows.Application();
            WinApp.Run(fakeScreen); // note: blocking call
        }

        private static void Intercept(object sender, KeyEventArgs e)
        {
            if (ShutdownConfirmed)
            {
                e.SuppressKeyPress = true;
            }
        }
        #region Config Updated
        private static void SharedInfo_KeysChanged(object sender, EventArgs e)
        {
            if(Unlocker!= null) { Unlocker.Unhook(); }
            Unlocker = new CombinationHook(SharedInfo.UnlockKeys);
            Unlocker.Triggered += Unlocker_Triggered;

            if (ShowConfig != null) { ShowConfig.Unhook(); }
            ShowConfig = new CombinationHook(SharedInfo.MenuKeys);
            ShowConfig.Triggered += () => ((FakeScreen)fakeScreen).ShowWindow(new Config());
            
        }

        public static void SharedInfo_PrankUpdated()
        {
            Update();
        }
        public static void SharedInfo_PrankInfoUpdated()
        {
            Update();
        }

        public static void Update()
        {
            LoadedPranks = SharedInfo.Pranks.ToArray();
            SharedInfo_KeysChanged(null, null);


            foreach(CombinationHook hook in ComboHooks)
            {
                hook.Unhook();
            }
            foreach (SequenceHook hook in SequenceHooks)
            {
                hook.Unhook();
            }
            ComboHooks.Clear();
            SequenceHooks.Clear();

            foreach(PrankInfo prankInfo in LoadedPranks.ToArray())
            {
                if (prankInfo.TriggerType == PrankTrigger.KeyCombination)
                {
                    var hook = new CombinationHook(prankInfo.TriggerKeys);
                    hook.Triggered += () => KeyTriggered(prankInfo, hook);
                    ComboHooks.Add(hook);
                }
                else if (prankInfo.TriggerType == PrankTrigger.KeySequence)
                {
                    var hook = new SequenceHook(prankInfo.TriggerKeys);
                    hook.Triggered += () => KeyTriggered(prankInfo,sequenceHook:hook);
                    SequenceHooks.Add(hook);
                }
            }
        }

        private static void KeyTriggered(PrankInfo prankInfo,CombinationHook CombHook=null,SequenceHook sequenceHook=null)
        {
            
            if (ValidateConditions(prankInfo,new Point(2, 2)))
            {
                
                if (CombHook != null) { CombHook.SuppressOnce=prankInfo.SuppressLastKey; }
                if(sequenceHook != null) { sequenceHook.SuppressOnce = prankInfo.SuppressLastKey; }
                StartPrank(prankInfo);
            }
        }
        #endregion
        #region Hooks
        #endregion
        private static void HookManager_MouseClickExt(object sender, MouseEventExtArgs e)
        {
            Point mousePoint = new Point(e.X, e.Y);
            if (e.Clicks > 0)
            {
                foreach (PrankInfo prankInfo in LoadedPranks)
                {
                    if((prankInfo.TriggerType == PrankTrigger.MouseClickLeft) && (e.Button == MouseButtons.Left) && ValidateConditions(prankInfo,mousePoint))
                    {
                        StartPrank(prankInfo);
                    }
                    if ((prankInfo.TriggerType == PrankTrigger.MouseClickRight) && (e.Button == MouseButtons.Right) && ValidateConditions(prankInfo, mousePoint))
                    {
                        StartPrank(prankInfo);
                    }
                    if ((prankInfo.TriggerType == PrankTrigger.MouseClickBoth) && ValidateConditions(prankInfo, mousePoint))
                    {
                        StartPrank(prankInfo);
                    }

                }
            }
        }


        static bool ValidateConditions(PrankInfo prankInfo, Point MousePos)
        {
            string title, path;
            Helper.GetActiveWindow(out title, out path);
            bool TitleCorrect = (title == prankInfo.MenuTitle);
            bool PathCorrect= (path.ToLower() == prankInfo.MenuPath.ToLower());
            bool MouseInRange = (TestRange((int)MousePos.X, (int)prankInfo.LowPoint.X, (int)prankInfo.HighPoint.X) && TestRange((int)MousePos.Y, (int)prankInfo.LowPoint.Y, (int)prankInfo.HighPoint.Y));
            
            
            if ((!TitleCorrect) && prankInfo.ValidateTitle)      {
                return false; }
            if((!PathCorrect) && prankInfo.ValidatePath) {
                return false; }
            if ((!MouseInRange)&& prankInfo.ValidateArea)  {
                return false; }
            return true;
        }

        private static void Unlocker_Triggered()
        {
            Unlock();
        }


        static bool TestRange(int numberToCheck, int bottom, int top)
        {
            if (bottom > top) { int t = bottom;bottom = top; top=t;}
            return ((numberToCheck > bottom) && (numberToCheck < top));
        }

        
        static void StartPrank(PrankInfo prank)
        {

            
            if (prank.PressStart)
            {
                try { sim.Keyboard.KeyPress(VirtualKeyCode.LWIN); } catch { }
            }
            if (prank.MoveCursor)
            {
                try { sim.Mouse.MoveMouseTo(10, 10); } catch { };
            }
            ShutdownConfirmed = true;



            
            ((FakeScreen)fakeScreen).Begin(prank);
            Update();
        }
        static void Unlock()
        {
            ((FakeScreen)fakeScreen).End();
            Update();
            ShutdownConfirmed = false;
        }
        public static void Exit()
        {
            
            Environment.Exit(0);
        }


        
    }
}
