using System;
using System. Collections. Generic;
using System. Linq;
using System. Text;
using System. Collections. ObjectModel;
using System. ComponentModel;

namespace quicky. Objects
{
    public class RepositoryLibrary
    {

        private static ObservableCollection<CommandRepository> repositorys = new ObservableCollection<CommandRepository> ( );

        public static ObservableCollection<CommandRepository> Repositorys
        {
            get
            {
                return repositorys;
            }
            set
            {

                repositorys = value;
            }
        }



        public static bool ExistsQuicky ( string Quicky )
        {
            if ( Quicky == null )
            {
                return true;
            }
            foreach ( CommandRepository rep in repositorys )
            {

                foreach ( RunCommand rc in rep. Commands )
                {
                    if ( rc. Quicky. ToLower ( ). Equals ( Quicky. ToLower ( ). Trim ( ) ) )
                    {
                        return true;
                    }

                }
            }
            return false;


        }


        internal static void LoadRepositorys ( )
        {
            System. IO. DirectoryInfo myDirectory = appDirectory ( );

            List<System. IO. FileInfo> xmlFiles = myDirectory. GetFiles ( "*.xml", System. IO. SearchOption. TopDirectoryOnly ). ToList ( );

            xmlFiles. Sort ( ( x, y ) => string. Compare ( x. Name, y. Name ) );

            repositorys = new ObservableCollection<CommandRepository> ( );

            foreach ( System. IO. FileInfo file in xmlFiles )
            {
                CommandRepository repository = new CommandRepository ( );
                repository. Load ( file. FullName );
                repositorys. Add ( repository );
            }



        }

        internal static void SaveRepositorys ( )
        {
            System. IO. DirectoryInfo myDirectory = appDirectory ( );

            foreach ( CommandRepository file in repositorys )
            {
                CommandRepository repository = new CommandRepository ( );
                repository. Save ( );
                repositorys. Add ( repository );
            }
        }

        protected static System. IO. DirectoryInfo appDirectory ( )
        {
            System. IO. DirectoryInfo myDirectory = new System. IO. DirectoryInfo ( System. Windows. Forms. Application. UserAppDataPath );
            if ( !myDirectory. Exists )
            {
                myDirectory. Create ( );
            }
            return myDirectory;
        }

        internal static void AddRepository ( string name )
        {
            repositorys. Add ( new CommandRepository ( name ) );
        }

        internal static void RemoveRepository ( string name )
        {
            foreach ( CommandRepository repos in repositorys )
            {
                if ( repos. Name. Equals ( name, StringComparison. CurrentCultureIgnoreCase ) )
                {
                    repos. DeleteXml ( );
                    Repositorys. Remove ( repos );
                    //if repository is removed, remove the xaml file aswell!!!

                    return;
                }
            }
        }

        internal static bool ExistsNameRepository ( string Name )
        {
            foreach ( CommandRepository item in repositorys )
            {
                if ( item. Name. Equals ( Name, StringComparison. CurrentCultureIgnoreCase ) )
                    return true;
            }
            return false;
        }

        internal static bool ExistsIconLocationRepository ( string iconLocation )
        {
            foreach ( CommandRepository item in repositorys )
            {
                foreach ( RunCommand cmd in item. Commands )
                {
                    if ( cmd. IconLocation. Equals ( iconLocation, StringComparison. CurrentCultureIgnoreCase ) )
                        return true;
                }

            }
            return false;
        }
    }
}
