using System;
using System.Windows;
using System.Windows.Input;
using StickyWindows;
using System.Windows.Media.Animation;

namespace quicky.Addon
{


    /// <summary>
    /// Interaction logic for Inputbox.xaml
    /// </summary>
    public partial class Inputbox : Window
    {
        #region standards window
        private NativeBehaviors m_NativeBehaviors;
        private bool snapToEdge;
        private Storyboard fadeInStoryboard, fadeOutStoryboard;
       
        #endregion

        public string ResultName
        {
            get;
            set;
        }

        public bool IsCanceled
        {
            get;
            set;
        }

        public Inputbox(string message, string _title)
        {
            InitializeComponent();
            IsCanceled = true;
            this.ResultName = "";

            this.tbTitle.Text = _title;
            this.txtMessage.Text = message;

            

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

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            ResultName = txtinput.Text;
            if (string.IsNullOrWhiteSpace(txtinput.Text))
                return;
            
            if (NameIsValid())
            {
                IsCanceled = false;
               
            }
            else
                return;
            this.FadeOut();
        }

        private bool NameIsValid()
        {

            if (Objects.RepositoryLibrary.ExistsNameRepository(ResultName))
            {
                quicky.MyMessageBox b = new quicky.MyMessageBox();
                b.ShowMe("That name is already taken, please enter new name", "Duplicate names found");
                this.txtinput.Text = "";
                return false;
            }
            return true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.FadeOut();
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
        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            if (args.ChangedButton == MouseButton.Left)
                DragMove();

        }


      
    }
}
