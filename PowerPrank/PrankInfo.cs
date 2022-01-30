using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SettingsProvider;
using Windows.UI.ViewManagement;
using System.Windows.Media;
using System.Windows.Forms;
namespace PowerPrank
{
    public enum PrankTrigger
    {
        MouseClickLeft,
        MouseClickRight,
        MouseClickBoth,
        KeyCombination,
        KeySequence
    }
    public class PrankInfo
    {
        public string PrankName;

        public delegate void PrankUpdate();
        public event PrankUpdate PrankUpdated;
        public PrankInfo(string identifier)
        {
            PrankName = "Pranks." + identifier;
            GlobalSettings.AddDefault(PrankName + ".BackColor", ColorConverterExtensions.ToHexString(Helper.GetAccentColor()));
            GlobalSettings.AddDefault(PrankName + ".ShutDownText", "Shutting Down");
            GlobalSettings.AddDefault(PrankName + ".ShutDownTimeSec", "10");
            GlobalSettings.AddDefault(PrankName + ".MoveCursor", "True");
            GlobalSettings.AddDefault(PrankName + ".PressStart", "True");
            GlobalSettings.AddDefault(PrankName + ".MenuTitle", "Undefined");
            GlobalSettings.AddDefault(PrankName + ".MenuPath", "Undefined");
            GlobalSettings.AddDefault(PrankName + ".KillProgram", "false");
            GlobalSettings.AddDefault(PrankName + ".SuppressLastKey", "True");
            GlobalSettings.AddDefault(PrankName + ".TriggerType", PrankTrigger.MouseClickBoth.ToString());
            GlobalSettings.AddDefault(PrankName + ".ShowFakeScreen", "True");
            GlobalSettings.AddDefault(PrankName + ".ValidateArea", "True");
            GlobalSettings.AddDefault(PrankName + ".ValidateTitle", "True");
            GlobalSettings.AddDefault(PrankName + ".ValidatePath", "True");
            GlobalSettings.FillDefault();
            GlobalSettings.Defaults.Clear();
        }
        public Point LowPoint
        {
            get
            {
                try { return new Point(int.Parse(GlobalSettings.Get(PrankName + ".LowX")), int.Parse(GlobalSettings.Get(PrankName + ".LowY"))); }
                catch { return new Point(0, 0); }
            }
            set {

                GlobalSettings.Set(PrankName + ".LowX", value.X);
                GlobalSettings.Set(PrankName + ".LowY", value.Y);
                PrankUpdated?.Invoke();

            }
        }
        public Point HighPoint
        {
            get
            {
                try { return new Point(int.Parse(GlobalSettings.Get(PrankName + ".HighX")), int.Parse(GlobalSettings.Get(PrankName + ".HighY"))); }
                catch { return new Point(0, 0); }
            }
            set
            {

                GlobalSettings.Set(PrankName + ".HighX", value.X);
                GlobalSettings.Set(PrankName + ".HighY", value.Y);
                PrankUpdated?.Invoke();

            }
        }
        public string MenuTitle
        {
            get {
                return GlobalSettings.Get(PrankName + ".MenuTitle");
            }
            set
            {
                GlobalSettings.Set(PrankName + ".MenuTitle", value);
                PrankUpdated?.Invoke();

            }
        }
        public Color BackColor
        {
            get
            {
                try
                {
                    return (Color)ColorConverter.ConvertFromString(GlobalSettings.Get(PrankName + ".BackColor"));
                }
                catch { }
                return Helper.GetAccentColor();
            }
            set { GlobalSettings.Set(PrankName + ".BackColor", Helper.ColorToHex(value)); PrankUpdated?.Invoke();
            }

        }
        public string MenuPath
        {
            get
            {
                return GlobalSettings.Get(PrankName + ".MenuPath");
            }
            set
            {
                GlobalSettings.Set(PrankName + ".MenuPath", value);
                PrankUpdated?.Invoke();

            }
        }
        public int ShutDownTime
        {
            get
            {
                try
                {
                    return int.Parse(GlobalSettings.Get(PrankName + ".ShutDownTimeSec"));
                }
                catch { return 10; }
            }
            set
            {
                GlobalSettings.Set(PrankName + ".ShutDownTimeSec", value.ToString());
                PrankUpdated?.Invoke();

            }
        }

