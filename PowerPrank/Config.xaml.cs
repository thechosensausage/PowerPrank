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
using SettingsProvider;
using GlobalHook;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualBasic;
using WindowScrape.Static;
using System.Windows.Interop;
using Microsoft.Win32;
namespace PowerPrank
{
    /// <summary>
    /// Interaction logic for Config.xaml
    /// </summary>
    public partial class Config : Window
    {

        PrankInfo CurrentPrank;
        CombinationSetter MenuKeySetter = new CombinationSetter(SharedInfo.MenuKeys, "Bring up the menu");
        CombinationSetter UnlockKeySetter = new CombinationSetter(SharedInfo.UnlockKeys, "Unlock screen");
        CombinationSetter CaptureKeySetter = new CombinationSetter(SharedInfo.CaptureKeys, "Capture key (First key only)");
        CombinationSetter CaptureCancelKeySetter = new CombinationSetter(SharedInfo.CaptureCancelKeys, "Cancel capture key (First key only)");
        


        public Config()
        {
            InitializeComponent();
            if (SharedInfo.PrankNames.Length == 0)
            {
                SharedInfo.AddPrank("Default");
            }
            ReloadPrankNames();


            #region KeySetters

            MenuKeySetter.CombinationChanged += (n,e)=>SharedInfo.MenuKeys = e;
            UnlockKeySetter.CombinationChanged += (n, e) => SharedInfo.UnlockKeys = e;
            CaptureKeySetter.CombinationChanged += (n, e) => SharedInfo.CaptureKeys = e;
            CaptureCancelKeySetter.CombinationChanged += (n, e) => SharedInfo.CaptureCancelKeys = e;
            
            


            MenuKeySetter.MessageDisplayed += MessageDisplayed;
            UnlockKeySetter.MessageDisplayed += MessageDisplayed;
            CaptureKeySetter.MessageDisplayed += MessageDisplayed;
            CaptureCancelKeySetter.MessageDisplayed += MessageDisplayed;


            spEditKeys.Children.Add(MenuKeySetter);
            spEditKeys.Children.Add(UnlockKeySetter);
            spEditKeys.Children.Add(CaptureKeySetter);
            spEditKeys.Children.Add(CaptureCancelKeySetter);
            #endregion

            cbbTriggerType.Items.Clear();
            foreach(PrankTrigger trigger in Enum.GetValues(typeof(PrankTrigger)))
            {
                cbbTriggerType.Items.Add(trigger.ToString());
            }

            Helper.ActivateWindow(this);
            Refresh();
            Activate();
        }

        private void MessageDisplayed(object sender, string e)
        {
            lbMessage.Content = e;
        }

        

        private void Refresh()
        {
            

            lbTitle.Content = CurrentPrank.MenuTitle;
            lbPath.Content = CurrentPrank.MenuPath;
            cbbTriggerType.SelectedItem= CurrentPrank.TriggerType.ToString();
                      
            lbPoint1.Content= CurrentPrank.LowPoint.ToString();
            lbPoint2.Content = CurrentPrank.HighPoint.ToString();
            tbBackColor.Text=ColorConverterExtensions.ToHexString(CurrentPrank.BackColor);
            tbShutdownText.Text = CurrentPrank.ShutDownText;
            tbShutdownTime.Text = CurrentPrank.ShutDownTime.ToString();
            
            cbMoveCusor.IsChecked = CurrentPrank.MoveCursor;
            cbSuppressKey.IsChecked = CurrentPrank.SuppressLastKey;
            cbPressStart.IsChecked = CurrentPrank.PressStart;
            cbKillProgram.IsChecked= CurrentPrank.KillProgram;

            cbValidateArea.IsChecked = CurrentPrank.ValidateArea;
            cbValidatePath.IsChecked = CurrentPrank.ValidatePath;
            cbValidateTitle.IsChecked = CurrentPrank.ValidateTitle;

            TriggerKeySetter.SetKeys(CurrentPrank.TriggerKeys);
        }

        

