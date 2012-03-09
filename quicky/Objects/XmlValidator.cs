using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using System.Xml;

namespace quicky.Addon
{
    public class XMLValidator
    {

        public List<string> getErrorsMessages()
        {
            return ErrorMessage;
        }

        // Validation Error Message
        static List<string> ErrorMessage = new List<string>();

        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    ErrorMessage.Add("Error: " + e.Message);
                    break;
                case XmlSeverityType.Warning:
                    ErrorMessage.Add("Warning: " + e.Message);
                    break;
            }
        }

        public bool Validate(string XMLDoc, string schema)
        {
            try
            {
                ErrorMessage.Clear();

                XmlDocument tmpDoc = new XmlDocument();
                tmpDoc.Load(XMLDoc);
                tmpDoc.Schemas.Add(null, schema);
                ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationEventHandler);
                tmpDoc.Validate(eventHandler);

                if (ErrorMessage.Count != 0)
                    return false;
                else
                    return true;
            }
            catch (Exception)
            {
                
                throw;
            }
            

        }
    }
}
