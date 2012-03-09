using System;
using System.Windows;
using System.Windows.Input;
using StickyWindows;
using System.Windows.Media.Animation;

namespace quicky
{
    /// <summary>
    /// Interaction logic for MyMessageBox.xaml
    /// </summary>
    public partial class MyMessageBox : Window
    {
        private NativeBehaviors m_NativeBehaviors;
        private bool snapToEdge;
        private Storyboard fadeInStoryboard, fadeOutStoryboard;
        public MyMessageBox()
        {
            InitializeComponent();

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

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.ShowInTaskbar = false;
            this.Topmost = true;
            this.FadeIn();
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
            button1.Focus();
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

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.FadeOut();
        }

        public void ShowMe(string message, string title = "Message")
        {
            this.txtMessage.Text = message;
            this.tbTitle.Text = title;
            this.ShowDialog();
        }
        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            if (args.ChangedButton == MouseButton.Left)
                DragMove();

        }

        internal static void SendMessage(string message, string title)
        {
            MyMessageBox b = new MyMessageBox();
            b.ShowMe(message, title);
            b = null;
        }


    }
}
