using System;
using System. ComponentModel;
using System. Windows;
using System. Windows. Input;
using System. Collections. Generic;
using System. Collections. ObjectModel;
using quicky. Objects;
using System. Windows. Media. Animation;
using StickyWindows;
using System. Windows. Media. Imaging;
using System. Windows. Controls;
using System. Drawing;

namespace quicky
{

    public partial class CommandBox: Window
    {
        #region Public
        public CodeProject. SystemHotkey. SystemHotkey SystemHotkey1 = new CodeProject. SystemHotkey. SystemHotkey ( );

        public void Restart ( )
        {
            loadCommands ( );

        }
        #endregion

        #region private
        private Storyboard fadeInStoryboard, fadeOutStoryboard;
        private bool shouldClose;
        private RunCommand lastUsedCommand;
        private bool started;
        private bool snapToEdge;
        private bool snapToTaskBar;
        private NativeBehaviors m_NativeBehaviors;
        private Addon. WindowSettings sw;
        private bool storyBoardRunning = false;
        private SnapToBehavior stb = new SnapToBehavior ( );
        MyWorkingAreaSpy spy = new MyWorkingAreaSpy ( );
        #endregion

        #region lists
        protected static ObservableCollection<Objects. RunCommand> runcommands;


        #endregion

        #region INativeBehavioral Members

        public NativeBehaviors NativeBehaviors
        {

            get
            {
                return m_NativeBehaviors;
            }
        }

        #endregion

        #region Constructor
        public CommandBox ( )
        {

            InitializeComponent ( );

            initializeNow ( );

            this. txtInput. ItemsSource = runcommands;
            this. txtInput. DisplayMemberPath = "Quicky";
            this. txtInput. SelectedValuePath = "Command";
        }
        #endregion

        #region Initializing
        public void initializeNow ( )
        {


            try
            {


                #region initialize variables      
                runcommands = new ObservableCollection<Objects. RunCommand> ( );
                snapToEdge = Properties. Settings. Default. SnapToEdges;
                snapToTaskBar = Properties. Settings. Default. SnapToTaskbar;
                started = false;
                shouldClose = false;
                this. Topmost = Properties. Settings. Default. OnTop;
                this. ResizeMode = System. Windows. ResizeMode. NoResize;

                #endregion

                this. Close ( );

                #region sticky windows
                if ( snapToEdge )
                    unRegisterThisSnapWindow ( );

                #endregion

                #region FADE

                // Create the fade in storyboard
                fadeInStoryboard = new Storyboard ( );
                fadeInStoryboard. Completed += new EventHandler ( fadeInStoryboard_Completed );
                DoubleAnimation fadeInAnimation = new DoubleAnimation ( 0.0, 1.0, new Duration ( TimeSpan. FromSeconds ( 0.20 ) ) );
                Storyboard. SetTarget ( fadeInAnimation, this );
                Storyboard. SetTargetProperty ( fadeInAnimation, new PropertyPath ( UIElement. OpacityProperty ) );
                fadeInStoryboard. Children. Add ( fadeInAnimation );

                // Create the fade out storyboard
                fadeOutStoryboard = new Storyboard ( );
                fadeOutStoryboard. Completed += new EventHandler ( fadeOutStoryboard_Completed );
                DoubleAnimation fadeOutAnimation = new DoubleAnimation ( 1.0, 0.0, new Duration ( TimeSpan. FromSeconds ( 0.50 ) ) );
                Storyboard. SetTarget ( fadeOutAnimation, this );
                Storyboard. SetTargetProperty ( fadeOutAnimation, new PropertyPath ( UIElement. OpacityProperty ) );
                fadeOutStoryboard. Children. Add ( fadeOutAnimation );

                #endregion

                #region setting window position
                this. WindowStartupLocation = System. Windows. WindowStartupLocation. Manual;
                this. Left = Properties. Settings. Default. LocLeft;
                this. Top = Properties. Settings. Default. LocTop;

                double vsLeft = System. Windows. SystemParameters. VirtualScreenWidth;
                double vsTop = System. Windows. SystemParameters. VirtualScreenHeight;

                if ( this. Left > vsLeft || this. Top > vsTop )
                {
                    this. Left = vsLeft - this. Width;
                    this. Top = vsTop - this. Height;
                }


                #endregion
                //create hotkey
                SystemHotkey1. Shortcut = System. Windows. Forms. Shortcut. F1;
                SystemHotkey1. Pressed += new System. EventHandler ( this. systemHotkey1_Pressed );

                loadCommands ( );

                if ( snapToTaskBar )
                    findTaskbar ( );


            }
            catch ( Exception ex )
            {
                MyMessageBox. SendMessage ( ex. Message, "Startup error" );
                Application. Current. Shutdown ( );
            }


            started = true;

        }


