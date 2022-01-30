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
using GlobalHook;
namespace PowerPrank
{
    /// <summary>
    /// Interaction logic for CombinationSetter.xaml
    /// </summary>
    public partial class CombinationSetter : UserControl
    {
        public event EventHandler<System.Windows.Forms.Keys[]> CombinationChanged;
        public event EventHandler<string> MessageDisplayed;
        public System.Windows.Forms.Keys[] CurrentKeys=new System.Windows.Forms.Keys[] { };
        public bool AllowDuplicate=false;
        public CombinationSetter()
        {
            InitializeComponent();
            lbTitle.Content = "Set Keys";
        }
        public CombinationSetter(System.Windows.Forms.Keys[] OldKeys=null,string Title="Set Keys")
        {
            InitializeComponent();
            if (OldKeys == null) { OldKeys = new System.Windows.Forms.Keys[] { }; }
            CurrentKeys = OldKeys;
            lbTitle.Content = Title;
            ReloadMenuKeys();
        }
        public void SetKeys(System.Windows.Forms.Keys[] keys)
        {
            CurrentKeys = keys;
            ReloadMenuKeys();
        }

        private void btAddMenuKey_Click(object sender, RoutedEventArgs e)
        {
            btAddMenuKey.IsEnabled = false;
            MessageDisplayed?.Invoke(null, "Press desired key");
            HookManager.KeyDown += AddMenuKey_Keydown;
        }

        private void AddMenuKey_Keydown(object sender, System.Windows.Forms.KeyEventArgs e)
        {

            List<System.Windows.Forms.Keys> tmpKeys = CurrentKeys.ToList();
            if (AllowDuplicate)
            {
                tmpKeys.Add(e.KeyCode);
            }
            else
            {
                if (!tmpKeys.Contains(e.KeyCode))
                {
                    tmpKeys.Add(e.KeyCode);
                }
            }
            CurrentKeys = tmpKeys.ToArray();
            CombinationChanged?.Invoke(null,CurrentKeys);
            ReloadMenuKeys();
            HookManager.KeyDown -= AddMenuKey_Keydown;
            btAddMenuKey.IsEnabled = true;
            MessageDisplayed?.Invoke(null, "Key added");
        }

        void ReloadMenuKeys()
        {
            spMenuKeys.Children.Clear();
            foreach (Button bt in Helper.KeysToButtons(CurrentKeys))
            {
                spMenuKeys.Children.Add(bt);
            }
        }

        private void btClearMenuKey_Click(object sender, RoutedEventArgs e)
        {
            CurrentKeys=new System.Windows.Forms.Keys[] { };

            CombinationChanged?.Invoke(null, CurrentKeys);
            ReloadMenuKeys();
        }
    }
}
