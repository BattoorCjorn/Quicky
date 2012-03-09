using System;
using System. Collections. Generic;
using System. Linq;
using System. Text;
using System. Collections. ObjectModel;
using System. ComponentModel;
using System. Windows. Media. Imaging;
using System. IO;
using Components. Shell;
using System. Windows. Media;

namespace quicky. Objects
{


    public class RunCommand: INotifyPropertyChanged, IDataErrorInfo
    {
        public static string [ ] imageExt = new string [ 4 ] { ".bmp", ".jpg", ".gif", ".png" };

        private string description;
        public string Desctiption
        {
            get
            {
                return this. description;
            }
            set
            {
                if ( value != this. description )
                {
                    this. description = value;
                    NotifyPropertyChanged ( "Desctiption" );
                }
            }
        }
        private string command;
        public string Command
        {
            get
            {
                return this. command;
            }
            set
            {
                if ( value != this. command || value == "" )
                {
                    this. command = value;
                    NotifyPropertyChanged ( "Command" );
                }
            }
        }

        public bool IsCommandValid ( string value )
        {
            return true;
        }

        private string arguments;
        public string Arguments
        {
            get
            {
                return this. arguments;
            }
            set
            {
                if ( value != this. arguments || value == "" )
                {
                    if ( value == "" )
                    {
                        this. type = CommandType. SimpleRunCommand;
                    }
                    else if ( this. type != CommandType. CMDExecutedCommand )
                    {
                        this. type = CommandType. CMDExecutedCommand;
                    }

                    this. arguments = value;
                    NotifyPropertyChanged ( "Arguments" );
                    NotifyPropertyChanged ( "Type" );

                }
            }
        }





        #region ICON
        private string iconLocation;
        public string IconLocation
        {
            get
            {
                return this. iconLocation;
            }
            set
            {
                if ( iconLocation == value && !string. IsNullOrWhiteSpace ( value ) )
                    return;

                if ( IsIconLocationValid ( value ) )
                {
                    //before we set anything on this we will first transform and 
                    //relocate the image for further use

                    string buffer = relocateIconImage ( value. ToString ( ) );

                    if ( IsIconLocationValid ( buffer. ToString ( ) ) )
                    {
                        iconLocation = buffer;
                        if ( iconLocation != "" )
                            ImageSource = new BitmapImage ( new Uri ( iconLocation ) );
                        NotifyPropertyChanged ( "IconLocation" );
                        NotifyPropertyChanged ( "GetimageLoc" );
                    }

                }


            }
        }

        private ImageSource imageSource;
        public ImageSource ImageSource
        {
            get { return imageSource; }
            set { if ( value == imageSource )return; else imageSource = value; }
        }



        // Validates the IconLocation property, updating the errors collection as needed.
        public bool IsIconLocationValid ( string value )
        {

            bool isValid = true;
            bool IsSuportedExtension = false;
            if ( string. IsNullOrWhiteSpace ( value ) )
            {

                RemoveError ( "IconLocation", ERROR_IconLocation_DoesNotExist );
                RemoveError ( "IconLocation", ERROR_IconLocation_NotSupported );
                return isValid;
            }

            //Check if the user is giving a web uri, or just a local file
            try
            {
                Uri path = new Uri ( value );
                if ( !path. IsLoopback )
                { //it is a weblink, now check if its a file


                    //it is a file
                    //does it have the right extension?
                    string fileExstension = System. IO. Path. GetExtension ( path. LocalPath );
                    foreach ( string item in imageExt )
                    {
                        if ( fileExstension. ToLower ( ). Equals ( item, StringComparison. CurrentCultureIgnoreCase ) )
                        {
                            IsSuportedExtension = true;
                            break;
                        }
                    }

                    if ( IsSuportedExtension )
                    {
                        isValid = true;
                        RemoveError ( "IconLocation", ERROR_IconLocation_DoesNotExist );
                        RemoveError ( "IconLocation", ERROR_IconLocation_NotSupported );

                    }
                    else
                    {
                        isValid = false;
                        AddError ( "IconLocation", ERROR_IconLocation_NotSupported, false );
                    }
                    return isValid;
                }
            }
            catch ( Exception )
            {

                //if we fail here we try to handle it as a local path
            }




            if ( System. IO. File. Exists ( value ) )
            {

                //Check to see if the file dropped on the icon lapel
                //is suported by the program



                string fileExstension = System. IO. Path. GetExtension ( value );
                foreach ( string item in imageExt )
                {
                    if ( fileExstension. ToLower ( ). Equals ( item ) )
                    {
                        IsSuportedExtension = true;
                        break;
                    }
                }



                if ( IsSuportedExtension )
                {
                    isValid = true;
                    RemoveError ( "IconLocation", ERROR_IconLocation_DoesNotExist );
                    RemoveError ( "IconLocation", ERROR_IconLocation_NotSupported );

                }
                else
                {
                    isValid = false;
                    AddError ( "IconLocation", ERROR_IconLocation_NotSupported, false );
                }

            }
            else
            {
                isValid = false;
                AddError ( "IconLocation", ERROR_IconLocation_DoesNotExist, false );
            }

            return isValid;
        }
        #endregion

