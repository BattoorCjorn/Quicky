using System.Windows;
using StickyWindows;

namespace quicky.Addon
{
    /// <summary>
    /// Interaction logic for taskbarDummy.xaml
    /// </summary>
    public partial class taskbarDummy : Window
    {
        private NativeBehaviors m_NativeBehaviors;
        private SnapToBehavior stb = new SnapToBehavior();
     
        public taskbarDummy(Point grippoint, Size size)
        {
            InitializeComponent();
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            WindowManager.RegisterWindow(this);
            m_NativeBehaviors = new NativeBehaviors(this);
            stb = new SnapToBehavior();
            stb.OriginalForm = this;
            NativeBehaviors.Add(stb);

            this.ShowInTaskbar = false;
            this.Top = grippoint.Y;
            this.Left = grippoint.X;
            this.Width = size.Width;
            this.Height = size.Height;
            this.Show();
           
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
               

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            WindowManager.UnregisterWindow(this);
            NativeBehaviors.Remove(stb);
            base.OnClosing(e);
        }
    }
}
