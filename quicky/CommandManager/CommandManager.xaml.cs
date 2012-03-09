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
using System.Collections.ObjectModel;
using quicky.Objects;
using System.IO;
using System.ComponentModel;
using quicky.Addon;
using Components.Shell;
using Microsoft.Win32;

namespace quicky.CommandManager
{
    /// <summary>
    /// Interaction logic for CommandManager.xaml
    /// </summary>
    public partial class CommandManager : Window
    {

        CommandBox currentCmd;

        #region constructor
        public CommandManager(CommandBox current)
        {
            InitializeComponent();
            currentCmd = current;
            StatusGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            if (args.ClickCount == 2)
            {
                if (this.WindowState == System.Windows.WindowState.Maximized)
                    this.WindowState = System.Windows.WindowState.Normal;
                else
                    this.WindowState = System.Windows.WindowState.Maximized;

                return;
            }

            if (args.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }


        }
        #endregion

        #region CollectionViewSources
        private System.Windows.Data.CollectionViewSource commandRepositoryViewSource;
        private System.Windows.Data.CollectionViewSource runCommandViewSource;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            try
            {
                //commandRepositoryViewSource.View.CurrentChanging += new System.ComponentModel.CurrentChangingEventHandler(View_CurrentChanging);

                this.Topmost = false;
                commandRepositoryViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("commandRepositoryViewSource")));
                // Load data by setting the CollectionViewSource.Source property:
                commandRepositoryViewSource.Source = RepositoryLibrary.Repositorys;
                commandRepositoryViewSource.View.CurrentChanged += new EventHandler(View_CurrentChanged);
                runCommandViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("runCommandViewSource")));
                // Load data by setting the CollectionViewSource.Source property:
                if (((CommandRepository)(((ListCollectionView)(commandRepositoryViewSource.View)).CurrentItem)) != null)
                    runCommandViewSource.Source = ((CommandRepository)(((ListCollectionView)(commandRepositoryViewSource.View)).CurrentItem)).Commands;

            }
            catch (Exception ex)
            {


                MyMessageBox b = new MyMessageBox();
                b.ShowMe(ex.Message, "Unhandled error");
                Application.Current.Shutdown();
            }


        }

        private void togleEnable(bool tof)
        {
            this.gridComboxRepository.IsEnabled = tof;
            
            foreach (MenuItem x in MainContextmenu.Items)
            {
                x.IsEnabled = tof;
            }

            this.ReposMenu.IsEnabled = tof;
            this.CommandsMenu.IsEnabled = tof;
            this.ExitNdSave.IsEnabled = tof;

            if (tof)
                this.runCommandDataGrid.IsReadOnly = false;
            else
                this.runCommandDataGrid.IsReadOnly = true;

        }

        private void View_CurrentChanged(object sender, EventArgs e)
        {
            try
            {
                if ((CommandRepository)(((ListCollectionView)(commandRepositoryViewSource.View)).CurrentItem) != null)
                {
                    runCommandViewSource.Source = ((CommandRepository)(((ListCollectionView)(commandRepositoryViewSource.View)).CurrentItem)).Commands;
                }
                else
                {
                    runCommandViewSource.Source = null;
                }
            }
            catch (Exception ex)
            {

                MyMessageBox b = new MyMessageBox();
                b.ShowMe(ex.Message, "Unhandled error");
                Application.Current.Shutdown();
            }
        }
        #endregion

        #region doubleclick type column to toggle type
        private void typeColumn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (string.IsNullOrWhiteSpace((runCommandViewSource.View.CurrentItem as RunCommand).Arguments))
                {
                    switch (((Control)sender).Tag.ToString())
                    {
                        case "type":
                            {

                                if ((runCommandViewSource.View.CurrentItem as RunCommand).Type == CommandType.CMDExecutedCommand)
                                    (runCommandViewSource.View.CurrentItem as RunCommand).Type = CommandType.SimpleRunCommand;
                                else if ((runCommandViewSource.View.CurrentItem as RunCommand).Type == CommandType.SimpleRunCommand)
                                    (runCommandViewSource.View.CurrentItem as RunCommand).Type = CommandType.AutoCadCommand;
                                else if ((runCommandViewSource.View.CurrentItem as RunCommand).Type == CommandType.AutoCadCommand)
                                    (runCommandViewSource.View.CurrentItem as RunCommand).Type = CommandType.CMDExecutedCommand;
                                break;
                            }

                    }

                }
                else
                {

                    MyMessageBox b = new MyMessageBox();
                    b.ShowMe("If you have used arguments, you must use CMDExecutedCommand type!", "Toggle error");
                    e.Handled = true;
                    b = null;
                }

            }
        }
        #endregion

        #region Drop Icone location

        //Create a Delegate that matches the Signature of the ProgressBar's SetValue method
        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        private bool draging = false;
        //create delegates to hande entry in unmanaged code
        private delegate void OneArgDelegateCreateNewRunCommand(String arg);
        // create list to hold extra files
        private void DoDrop(object sender, DragEventArgs e)
        {
            if (draging)
            {
                e.Handled = true;
                return;
            }
            draging = true;
            List<string> errors = new List<string>();
            bool usePGB = false;

            //only one image can be added at a time
            bool oneImageadded = false;

            if (commandRepositoryViewSource.View.CurrentItem == null)
            {
                e.Handled = true;
                return;
            }

            #region enable statusbar

            togleEnable(false);
            //Create a new instance of our ProgressBar Delegate that points
            //  to the ProgressBar's SetValue method.
            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(ProgressBarStatus.SetValue);

            //Stores the value of the ProgressBar
            double value = 0;
            #endregion

            try
            {


                // Handle FileDrop data.
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // Check if we have been given only one file, if not kill the user with error message!
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    List<string> newList = cleanUpListAndFindDirectories(files);

                    if (files.Count() > 1) oneImageadded = true;


                    #region enable statusbar


                    if (newList.Count() > 2)
                    {
                        usePGB = true;
                        //Configure the ProgressBar
                        StatusGrid.Visibility = System.Windows.Visibility.Visible;
                        ProgressBarStatus.Minimum = 0;
                        ProgressBarStatus.Maximum = newList.Count();
                        ProgressbarLabel.Text = "Gettering information of files...";

                        ProgressBarStatus.Value = 0;

                    }

                    #endregion


                    foreach (string s in newList)
                    {
                        ProgressbarLabel.Text = "Processing file: " + s;

                        System.Windows.Forms.Application.DoEvents();
                        try
                        {
                            if (System.IO.Path.GetExtension(s).Equals(".lnk", StringComparison.CurrentCultureIgnoreCase))
                            {
                                try
                                {
                                    RunCommand newCmd = RunCommand.CreateCommandFromLnk(s);

                                    if (newCmd != null && newCmd.Error.Length == 0)
                                    {
                                        (commandRepositoryViewSource.View.CurrentItem as CommandRepository).Commands.Add(newCmd);
                                        runCommandViewSource.View.MoveCurrentToLast();

                                        runCommandDataGrid.ScrollIntoView(runCommandDataGrid.SelectedItem);
                                        DataGridRow dgrow = (DataGridRow)runCommandDataGrid.ItemContainerGenerator.ContainerFromItem(runCommandDataGrid.SelectedItem);
                                        dgrow.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

                                        oneImageadded = true;
                                    }
                                }
                                catch (Exception ex)
                                {

                                    errors.Add("error in file: " + s + " " + ex.Message + Environment.NewLine);
                                }
        #endregion

                            }
                            else
                            {
                                #region check if its image
                                //file is not an ink object, check if its an image
                                bool isSuportedExtension = false;
                                string fileExstension = System.IO.Path.GetExtension(s);
                                foreach (string item in RunCommand.imageExt)
                                {
                                    if (fileExstension.ToLower().Equals(item, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        isSuportedExtension = true;
                                        break;
                                    }
                                }
                                #endregion

                                if (!isSuportedExtension)// it is not a supported image, check if we want to make a shortcut out of is --> extensions properties
                                {
                                    bool isValidExtension = false;
                                    foreach (string sr in Properties.Settings.Default.MakeShortCutExtensions)
                                    {
                                        if (System.IO.Path.GetExtension(s).Equals(sr, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            isValidExtension = true;
                                            break;
                                        }
                                    }

                                    if (isValidExtension)
                                    {
                                        #region Make it into a shortcut
                                        try
                                        {
                                            // Start fetching new runcommand.
                                            RunCommand newCmd = RunCommand.CreateCommandFromstring(s);

                                            if (newCmd != null && newCmd.Error.Length == 0)
                                            {
                                                (commandRepositoryViewSource.View.CurrentItem as CommandRepository).Commands.Add(newCmd);
                                                runCommandViewSource.View.MoveCurrentToLast();

                                                runCommandDataGrid.ScrollIntoView(runCommandDataGrid.SelectedItem);
                                                DataGridRow dgrow = (DataGridRow)runCommandDataGrid.ItemContainerGenerator.ContainerFromItem(runCommandDataGrid.SelectedItem);
                                                dgrow.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

                                                oneImageadded = true;
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                            errors.Add(s + "  failed for shortcut creation: " + ex.Message + Environment.NewLine);
                                        }
                                        #endregion
                                    }

                                }
                                else
                                {
                                    #region Add maximum one image
                                    if (!oneImageadded)//if more items were provided, only add one image
                                    {

                                        HitTestResult result = VisualTreeHelper.HitTest(this, e.GetPosition(this));
                                        DependencyObject obj = result.VisualHit;


                                        while (VisualTreeHelper.GetParent(obj) != null && !(obj is DataGrid))
                                        {
                                            obj = VisualTreeHelper.GetParent(obj);
                                            if (obj is DataGridRow)
                                            {
                                                runCommandDataGrid.SelectedItem = (obj as DataGridRow).Item;
                                                break;
                                            }
                                        }

                                        if (runCommandViewSource.View.CurrentItem != null)
                                        {
                                            (runCommandViewSource.View.CurrentItem as RunCommand).IconLocation = s;

                                        }

                                        oneImageadded = true;
                                    }
                                }
                                    #endregion
                            }
                        }
                        catch (Exception ex)
                        {

                            errors.Add("Unhandled error" + ex.Message + Environment.NewLine);
                        }
                        finally
                        {
                            #region Step statusbar
                            if (usePGB)
                            {
                                value += 1;

                                Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });

                            }


                            #endregion
                        }
                    }
                }

                #region textdrop = only for images
                // Handle TextDrop data. Texstdrops are only for icons!
                if (e.Data.GetDataPresent(DataFormats.Text))
                {

                    if (runCommandViewSource.View.CurrentItem == null)
                    {

                        HitTestResult result = VisualTreeHelper.HitTest(this, e.GetPosition(this));
                        DependencyObject obj = result.VisualHit;


                        while (VisualTreeHelper.GetParent(obj) != null && !(obj is DataGrid))
                        {
                            obj = VisualTreeHelper.GetParent(obj);
                            if (obj is DataGridRow)
                            {
                                runCommandDataGrid.SelectedItem = (obj as DataGridRow).Item;
                                oneImageadded = true;
                            }
                        }
                    }

                    string file = (string)e.Data.GetData(DataFormats.Text);

                    (runCommandDataGrid.SelectedItem as RunCommand).IconLocation = file;


                }
                #endregion


            }
            catch (Exception ex)
            {
                errors.Add(ex.Message + Environment.NewLine + Environment.NewLine);
            }

            finally
            {

                #region reporting errors
                if (errors.Count >= 1 && errors.Count < 50)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string x in errors)
                        sb.Append(x);


                    sendMessage("Drop failer for the item(s): " + Environment.NewLine + Environment.NewLine + sb.ToString(), "Drop error(s)");

                }
                else if (errors.Count > 50)
                {
                    sendMessage("a large number of files were logged, they will be displayed in a txt file...", "Drop error(s)");
                    StringBuilder sb = new StringBuilder();
                    foreach (string x in errors)
                        sb.Append(x);

                    try
                    {
                        string tempPath = System.IO.Path.GetTempPath() + DateTime.Now.ToFileTime() + ".txt";

                        File.WriteAllText(tempPath, sb.ToString());
                        RunCommand x = new RunCommand();
                        x.Type = CommandType.SimpleRunCommand;
                        x.Command = tempPath;
                        x.ExecuteCommandSync();
                    }
                    catch (Exception ex)
                    {

                        sendMessage("Fatal error!" + Environment.NewLine + ex.Message, "Writing to textfile failed.");
                    }


                }
                #endregion

                #region Step statusbar
                StatusGrid.Visibility = System.Windows.Visibility.Collapsed;
                ProgressbarLabel.Text = string.Empty;
                if (usePGB)
                {

                    value = 0;

                    /*Update the Value of the ProgressBar:
                      1)  Pass the "updatePbDelegate" delegate that points to the ProgressBar1.SetValue method
                      2)  Set the DispatcherPriority to "Background"
                      3)  Pass an Object() Array containing the property to update (ProgressBar.ValueProperty) and the new value */
                    Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });

                }
                togleEnable(true);

                #endregion
                draging = false;
                e.Handled = true;
            }

        }


        private List<string> cleanUpListAndFindDirectories(string[] files)
        {
            List<string> newStrings = new List<string>();

            foreach (string s in files)
            {

                // get the file attributes for file or directory
                FileAttributes attr = File.GetAttributes(s);

                //detect whether its a directory or file
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                //Get all files from that directory and add the to extra list of files
                {
                    DirectoryInfo newDir = new DirectoryInfo(s);
                    FileInfo[] listFiles = newDir.GetFiles("*", SearchOption.AllDirectories);

                    foreach (FileInfo item in listFiles)
                    {
                        newStrings.Add(item.FullName);
                    }


                }
                else
                {
                    newStrings.Add(s);
                }

            }
            return newStrings;
        }

        #region Events
        private void remove_repository_click(object sender, RoutedEventArgs e)
        {
            removerepository();
        }

        private void mouseUpExecute_Command(object sender, MouseButtonEventArgs e)
        {
            execute();

        }

        private void mouse_fromtextbox_UpExecute_Command(object sender, RoutedEventArgs e)
        {
            execute();

        }

        private void SaveChangesOnClose(object sender, CancelEventArgs e)
        {
            save();
        }

        private void MenuItem_NewCommand_Click(object sender, RoutedEventArgs e)//create new command in current repo
        {
            addNewCommand();
        }

        private void AddNewCommand_click(object sender, MouseButtonEventArgs e)
        {
            addNewCommand();
        }

        private void click_close_progy(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void mouseUpDelete_Command(object sender, MouseButtonEventArgs e)
        {

            removeSelectedCommands();

        }

        private void MenuItem_deleteCommand_Click(object sender, RoutedEventArgs e)
        {
            removeSelectedCommands();
        }

        private void add_repository_click(object sender, RoutedEventArgs e)
        {
            addANewRepository();
        }

        private void mouseUp_fromtextbox_Delete_Command(object sender, RoutedEventArgs e)
        {
            removeSelectedCommands();
        }

        private void AddNewCommand_fromtextbox_click(object sender, RoutedEventArgs e)
        {
            addNewCommand();
        }

        private void click_minimize_progy(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void click_explore_icons_progy(object sender, RoutedEventArgs e)
        {
            RunCommand x = new RunCommand
            {
                Command = MyImageWorker.myPath
            };

            x.ExecuteCommandSync();
        }

        private void Export_click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveDia = new SaveFileDialog();
                saveDia.Title = "Please select where to save the xaml file";
                saveDia.ShowDialog();
                if (!string.IsNullOrWhiteSpace(saveDia.SafeFileName))
                {
                    if (commandRepositoryViewSource.View.CurrentItem != null)
                    {
                        (commandRepositoryViewSource.View.CurrentItem as CommandRepository).Save(saveDia.FileName + ".xaml");
                    }
                }
            }
            catch (Exception ex)
            {

                sendMessage(ex.Message, "Unhandled error");

            }

        }

        private void click_test_acad(object sender, RoutedEventArgs e)
        {
            //this.Hide();
            //Interop.AutoCad_Connection x = new Interop.AutoCad_Connection();
            //x.SendMessage("The connection has been tested.");
            //x.SendMessage("BeDeVeloper Quicky Launcher is now ready to communicate with autocad!");
            //sendMessage("Respond to the question in autocad.", "Comunication form acad");
            //sendMessage("You're answer was:" + x.GetString("Please enter you're name to test the connection in both ways"), "Comunication form acad");
            //this.Show();
        }
        #endregion

        #region Private methodes

        private void addNewCommand()
        {
            try
            {
                if (commandRepositoryViewSource.View.CurrentItem == null)
                    return;

                int i = 0;
                const string newName = "NewCommand ";

                do
                {
                    i++;
                } while (RepositoryLibrary.ExistsQuicky(newName + i));


                (commandRepositoryViewSource.View.CurrentItem as CommandRepository).Commands.Add(new RunCommand
                {
                    Command = "",
                    Desctiption = "",
                    IconLocation = "",
                    Quicky = newName + i,
                    Type = CommandType.SimpleRunCommand
                });

                runCommandViewSource.View.MoveCurrentToLast();
            }
            catch (Exception ex)
            {

                sendMessage(ex.Message, "Unhandled error");

            }


        }

        private void removeSelectedCommands()
        {
            try
            {
                if (commandRepositoryViewSource.View.CurrentItem == null)
                    return;



                if (runCommandDataGrid.SelectedItems.Count > 0)
                {

                    List<RunCommand> listtodelete = new List<RunCommand>();
                    foreach (var x in runCommandDataGrid.SelectedItems)
                    {

                        listtodelete.Add(x as RunCommand);
                    }
                    foreach (RunCommand cmd in listtodelete)
                    {
                        //if a command is in editing mode an exeption will be trown
                        (runCommandDataGrid.ItemsSource as ListCollectionView).Remove(cmd);
                    }

                }
            }
            catch (Exception ex)
            {

                if (ex.Message == "'RemoveAt' is not allowed during an AddNew or EditItem transaction.")
                {
                    //this will loop until edit has been completly canceled
                    runCommandDataGrid.CancelEdit();

                    removeSelectedCommands();
                }
                else
                {
                    sendMessage(ex.Message, "Unhandled exeption");
                }


            }


        }

        private void addANewRepository()
        {
            try
            {
                Inputbox getname = new Inputbox("Please give a unique name for the repository", "Input name");
                getname.ShowDialog();

                if (!getname.IsCanceled)
                {
                    RepositoryLibrary.AddRepository(getname.ResultName);
                    commandRepositoryViewSource.View.MoveCurrentToLast();
                    addNewCommand();
                    runCommandViewSource.View.MoveCurrentToLast();
                }
            }
            catch (Exception ex)
            {


                sendMessage(ex.Message, "Unhandled error");

            }


        }

        private void save()
        {
            #region enable statusbar
            this.IsEnabled = false;
            //Create a new instance of our ProgressBar Delegate that points
            //  to the ProgressBar's SetValue method.
            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(ProgressBarStatus.SetValue);
            bool usePGB = false;
            //Stores the value of the ProgressBar
            double value = 0;
            #endregion
            try
            {
                #region enable statusbar

                usePGB = true;
                //Configure the ProgressBar
                StatusGrid.Visibility = System.Windows.Visibility.Visible;
                ProgressBarStatus.Minimum = 0;
                ProgressBarStatus.Maximum = RepositoryLibrary.Repositorys.Count + 1;// add 1 for cleaning operation
                ProgressBarStatus.Value = 0;



                #endregion

                foreach (CommandRepository repo in RepositoryLibrary.Repositorys)
                {

                    repo.Save();
                    #region Step statusbar
                    if (usePGB)
                    {
                        value += 1;
                        ProgressbarLabel.Text = "Saving repositorys, please wait...";
                        //Update the Value of the ProgressBar:

                        Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });

                    }

                    #endregion
                }

            }
            catch (Exception ex)
            {

                sendMessage(ex.Message, "Unhandled error");

            }
            finally
            {
                #region Cleanup
                ProgressbarLabel.Text = "Cleaning up icondsfolder...";
                MyImageWorker.CleanIconFolder();
                value += 1;
                Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });
                #endregion

                ProgressbarLabel.Text = string.Empty;
                StatusGrid.Visibility = System.Windows.Visibility.Collapsed;
                commandRepositoryViewSource = null;
                runCommandViewSource = null;
                callReload();
                currentCmd = null;
                GC.Collect();
            }

        }

        private void removerepository()
        {
            if (commandRepositoryViewSource.View.CurrentItem != null)
                RepositoryLibrary.RemoveRepository((commandRepositoryViewSource.View.CurrentItem as CommandRepository).Name);
        }

        private void execute()
        {
            try
            {
                (runCommandDataGrid.SelectedItem as RunCommand).ExecuteCommandSync();
            }
            catch (Exception ex)
            {


                sendMessage(ex.Message, "Command error");

            }
        }

        private void callReload()
        {
            if (RepositoryLibrary.Repositorys.Count > 0)
                currentCmd.Restart();
        }

        private void sendMessage(string message, string title)
        {
            MyMessageBox.SendMessage(message, title);
        }


        #endregion

     
    }
}
