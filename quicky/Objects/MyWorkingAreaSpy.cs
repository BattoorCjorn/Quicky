using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using quicky.Addon;
using System.Windows;

namespace quicky.Objects
{
    public class MyWorkingAreaSpy
    {


        private List<taskbarDummy> myList;

        public  void CloseWindows()
        {
            foreach (Window w in myList)
            {
                w.Close();
            }
        }

        public List<taskbarDummy> RegisterTaskBarDummys()
        {
            /*The taskbar can be on any edge of the screen, so we need to find te bigest rectangle on
            * the screen [this is not always the taskbar as othe bars can be created on a windows desktop]
            * though we will make a taskbarDummy for every window.
            */

            //Get true dim of screen
            double primaryHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double primarywidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            Rect workArea = System.Windows.SystemParameters.WorkArea;

            if (workArea.Height > primaryHeight)
            { workArea.Height = primaryHeight; }
            if (workArea.Width > primarywidth)
            { workArea.Width = primarywidth; }



            //Get points from working area and try to calculate 4 rectangles (left, right, bottom, upper -rec)
            Rect left = new Rect(new Point(0, workArea.Top), new Point(workArea.Left, primaryHeight));

            Rect right = new Rect(new Point(workArea.Right, 0), new Point(primarywidth, primaryHeight));

            Rect bottom = new Rect(new Point(0, workArea.Bottom), new Point(primarywidth, primaryHeight));

            Rect upper = new Rect(new Point(0, 0), new Point(primarywidth, workArea.Top));

            myList = new List<taskbarDummy>();
            //Create taskbarDummys
            if (bottom.Height > 10 && bottom.Width > 10)
            {
                taskbarDummy bottomtaskbarDummy = new taskbarDummy(new Point(bottom.Left, bottom.Top), bottom.Size);
                myList.Add(bottomtaskbarDummy);
            }
            if (left.Width > 10 && left.Height > 10)
            {
                taskbarDummy lefttaskbarDummy = new taskbarDummy(new Point(left.Left, left.Top), left.Size);
                myList.Add(lefttaskbarDummy);
            }
            if (right.Width > 10 && right.Height > 10)
            {
                taskbarDummy righttaskbarDummy = new taskbarDummy(new Point(right.Left, right.Top), right.Size);
                myList.Add(righttaskbarDummy);
            }
            if (upper.Height > 10 && upper.Width > 10)
            {
                taskbarDummy uppertaskbarDummy = new taskbarDummy(new Point(upper.Left, upper.Top), upper.Size);
                myList.Add(uppertaskbarDummy);
            }
            return myList;
        }

       
    }
}