        private void loadCommands ( )
        {

            RepositoryLibrary. LoadRepositorys ( );

            //load runcommands
            List<Objects. RunCommand> runcommandsList = new List<Objects. RunCommand> ( );

            foreach ( CommandRepository repo in RepositoryLibrary. Repositorys )
            {

                foreach ( RunCommand rc in repo. Commands )
                {

                    runcommandsList. Add ( rc );
                }
            }
            //Create hardcoded commands
            #region HardCoded commands
            // runcommandsList.Add(new RunCommand
            //{
            //    Command = "QUICKY RELOAD",
            //    Desctiption = "Reinitializes the Quiky commands"
            //    ,
            //    Quicky = "QUICKY RELOAD",
            //    Type = CommandType.SimpleRunCommand
            //});


            #endregion

            #region Sort and add to Observable
            runcommandsList. Sort ( delegate ( RunCommand g1, RunCommand g2 )
            {
                return g1. AlphaName. CompareTo ( g2. AlphaName );
            } );

            //emptey list of commands
            runcommands. Clear ( );
            foreach ( var item in runcommandsList )
            {
                runcommands. Add ( item );
            }
            #endregion

            //clean
            runcommandsList = null;

        }
        #endregion

        #region Public Hotkey event
        public void systemHotkey1_Pressed ( object sender, System. EventArgs e )
        {
            pressedHotkey ( );
        }

        private void pressedHotkey ( )
        {

            if ( this. IsVisible )
            {
                if ( this. IsActive )
                {
                    Close ( );
                }
                else
                {
                    this. Activate ( );
                    txtInput. Focus ( );
                }
            }

            else
                Open ( );
        }
        #endregion

        #region Private methodes
        private void reloadSettings ( )
        {
            this. Topmost = Properties. Settings. Default. OnTop;
            if ( !this. snapToEdge. Equals ( Properties. Settings. Default. SnapToEdges ) ) //detect if changes have been made
            {
                this. snapToEdge = Properties. Settings. Default. SnapToEdges;
                if ( snapToEdge )
                {
                    registerThisSnapWindow ( );
                }
                else
                {
                    unRegisterThisSnapWindow ( );

                }
            }
            if ( !this. snapToTaskBar. Equals ( Properties. Settings. Default. SnapToTaskbar ) ) //detect if changes have been made
            {
                this. snapToTaskBar = Properties. Settings. Default. SnapToTaskbar;
                if ( snapToTaskBar )
                {
                    spy. RegisterTaskBarDummys ( );
                }
                else
                {
                    spy. CloseWindows ( );

                }
            }

        }

        private void registerThisSnapWindow ( )
        {
            WindowManager. RegisterWindow ( this );
            m_NativeBehaviors = new NativeBehaviors ( this );
            stb. OriginalForm = this;
            NativeBehaviors. Add ( stb );
        }

        private void unRegisterThisSnapWindow ( )
        {
            if ( WindowManager. Windows. Contains ( this ) )
                WindowManager. UnregisterWindow ( this );
            if ( m_NativeBehaviors != null ) if ( m_NativeBehaviors. Contains ( stb ) )
                    m_NativeBehaviors. Remove ( stb );
        }