        private void SaveConfig()
        {
            try
            {

                CurrentPrank.MenuTitle = lbTitle.Content.ToString();
                CurrentPrank.MenuPath = lbPath.Content.ToString();
                CurrentPrank.LowPoint = Point.Parse(lbPoint1.Content.ToString());
                CurrentPrank.HighPoint = Point.Parse(lbPoint2.Content.ToString());
                CurrentPrank.TriggerType = (PrankTrigger)Enum.Parse(typeof(PrankTrigger), cbbTriggerType.SelectedItem.ToString());
                try
                { CurrentPrank.BackColor = (Color)ColorConverter.ConvertFromString(tbBackColor.Text); }
                catch { CurrentPrank.BackColor = Helper.GetAccentColor(); }
                CurrentPrank.ShutDownTime = int.Parse(tbShutdownTime.Text);
                CurrentPrank.ShutDownText = tbShutdownText.Text;
                CurrentPrank.MoveCursor = (bool)cbMoveCusor.IsChecked;
                CurrentPrank.PressStart = (bool)cbPressStart.IsChecked;
                CurrentPrank.SuppressLastKey=(bool)cbSuppressKey.IsChecked;
                CurrentPrank.KillProgram = (bool)cbKillProgram.IsChecked;
                CurrentPrank.TriggerKeys = TriggerKeySetter.CurrentKeys;

                CurrentPrank.ValidateArea   = (bool)cbValidateArea.IsChecked ;
                CurrentPrank.ValidatePath   = (bool)cbValidatePath.IsChecked ;
                CurrentPrank.ValidateTitle = (bool)cbValidateTitle.IsChecked;
                GlobalSettings.Save();
                Program.Update();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        Point MousePos=new Point(0,0);
        List<Point> HotArea=new List<Point>() {  };
        bool capturing=false;
        private void btCapture_Click(object sender, RoutedEventArgs e)
        {

            if (capturing) { MessageBox.Show("please finish the current capture operation first."); return; }
            capturing=true;
            MessageBox.Show(String.Format("Make your desired window active(Foreground), and press {0} while your mouse is at one corner of the trigger area. Do it again for the other corner." +
                "\r\n\r\npress {1} to cancel.",SharedInfo.CaptureKeys[0],SharedInfo.CaptureCancelKeys[0]));
            HookManager.KeyDown += Hook_KeyDown;
            HookManager.MouseMove += Hook_OnMouseActivity;
        }

        private void UnSubscribe()
        {
            HookManager.MouseMove -= Hook_OnMouseActivity;
            HookManager.KeyDown -= Hook_KeyDown;
        }
        private void Hook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == SharedInfo.CaptureKeys[0])
            {
                HotArea.Add(MousePos);
                if(HotArea.Count >= 2) {
                    string MenuTitle;
                    string MenuPath;
                    Helper.GetActiveWindow(out MenuTitle, out MenuPath); 
                    lbTitle.Content=MenuTitle; 
                    lbPath.Content=MenuPath;
                    UnSubscribe(); 
                    ApplyPoints(); 
                    SaveConfig(); 
                    HotArea.Clear(); 
                    Helper.ActivateWindow(this); 
                    capturing=false; Refresh(); 
                    MessageBox.Show("Captured"); }
            }
            if (e.KeyCode == SharedInfo.CaptureCancelKeys[0])
            {
                 HotArea.Clear(); capturing = false; Refresh(); UnSubscribe(); Activate(); MessageBox.Show("canceled"); 
            }
        }

        private void ApplyPoints()
        {
            lbPoint1.Content = HotArea[0].ToString();
            lbPoint2.Content = HotArea[1].ToString();
        }

        


        private void Hook_OnMouseActivity(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            MousePos.X = e.X;
            MousePos.Y = e.Y;
        }

        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
            
        }