        public bool MoveCursor
        {
            get
            {
                try
                {
                    return bool.Parse(GlobalSettings.Get(PrankName + ".MoveCursor"));
                }
                catch
                {
                    return true;
                }
            }
            set { GlobalSettings.Set(PrankName + ".MoveCursor", value); PrankUpdated?.Invoke(); }
        }
        public bool PressStart
        {
            get
            {
                try
                {
                    return bool.Parse(GlobalSettings.Get(PrankName + ".PressStart"));
                }
                catch
                {
                    return true;
                }
            }
            set { GlobalSettings.Set(PrankName + ".PressStart", value); PrankUpdated?.Invoke(); }
        }
        public bool KillProgram
        {
            get
            {
                try
                {
                    return bool.Parse(GlobalSettings.Get(PrankName + ".KillProgram"));
                }
                catch
                {
                    return true;
                }
            }
            set { GlobalSettings.Set(PrankName + ".KillProgram", value); PrankUpdated?.Invoke(); }
        }

        public bool ShowFakeScreen
        {
            get
            {
                try
                {
                    return bool.Parse(GlobalSettings.Get(PrankName + ".ShowFakeScreen"));
                }
                catch
                {
                    return true;
                }
            }
            set { GlobalSettings.Set(PrankName + ".ShowFakeScreen", value); PrankUpdated?.Invoke(); }
        }

        public bool SuppressLastKey
        {
            get
            {
                try
                {
                    return bool.Parse(GlobalSettings.Get(PrankName + ".SuppressLastKey"));
                }
                catch
                {
                    return true;
                }
            }
            set { GlobalSettings.Set(PrankName + ".SuppressLastKey", value); PrankUpdated?.Invoke(); }
        }

        public bool ValidateArea
        {
            get
            {
                try
                {
                    return bool.Parse(GlobalSettings.Get(PrankName + ".ValidateArea"));
                }
                catch
                {
                    return true;
                }
            }
            set { GlobalSettings.Set(PrankName + ".ValidateArea", value); PrankUpdated?.Invoke(); }
        }
        public bool ValidateTitle
        {
            get
            {
                try
                {
                    return bool.Parse(GlobalSettings.Get(PrankName + ".ValidateTitle"));
                }
                catch
                {
                    return true;
                }
            }
            set { GlobalSettings.Set(PrankName + ".ValidateTitle", value); PrankUpdated?.Invoke(); }
        }
        public bool ValidatePath
        {
            get
            {
                try
                {
                    return bool.Parse(GlobalSettings.Get(PrankName + ".ValidatePath"));
                }
                catch
                {
                    return true;
                }
            }
            set { GlobalSettings.Set(PrankName + ".ValidatePath", value); PrankUpdated?.Invoke(); }
        }
        public string ShutDownText
        {
            get { return GlobalSettings.Get(PrankName + ".ShutDownText"); }
            set { GlobalSettings.Set(PrankName + ".ShutDownText", value); PrankUpdated?.Invoke();
            }
        }
        public PrankTrigger TriggerType
        {
            get
            {
                try { return (PrankTrigger)Enum.Parse(typeof(PrankTrigger), GlobalSettings.Get(PrankName + ".TriggerType")); }
                catch { return PrankTrigger.MouseClickBoth; }
            }
            set
            {
                GlobalSettings.Set(PrankName + ".TriggerType", value.ToString());
                
            }
        }

        public Keys[] TriggerKeys
        {
            get
            {
                return Helper.StringsToKeys(GlobalSettings.GetValues(PrankName + ".TriggerKeys"));
            }
            set
            {

                GlobalSettings.SetValues(PrankName + ".TriggerKeys", Helper.KeysToStrings(value));
            }
        }
    }
}