        private void Open ( )
        {
            if ( snapToEdge )
            {
                registerThisSnapWindow ( );
            }
            checkWindowPosision ( );
            this. Opacity = 0.0;
            this. Show ( );
            FadeIn ( );

        }

        private void checkWindowPosision ( )
        {
            if ( !IsOnScreen(this) )
            {
                this. Left = 0;
                this. Top = 0;
            }

        }

        private bool IsOnScreen ( Window form )
        {
            System.Windows.Forms.Screen [ ] screens = System.Windows.Forms.Screen. AllScreens;
            foreach ( System.Windows.Forms.Screen screen in screens )
            {
                Rectangle formRectangle = new Rectangle ( ( int ) form. Left, ( int ) form. Top, ( int ) form. ActualWidth, ( int ) form. ActualHeight );

                if ( screen. WorkingArea. Contains ( formRectangle ) )
                {
                    return true;
                }
            }

            return false;
        }

        public void findTaskbar ( )
        {
            spy. RegisterTaskBarDummys ( );
        }

        public void removeTaskbar ( )
        {
            spy. CloseWindows ( );
        }

        #endregion

        #region Events of CommandBox

        private void Window_Deactivated ( object sender, EventArgs e )
        {
            if ( Properties. Settings. Default. CloseonfocusLost )
                this. Close ( );
        }

        protected override void OnClosing ( CancelEventArgs e )
        {

            if ( !shouldClose )
            {


                e. Cancel = true;

                if ( started )
                {
                    if ( snapToEdge )
                        unRegisterThisSnapWindow ( );
                    FadeOut ( );
                }
                else
                    this. Hide ( );



            }
            else
            {
                Properties. Settings. Default. LocTop = this. Top;
                Properties. Settings. Default. LocLeft = this. Left;
                Properties. Settings. Default. Save ( );
                FadeOut ( );
                Application. Current. Shutdown ( );
            }

        }

        private void txtInput_KeyDown ( object sender, KeyEventArgs e )
        {
            if ( e. Key == Key. Enter )
            {
                Objects. RunCommand x = txtInput. SelectedItem as Objects. RunCommand;
                if ( x != null )
                {
                    try
                    {
                        x. ExecuteCommandSync ( this );
                        lastUsedCommand = x;
                        this. Close ( );
                    }
                    catch ( Exception ex )
                    {
                        MyMessageBox. SendMessage ( ex. Message, x. Desctiption );
                    }
                }
                else
                {
                    Objects. RunCommand x1 = new Objects. RunCommand
                    {
                        Command = txtInput. Text,
                        Desctiption = "Unknown command",
                        Type = CommandType. SimpleRunCommand
                    };

                    if ( !string. IsNullOrWhiteSpace ( x1. Command ) )
                    {
                        try
                        {
                            x1. ExecuteCommandSync ( this );
                            lastUsedCommand = x1;
                            this. Close ( );
                        }
                        catch ( Exception ex )
                        {

                            MyMessageBox. SendMessage ( ex. Message, x1. Desctiption );

                        }
                    }
                    else
                        try
                        {
                            if ( lastUsedCommand != null )
                                lastUsedCommand. ExecuteCommandSync ( this );
                        }
                        catch ( Exception ex )
                        {

                            MyMessageBox. SendMessage ( ex. Message, lastUsedCommand. Desctiption );
                        }
                }

            }

            if ( e. Key == Key. Escape )
            {
                if ( string. IsNullOrWhiteSpace ( this. txtInput. Text ) )
                {

                    this. Close ( );
                }
                else
                {
                    imageView. ImageSource = null;
                    this. txtInput. Text = "";
                    this. txtInput. SelectedIndex = -1;
                }
            }
        }

        public void DragWindow ( object sender, MouseButtonEventArgs args )
        {
            if ( args. ChangedButton == MouseButton. Left )
            {
                DragMove ( );

                Properties. Settings. Default. LocLeft = this. Left;
                Properties. Settings. Default. LocTop = this. Top;
            }
        }

