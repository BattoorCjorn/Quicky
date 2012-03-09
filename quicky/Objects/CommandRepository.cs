using System;
using System. Collections. Generic;
using System. Linq;
using System. Text;
using System. Windows;
using System. IO;
using System. Xml;
using System. Xml. Schema;
using System. Data;
using System. ComponentModel;
using System. Collections. ObjectModel;
using System. Threading. Tasks;

namespace quicky. Objects
{

    public class CommandRepository: INotifyPropertyChanged
    {
        public CommandRepository ( string name = "" )
        {
            this. Name = name;
        }

        public string Name
        {
            get;
            set;
        }


        public string FileXMLpath
        {
            get
            {
                return System. Windows. Forms. Application. UserAppDataPath + @"\" + this. Name + ".xml";
            }
        }

        public string DisplayName
        {
            get
            {
                return this. Name;
            }
        }

        private ObservableCollection<RunCommand> commands = new ObservableCollection<RunCommand> ( );
        public ObservableCollection<RunCommand> Commands
        {
            get
            {
                return commands;
            }
            set
            {
                NotifyPropertyChanged ( "Commands" );
                commands = value;

            }
        }

        public CommandType CommandType
        {
            get
            {
                return getTypeOutName ( this. Name );
            }
        }

        public bool Save ( string fileName = null )
        {
            try
            {
                if ( string. IsNullOrWhiteSpace ( fileName ) )
                {
                    saveRuncommandsXML ( );

                }
                else
                {

                    saveRuncommandsXML ( fileName );


                }

                return true;
            }
            catch ( Exception )
            {
                return false;
                throw;
            }
        }

        public bool Load ( string fileName = null )
        {
            try
            {
                if ( string. IsNullOrWhiteSpace ( fileName ) )
                {
                    loadRuncommandsXML ( );

                }
                else
                {
                    if ( System. IO. File. Exists ( fileName ) )
                    {
                        loadRuncommandsXML ( fileName );

                    }
                }

                return true;
            }
            catch ( Exception )
            {
                return false;
                throw;
            }
        }


        private CommandType getTypeOutName ( string name )
        {

            string x = name. Substring ( name. LastIndexOf ( "[" ) + 1, 1 );

            switch ( x )
            {
                case "1":
                    {
                        return Objects. CommandType. CMDExecutedCommand;
                    }
                case "0":
                    {
                        return Objects. CommandType. SimpleRunCommand;
                    }
                default:
                    {
                        return Objects. CommandType. SimpleRunCommand;
                    }
            }

        }

        private void saveRuncommandsXML ( string pathTofile = "" )
        {
            if ( pathTofile == "" )
                pathTofile = this. FileXMLpath;

            File. Delete ( pathTofile );

            try
            {
                using ( XmlTextWriter myXmlTextWriter = new XmlTextWriter ( pathTofile, null ) )
                {
                    myXmlTextWriter. Formatting = Formatting. Indented;
                    myXmlTextWriter. WriteStartDocument ( false );
                    myXmlTextWriter. WriteDocType ( "CommandRepository", null, null, null );
                    myXmlTextWriter. WriteComment ( "This file represents a repository of Quicky Runcommands" );
                    myXmlTextWriter. WriteStartElement ( "CommandRepository" );
                    myXmlTextWriter. WriteAttributeString ( "xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance" );
                    myXmlTextWriter. WriteAttributeString ( "xsi:noNamespaceSchemaLocation", "CommandRepository.xsd" );
                    myXmlTextWriter. WriteAttributeString ( "Name", this. Name );

                    foreach ( RunCommand rc in this. Commands )
                    {
                        myXmlTextWriter. WriteStartElement ( "RunCommand", null );
                        myXmlTextWriter. WriteAttributeString ( "Quicky", rc. Quicky );
                        myXmlTextWriter. WriteAttributeString ( "Type", ( ( int ) rc. Type ). ToString ( ) );
                        myXmlTextWriter. WriteAttributeString ( "Description", rc. Desctiption );
                        myXmlTextWriter. WriteAttributeString ( "Command", rc. Command );
                        myXmlTextWriter. WriteAttributeString ( "Arguments", rc. Arguments );
                        myXmlTextWriter. WriteAttributeString ( "IconLocation", rc. IconLocation );
                        myXmlTextWriter. WriteEndElement ( );
                    }
                    myXmlTextWriter. WriteEndElement ( );

                    //Write the XML to file and close the writer xsi:noNamespaceSchemaLocation
                    myXmlTextWriter. Flush ( );
                    myXmlTextWriter. Close ( );
                }
            }
            catch ( Exception )
            {
                throw;
            }
        }

        private void loadRuncommandsXML ( string pathTofile = "" )
        {
            if ( pathTofile == "" )
                pathTofile = this. FileXMLpath;

            this. Commands = new ObservableCollection<RunCommand> ( );
            DataSet ds = new DataSet ( );

            try
            {
                ds. ReadXml ( pathTofile );


                foreach ( DataRow dr in ds. Tables [ "CommandRepository" ]. Rows )
                {
                    this. Name = dr [ "Name" ]. ToString ( );
                }

                foreach ( DataRow dr in ds. Tables [ "RunCommand" ]. Rows )
                {
                    //try get type

                    this. Commands. Add ( new RunCommand
                    {
                        Command = dr [ "Command" ]. ToString ( ),
                        Arguments = dr [ "Arguments" ]. ToString ( ),
                        Desctiption = dr [ "Description" ]. ToString ( ),
                        Quicky = dr [ "Quicky" ]. ToString ( ),
                        IconLocation = dr [ "IconLocation" ]. ToString ( ),
                        Type = getType ( dr [ "Type" ]. ToString ( ) )
                    } );
                }


            }
            catch ( Exception )
            {
                throw;
            }
        }

        public void DeleteXml ( )
        {
            File. Delete ( this. FileXMLpath );
        }

        private CommandType getType ( string integer )
        {
            switch ( int. Parse ( integer ) )
            {
                case 0:
                    return Objects. CommandType. SimpleRunCommand;
                case 1:
                    return Objects. CommandType. CMDExecutedCommand;
                case 2:
                    return Objects. CommandType. AutoCadCommand;
                default:
                    return Objects. CommandType. SimpleRunCommand;
            }
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
    }


}
