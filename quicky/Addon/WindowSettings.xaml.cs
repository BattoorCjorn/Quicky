using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using StickyWindows;
using System.Windows.Media.Animation;
using System.ComponentModel;
using Microsoft.Win32;

namespace quicky.Addon
{
    /// <summary>
    /// Interaction logic for WindowSettings.xaml
    /// </summary>
    public partial class WindowSettings : Window
    {
        private NativeBehaviors m_NativeBehaviors;
        private bool snapToEdge;
        private Storyboard fadeInStoryboard, fadeOutStoryboard;
        CommandBox currentCmd;
        public WindowSettings(CommandBox current)
        {
            InitializeComponent();
            this.txtTitle.Text = "Quicky simple settings";
            snapToEdge = Properties.Settings.Default.SnapToEdges;
            #region sticky windows
            if (snapToEdge)
            {
                WindowManager.RegisterWindow(this);
                m_NativeBehaviors = new NativeBehaviors(this);
                SnapToBehavior stb = new SnapToBehavior();
                stb.OriginalForm = this;
                NativeBehaviors.Add(stb);
            }
           
            #endregion

            currentCmd = current;

            #region FADE

            // Create the fade in storyboard
            fadeInStoryboard = new Storyboard();
            fadeInStoryboard.Completed += new EventHandler(fadeInStoryboard_Completed);
            DoubleAnimation fadeInAnimation = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(0.20)));
            Storyboard.SetTarget(fadeInAnimation, this);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(UIElement.OpacityProperty));
            fadeInStoryboard.Children.Add(fadeInAnimation);

            // Create the fade out storyboard
            fadeOutStoryboard = new Storyboard();
            fadeOutStoryboard.Completed += new EventHandler(fadeOutStoryboard_Completed);
            DoubleAnimation fadeOutAnimation = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromSeconds(0.50)));
            Storyboard.SetTarget(fadeOutAnimation, this);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(UIElement.OpacityProperty));
            fadeOutStoryboard.Children.Add(fadeOutAnimation);

            #endregion

            this.FadeIn();

            loadSettings();
        }

       

        #region INativeBehavioral Members

        public NativeBehaviors NativeBehaviors
        {

            get
            {
                return m_NativeBehaviors;
            }
        }

        #endregion

        #region fade
        private void fadeOutStoryboard_Completed(object sender, EventArgs e)
        {
            WindowManager.UnregisterWindow(this);
            this.Close();
        }
        private void fadeInStoryboard_Completed(object sender, EventArgs e)
        {

            this.Activate();
        }

        /// <summary>
        /// Fades the window in.
        /// </summary>
        public void FadeIn()
        {
            // Begin fade in animation
            fadeInStoryboard.Begin();
        }

        /// <summary>
        /// Fades the window out.
        /// </summary>
        public void FadeOut()
        {
            // Begin fade out animation
            fadeOutStoryboard.Begin();
        }

        #endregion

        #region Events
        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            if (args.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            saveSettings();
            this.FadeOut();
        }

       
        #endregion

        #region private Methods

        /// <summary>
        /// Remove or add a regestry key that will make this program load at startup
        /// </summary>
        /// <param name="add"></param>
        private void createStartRegestryKey(bool add)
        {
            string thisPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            //ADD KEY
            if (add)
            {
                Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                RegistryKey myKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                myKey.SetValue("QuickyStartUpPath", thisPath, RegistryValueKind.String);
            }
            else
            {
                //REMOVE KEY
                RegistryKey myKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (myKey.GetValueNames().Contains("QuickyStartUpPath"))
                    myKey.DeleteValue("QuickyStartUpPath");
            }
        }

        private bool existsStartRegestryKey()
        {
            string thisPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

           
                RegistryKey myKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (myKey.GetValueNames().Contains("QuickyStartUpPath"))
                {
                 if (thisPath.Equals(  myKey.GetValue("QuickyStartUpPath").ToString()))
                     return true;
                }
                return false;
        }

        private void loadSettings()
        {
            cbStartup.IsChecked = existsStartRegestryKey();
            cbSnapToEdge.IsChecked = Properties.Settings.Default.SnapToEdges;
            cbOnTop.IsChecked = Properties.Settings.Default.OnTop;
            cbHideInavctive.IsChecked = Properties.Settings.Default.CloseonfocusLost;
            cbSnapToTaskBar.IsChecked = Properties.Settings.Default.SnapToTaskbar;
        }

        private void saveSettings()
        {
            createStartRegestryKey((bool)cbStartup.IsChecked);
            Properties.Settings.Default.SnapToEdges = (bool)cbSnapToEdge.IsChecked;
            Properties.Settings.Default.OnTop = (bool)cbOnTop.IsChecked;
            Properties.Settings.Default.CloseonfocusLost = (bool)cbHideInavctive.IsChecked;
            Properties.Settings.Default.SnapToTaskbar = (bool)cbSnapToTaskBar.IsChecked;
            Properties.Settings.Default.Save();
        }
        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            CommandManager.CommandManager cmdmngr = new CommandManager.CommandManager(currentCmd);
            saveSettings();
            this.FadeOut();
            cmdmngr.ShowDialog();
          
            
        }

    }
}