        private void txtInput_SelectionChanged ( object sender, System. Windows. Controls. SelectionChangedEventArgs e )
        {
            Objects. RunCommand x = txtInput. SelectedItem as Objects. RunCommand;
            if ( x != null )
            {
                try
                {
                    txtDescription. Text = x. Desctiption;
                    if ( string. IsNullOrWhiteSpace ( x. IconLocation ) )
                    {

                        imageView. ImageSource = null;
                        MyIconViewer. Visibility = System. Windows. Visibility. Hidden;
                    }
                    else
                    {
                        imageView. ImageSource = x. ImageSource;
                        MyIconViewer. Visibility = System. Windows. Visibility. Visible;
                    }
                    //"Resources/Redbutton.png"
                }
                catch ( Exception ex )
                {
                    imageView. ImageSource = null;
                    MyIconViewer. Visibility = System. Windows. Visibility. Hidden;
                    MyMessageBox. SendMessage ( ex. Message, x. Desctiption );
                }
            }
            else
            {
                MyIconViewer. Visibility = System. Windows. Visibility. Hidden;
                imageView. ImageSource = null;
                txtDescription. Text = "";
            }
        }

        private void btnClose_Click ( object sender, RoutedEventArgs e )
        {
            this. Close ( );
        }

        private void SetImageTop ( object sender, RoutedEventArgs e )
        {
            Grid. SetRow ( MyIconViewer, 0 );
            Grid. SetRow ( StackPanelComboBoxBorder, 1 );

        }

        private void SetImageBot ( object sender, RoutedEventArgs e )
        {
            Grid. SetRow ( MyIconViewer, 1 );
            Grid. SetRow ( StackPanelComboBoxBorder, 0 );
        }

        private void SetImageAlLeft ( object sender, RoutedEventArgs e )
        {
            MyIconViewer. HorizontalAlignment = System. Windows. HorizontalAlignment. Left;
        }

        private void SetImageAlCenter ( object sender, RoutedEventArgs e )
        {
            MyIconViewer. HorizontalAlignment = System. Windows. HorizontalAlignment. Center;
        }

        private void SetImageRight ( object sender, RoutedEventArgs e )
        {
            MyIconViewer. HorizontalAlignment = System. Windows. HorizontalAlignment. Right;
        }

        #endregion

        #region Context menu Tray

        private void OnNotificationAreaIconDoubleClick ( object sender, MouseButtonEventArgs e )
        {
            if ( e. ChangedButton == MouseButton. Left )
            {
                pressedHotkey ( );
            }
        }

        private void MenuItem_Click ( object sender, EventArgs e )
        {
            storyBoardRunning = true;
            //open settings window only when it is not already open!
            if ( sw == null )
            {
                sw = new Addon. WindowSettings ( this );
                sw. ShowDialog ( );
                reloadSettings ( );
                sw = null;
            }
            else
                sw. Activate ( );

            storyBoardRunning = false;
        }

        private void OnMenuItemOpenClick ( object sender, EventArgs e )
        {
            pressedHotkey ( );
        }

        private void OnMenuItemExitClick ( object sender, EventArgs e )
        {
            shouldClose = true;
            Close ( );
        }
        #endregion

        #region fade
        private void fadeOutStoryboard_Completed ( object sender, EventArgs e )
        {
            this. Hide ( );
            storyBoardRunning = false;

        }
        private void fadeInStoryboard_Completed ( object sender, EventArgs e )
        {

            txtInput. Focus ( );
            storyBoardRunning = false;

        }

        /// <summary>
        /// Fades the window in.
        /// </summary>
        public void FadeIn ( )
        {
            if ( !storyBoardRunning )
            {
                this. Activate ( );
                // Begin fade in animation
                storyBoardRunning = true;
                fadeInStoryboard. Begin ( );
            }
        }

        /// <summary>
        /// Fades the window out.
        /// </summary>
        public void FadeOut ( )
        {
            if ( !storyBoardRunning )
            {
                // Begin fade out animation
                storyBoardRunning = true;
                fadeOutStoryboard. Begin ( );
            }
        }

        #endregion







    }
}