        private void btExit_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("Stop PowerPrank?","Quit",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Program.Exit();
            }
        }

        private void cbbPrankNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((cbbPrankNames.SelectedItem != null) && SharedInfo.PrankNames.Contains(cbbPrankNames.SelectedItem.ToString()))
            {
                
                string name = cbbPrankNames.SelectedItem.ToString();
                CurrentPrank = new PrankInfo(cbbPrankNames.SelectedItem.ToString());
                Refresh();
                Program.Update();
            }
        }

        private void btAddPrank_Click(object sender, RoutedEventArgs e)
        {
            SharedInfo.AddPrank(Interaction.InputBox("Enter Prank name"));
            ReloadPrankNames();

        }

        private void btRemovePrank_Click(object sender, RoutedEventArgs e)
        {
            SharedInfo.RemovePrank(cbbPrankNames.SelectedItem.ToString());
            ReloadPrankNames();
        }
        void ReloadPrankNames()
        {
            cbbPrankNames.Items.Clear();
            foreach (string s in SharedInfo.PrankNames)
            {
                cbbPrankNames.Items.Add(s);
            }
            try
            {
                cbbPrankNames.SelectedItem = cbbPrankNames.Items[cbbPrankNames.Items.Count - 1];
            }
            catch { }
        }

        void SaveGlobal()
        {
            if ((SharedInfo.MenuKeys.Count() <= 0) && (MessageBox.Show("No key configured to open this menu, are you sure?","Confirmation",MessageBoxButton.OKCancel)==MessageBoxResult.Cancel))
            {
                return;
            }
            GlobalSettings.Save();
        }

        private void btSaveGlobal_Click(object sender, RoutedEventArgs e)
        {
            SaveGlobal();
        }

        private void btReset_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("Reset all settings", "confirm", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) { return; }
            try
            {
                File.Delete(SharedInfo.ConfigPath);
                if (MessageBox.Show("Restart to complete the reset operation.", "confirm", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) { return; }
                else {
                    Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    Environment.Exit(0);
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cbbTriggerType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PrankTrigger trigger = (PrankTrigger)Enum.Parse(typeof(PrankTrigger), cbbTriggerType.SelectedItem.ToString());
            if(trigger==PrankTrigger.KeySequence)
            {
                TriggerKeySetter.IsEnabled = true;
                TriggerKeySetter.AllowDuplicate = true;
                cbValidateArea.IsChecked = false;
            }
            else if (trigger == PrankTrigger.KeyCombination)
            {
                TriggerKeySetter.IsEnabled = true;
                TriggerKeySetter.AllowDuplicate = false;
                cbValidateArea.IsChecked = false;
            }
            else { TriggerKeySetter.IsEnabled = false; }
        }

        private void btRestart_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Environment.Exit(0);
        }

        private void btRenamePrank_Click(object sender, RoutedEventArgs e)
        {
            SharedInfo.RenamePrank(cbbPrankNames.SelectedItem.ToString(), Interaction.InputBox("New name:"));

            Console.WriteLine("Dumps by bt BEFORE reload");
            Console.WriteLine(GlobalSettings.DumpKVPair());
            ReloadPrankNames();

            Console.WriteLine("Dumps by bt AFTER reload");

            Console.WriteLine(GlobalSettings.DumpKVPair());
        }

        private void btExport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.AddExtension = true;
            saveFileDialog.Filter = "PowerPrank config file|*.prank";
            saveFileDialog.DefaultExt = ".prank";
            if (saveFileDialog.ShowDialog() == true) {
                if (GlobalSettings.Export(saveFileDialog.FileName))
                {
                    MessageBox.Show("Your settings have been successfully exported");
                }
                else
                {
                    MessageBox.Show("export failed");
                }
            }
                
        }

        private void btImport_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("keep current settings?", "merge", MessageBoxButton.YesNoCancel);
            bool merge=false;
            if (result == MessageBoxResult.Yes)
            {
                merge = true;
            }
            else if (result == MessageBoxResult.No) { }
            else { return; }
            OpenFileDialog o=new OpenFileDialog();
            o.Filter = "PowerPrank config file|*.prank";
            o.AddExtension = true;
            o.DefaultExt = ".prank";
            if (o.ShowDialog() == true)
            {
                if (GlobalSettings.Import(o.FileName,merge))
                {
                    MessageBox.Show("Your settings have been successfully imported");
                }
                else
                {
                    MessageBox.Show("import failed");
                }
            }
            ReloadPrankNames();
            Program.Update();
        }
    }
}
