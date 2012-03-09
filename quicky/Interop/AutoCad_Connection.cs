//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Autodesk.AutoCAD.Interop;
//using System.Runtime.InteropServices;

//namespace quicky.Interop
//{


//    class AutoCad_Connection
//    {
//        private static string sAcadID = "AutoCAD.Application";
//        public static AcadApplication acAppComObj = null;

//        //Import the SetForeground API to activate it
//        [DllImportAttribute("User32.dll")]
//        private static extern IntPtr SetForegroundWindow(int hWnd);

//        public AcadApplication ConnectToAcad()
//        {
//            // Get a running instance of AutoCAD
//            try
//            {

//                acAppComObj = (AcadApplication)Marshal.GetActiveObject(sAcadID);
//                return acAppComObj;
//            }
//            catch // An error occurs if no instance is running
//            {
//                MyMessageBox.SendMessage("An instance of autocad is not found.", "Application error");
//                return null;
//            }

//            // Display the application
//            finally
//            {
//                if (acAppComObj != null)
//                {
//                    acAppComObj.Visible = true;

//                    if (acAppComObj.WindowState == Autodesk.AutoCAD.Interop.Common.AcWindowState.acNorm)
//                        acAppComObj.WindowState = Autodesk.AutoCAD.Interop.Common.AcWindowState.acNorm;
//                    else
//                        acAppComObj.WindowState = Autodesk.AutoCAD.Interop.Common.AcWindowState.acMax;

//                    SetForegroundWindow(acAppComObj.HWND); //Activate it
//                }
//            }

//        }

//        public bool IsValidConection()
//        {
//            try
//            {

//                acAppComObj = (AcadApplication)Marshal.GetActiveObject(sAcadID);
//                return true;
//            }
//            catch // An error occurs if no instance is running
//            {
//                return false;
//            }
//        }

//        public void SendCommand(string command)
//        {
//            // Get the active document
//            AcadApplication acAppComObj = ConnectToAcad();
//            if (acAppComObj != null)
//            {

//                acAppComObj.ActiveDocument.SendCommand(command);
//            }
//        }

//        public void SendMessage(string message)
//        {
//            // Get the active document
//            AcadApplication acAppComObj = ConnectToAcad();
//            if (acAppComObj != null)
//            {
//                AcadDocument doc = acAppComObj.ActiveDocument;

//                doc.Utility.Prompt(message + Environment.NewLine);
//            }
//        }

//        public string GetString(string message)
//        {
//            // Get the active document
//            AcadApplication acAppComObj = ConnectToAcad();
//            if (acAppComObj != null)
//            {
//                AcadDocument doc = acAppComObj.ActiveDocument;

//                return doc.Utility.GetString(0, message + ": ");

//            }

//            return "Failed";
//        }



//    }


//}