        #region Quicky
        private string quicky;
        public string Quicky
        {
            get
            {
                return this. quicky;
            }
            set
            {


                if ( IsQuickyValid ( value ) )
                {
                    this. quicky = value. ToLower ( ). Trim ( );
                    NotifyPropertyChanged ( "Quicky" );
                }


            }
        }

        public bool IsQuickyValid ( string value )
        {

            if ( value. ToLower ( ). Trim ( ) == this. quicky )
            {
                RemoveError ( "Quicky", ERROR_Quicky_emptey );
                RemoveError ( "Quicky", ERROR_Quicky_duplicate );
                return true;

            }

            bool isvalid = true;
            if ( value. Trim ( ) == "" )
            {

                AddError ( "Quicky", ERROR_Quicky_emptey, false );
                isvalid = false;
            }
            else
            {
                RemoveError ( "Quicky", ERROR_Quicky_emptey );

            }

            if ( RepositoryLibrary. ExistsQuicky ( value ) )
            {

                AddError ( "Quicky", ERROR_Quicky_duplicate, false );
                isvalid = false;
            }
            else
            {
                RemoveError ( "Quicky", ERROR_Quicky_duplicate );

            }
            return isvalid;
        }

        #endregion

        #region alfaname
        public string AlphaName
        {
            get
            {
                return alfaName ( );
            }
        }

        private string alfaName ( )
        {
            string an = this. Quicky. ToLower ( ). Trim ( );
            an = an. Replace ( " ", "" );
            return an;
        }
        #endregion

        #region type
        private CommandType type;
        public CommandType Type
        {
            get
            {
                return this. type;
            }
            set
            {
                if ( value != this. type )
                {
                    this. type = value;
                    NotifyPropertyChanged ( "Type" );
                }
            }
        }



        public RunCommand ( )
        {
            iconLocation = "";
        }
        #endregion




        /// <summary>
        /// Executes a shell command synchronously.
        /// </summary>
        /// <param name="command">string command</param>
        /// <returns>string, as output of the command.</returns>
        public void ExecuteCommandSync ( CommandBox currentBox = null )
        {
            try
            {
                if ( currentBox != null )
                {
                    if ( this. Command. ToUpper ( ) == "QUICKY RELOAD" )
                        restart ( currentBox );
                }

                if ( this. Type == CommandType. SimpleRunCommand )
                    System. Diagnostics. Process. Start ( this. Command );
                else if ( this. Type == CommandType. CMDExecutedCommand )
                    executespecial ( this. Command );
                else if ( this. Type == CommandType. AutoCadCommand )
                    executeAutocadCommand ( this. Command );


            }
            catch ( Exception )
            {
                throw;
            }
        }

        private void restart ( CommandBox currentBox )
        {
            Properties. Settings. Default. Save ( );
            currentBox. Restart ( );
        }

        private void executespecial ( string command )
        {

            System. Diagnostics. ProcessStartInfo procStartInfo =
                new System. Diagnostics. ProcessStartInfo ( command );
            procStartInfo. ErrorDialog = true;
            procStartInfo. CreateNoWindow = true;
            procStartInfo. Arguments = this. arguments;
            System. Diagnostics. Process proc = new System. Diagnostics. Process ( );
            proc. StartInfo = procStartInfo;
            proc. Start ( );
        }

        private void executeAutocadCommand ( string command )
        {
            //Interop.AutoCad_Connection cmd = new Interop.AutoCad_Connection();
            //cmd.SendCommand(command.Replace("&", " "));

        }

