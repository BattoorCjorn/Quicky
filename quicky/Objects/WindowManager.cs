using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace StickyWindows
{
    public static class WindowManager
    {

        static WindowManager()
        {
            m_Windows = new List<Window>();
        }

        private static List<Window> m_Windows;

        public static List<Window> Windows
        {
            get { return m_Windows; }
        }

        public static void RegisterWindow(Window window)
        {
            m_Windows.Add(window);
        }

        public static void UnregisterWindow(Window window)
        {
            m_Windows.Remove(window);
        }
    }
}
