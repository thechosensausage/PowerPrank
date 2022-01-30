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
    internal class SharedInfo
    {
        public static event EventHandler KeysChanged;
        public delegate void PrankUpdate();
        public static event PrankUpdate PrankUpdated;
        public static PrankInfo[] Pranks
        {
            get { 
                List<PrankInfo> list = new List<PrankInfo>() { };
                foreach (string PrankName in GlobalSettings.GetValues("PrankNames"))
                {
                    PrankInfo p = new PrankInfo(PrankName);
                    p.PrankUpdated += Program.SharedInfo_PrankInfoUpdated;
                    list.Add(p);
                }
                return list.ToArray();
            }
        }
        public static string[] PrankNames
        {
            get
            {
                return GlobalSettings.GetValues("PrankNames");
                
            }
        }
        public static void AddPrank(string Name)
        {
            if (Name == "") { return; }
            List<string> pranks = GlobalSettings.GetValues("PrankNames").ToList();
            pranks.Add(Name);
            GlobalSettings.SetValues("PrankNames", pranks.ToArray());
            PrankUpdated?.Invoke();
            
        }
        public static void  RemovePrank(string Name)
        {

            if (Name == "") { return; }
            List<string> pranks = GlobalSettings.GetValues("PrankNames").ToList();
            pranks.Remove(Name);
            GlobalSettings.SetValues("PrankNames", pranks.ToArray());
            GlobalSettings.Remove("Pranks." + Name);
            PrankUpdated?.Invoke();

        }
        public static void RenamePrank(string oldName,string newName)
        {
            GlobalSettings.Rename("Pranks." + oldName, "Pranks." + newName);
            List<string> pranks = GlobalSettings.GetValues("PrankNames").ToList();
            pranks.Remove(oldName);
            pranks.Add(newName);
            GlobalSettings.SetValues("PrankNames", pranks.ToArray());

            Console.WriteLine("Dumps by RenamePrank");
            Console.WriteLine(GlobalSettings.DumpKVPair());
        }

        public static Keys[] MenuKeys
        {
            get
            {
                return Helper.StringsToKeys(GlobalSettings.GetValues("MenuKeys"));
            }
            set
            {
                
                GlobalSettings.SetValues("MenuKeys", Helper.KeysToStrings(value));
                KeysChanged?.Invoke(null,new EventArgs());
            }
        }
        public static Keys[] UnlockKeys
        {
            get
            {
                return Helper.StringsToKeys(GlobalSettings.GetValues("UnlockKeys"));
            }
            set
            {

                GlobalSettings.SetValues("UnlockKeys", Helper.KeysToStrings(value));

                KeysChanged?.Invoke(null, new EventArgs());
            }
        }
        public static Keys[] CaptureKeys
        {
            get
            {
                return Helper.StringsToKeys(GlobalSettings.GetValues("CaptureKeys"));
            }
            set
            {

                GlobalSettings.SetValues("CaptureKeys", Helper.KeysToStrings(value));

            }
        }
        public static Keys[] CaptureCancelKeys
        {
            get
            {
                return Helper.StringsToKeys(GlobalSettings.GetValues("CaptureCancelKeys"));
            }
            set
            {

                GlobalSettings.SetValues("CaptureCancelKeys", Helper.KeysToStrings(value));

            }
        }

        public static string ConfigPath = Environment.ExpandEnvironmentVariables(@"%appdata%\PowerPrank\PowerPrank.ini");
        public static Point CursorPostition { get; set; }
    }
    public static class ColorConverterExtensions
    {
        public static string ToHexString(this Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        public static string ToRgbString(this Color c) => $"RGB({c.R}, {c.G}, {c.B})";
    }
}