        private string relocateIconImage ( string value )
        {
            return Objects. MyImageWorker. RelocateIconImage ( value );
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged ( String info )
        {
            if ( PropertyChanged != null )
            {
                PropertyChanged ( this, new PropertyChangedEventArgs ( info ) );
            }
        }

        #endregion


        public static RunCommand CreateCommandFromLnk ( string s )
        {
            ShellLink link = null;
            try
            {

                #region Read out the LNK file
                //read out shortcut properties
                link = new ShellLink ( s );
                string tempPath = System. IO. Path. GetTempPath ( ) + System. IO. Path. GetFileNameWithoutExtension ( link. ShortCutFile ) + ".png";
                string quickyName = System. IO. Path. GetFileNameWithoutExtension ( s );

                //Before adding check if name exists
                int counter = 0;
                string dummyName = quickyName;
                //make sure the name is unique!
                if ( RepositoryLibrary. ExistsQuicky ( quickyName ) )
                {
                    do
                    {
                        quickyName = dummyName + "(" + counter++. ToString ( ) + ")";
                    } while ( RepositoryLibrary. ExistsQuicky ( quickyName ) );
                }




            labelXx1:
                try
                {
                    File. Delete ( tempPath ); //clean file if it already exists
                }
                catch ( Exception )
                {
                    //File is prob in use.. increment file name with datetimenow
                    tempPath = System. IO. Path. GetTempPath ( ) + System. IO. Path. GetFileNameWithoutExtension ( link. ShortCutFile ) + DateTime. Now. Millisecond. ToString ( ) + ".png";
                    goto labelXx1;
                }


                //get icon and save it to temp location
                link. LargeIcon. ToBitmap ( ). Save ( tempPath, System. Drawing. Imaging. ImageFormat. Png );


                RunCommand tmp = new RunCommand
                        {
                            IconLocation = tempPath,
                            Command = link. Target,
                            Quicky = quickyName,
                            Arguments = link. Arguments,
                            Desctiption = link. Description
                        };

                return tmp;



            }
            catch ( Exception )
            {
                return null;
                throw;

            }

            finally
            {
                if ( link != null )
                    link. Dispose ( );
            }
                #endregion
        }
        public static RunCommand CreateCommandFromstring ( string path )
        {
            RunCommand returnCommand = null;
            try
            {
                FileInfo file = new FileInfo ( path );
                string tempPath = System. IO. Path. GetTempPath ( ) + System. IO. Path. GetFileNameWithoutExtension ( path ) + ".png";
                string quickyName = System. IO. Path. GetFileNameWithoutExtension ( path );

                //Before adding check if name exists
                int counter = 0;
                string dummyName = quickyName;
                //make sure the name is unique!
                if ( RepositoryLibrary. ExistsQuicky ( quickyName ) )
                {
                    do
                    {
                        quickyName = dummyName + "(" + counter++. ToString ( ) + ")";
                    } while ( RepositoryLibrary. ExistsQuicky ( quickyName ) );
                }

                FileIcon fi = null;
                try
                {
                    fi = new FileIcon ( path );

                labelXx2:
                    try
                    {
                        File. Delete ( tempPath ); //clean file if it already exists
                    }
                    catch ( Exception )
                    {
                        //File is prob in use.. increment file name with datetimenow
                        tempPath = System. IO. Path. GetTempPath ( ) + System. IO. Path. GetFileNameWithoutExtension ( path ) + DateTime. Now. Millisecond. ToString ( ) + ".png";
                        goto labelXx2;
                    }

                    //get icon and save it to temp location
                    fi. ShellIcon. ToBitmap ( ). Save ( tempPath, System. Drawing. Imaging. ImageFormat. Png );

                }
                catch ( Exception )
                {

                    //if it did not succeed do nothing, this is only for icon so null it
                    tempPath = "";
                }
                finally
                {
                    fi. ShellIcon. Dispose ( );
                }


                returnCommand = new RunCommand
                                             {
                                                 IconLocation = tempPath,
                                                 Command = path,
                                                 Quicky = quickyName,
                                                 Arguments = string. Empty,
                                                 Desctiption = string. Empty
                                             };
                return returnCommand;
            }
            catch ( Exception )
            {
                return returnCommand;
                throw;
            }
            finally
            {

            }

        }

        #region IDataErrorInfo Members
        private Dictionary<String, List<String>> errors =
          new Dictionary<string, List<string>> ( );
        private const string ERROR_Quicky_duplicate = "A Quicky command already exists in the library with the same name, please only use unique named Quicky's.";
        private const string ERROR_IconLocation_DoesNotExist = "The file you specified, could not be found...";
        private const string ERROR_IconLocation_NotSupported = "The file you specified is not suported.";
        private const string ERROR_Quicky_emptey = "The quicky command has to be provided";

        public string Error
        {
            get
            {
                StringBuilder bd = new StringBuilder ( );
                foreach ( List<string> l_s in errors. Values )
                {
                    foreach ( string s in l_s )
                    {
                        bd. Append ( s );
                    }
                }
                return bd. ToString ( );
            }
        }

        public string this [ string propertyName ]
        {
            get
            {
                return ( !errors. ContainsKey ( propertyName ) ? null :
                    String. Join ( Environment. NewLine, errors [ propertyName ] ) );
            }
        }
        // Adds the specified error to the errors collection if it is not already 
        // present, inserting it in the first position if isWarning is false. 
        public void AddError ( string propertyName, string error, bool isWarning )
        {
            if ( !errors. ContainsKey ( propertyName ) )
            {
                errors [ propertyName ] = new List<string> ( );
                NotifyPropertyChanged ( "Error" );
            }
            if ( !errors [ propertyName ]. Contains ( error ) )
            {
                if ( isWarning )
                    errors [ propertyName ]. Add ( error );
                else
                {
                    errors [ propertyName ]. Insert ( 0, error );
                    NotifyPropertyChanged ( "Error" );
                }
            }


        }

        // Removes the specified error from the errors collection if it is present. 
        public void RemoveError ( string propertyName, string error )
        {
            if ( errors. ContainsKey ( propertyName ) &&
                errors [ propertyName ]. Contains ( error ) )
            {
                errors [ propertyName ]. Remove ( error );
                if ( errors [ propertyName ]. Count == 0 )
                    errors. Remove ( propertyName );

                NotifyPropertyChanged ( "Error" );
            }
        }

        #endregion
    }
    public enum CommandType
    {
        SimpleRunCommand,
        CMDExecutedCommand,
        AutoCadCommand
    }
}
