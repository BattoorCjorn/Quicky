﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using MessageMapping = System.Collections.Generic.KeyValuePair<StickyWindows.NativeMethods.WindowMessage, StickyWindows.NativeMethods.MessageHandler>;
using System.Drawing;

namespace StickyWindows
{
    public class SnapToBehavior : NativeBehavior
    {
        /// <summary>
        /// Gets the <see cref="MessageMapping"/>s for this behavior:
        /// A single mapping of a handler for WM_WINDOWPOSCHANGING.
        /// </summary>
        /// <returns>A collection of <see cref="MessageMapping"/> objects.</returns>
        public override IEnumerable<MessageMapping> GetHandlers()
        {
            yield return new MessageMapping(NativeMethods.WindowMessage.WindowPositionChanging, OnPreviewPositionChange);
        }
        /// <summary>Handles the WindowPositionChanging Window Message.
        /// </summary>
        /// <param name="wParam">The wParam.</param>
        /// <param name="lParam">The lParam.</param>
        /// <param name="handled">Whether or not this message has been handled ... (we don't change it)</param>
        /// <returns>IntPtr.Zero</returns>      
        private IntPtr OnPreviewPositionChange(IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            bool updated = false;
            var windowPosition = (NativeMethods.WindowPosition)Marshal.PtrToStructure(lParam, typeof(NativeMethods.WindowPosition));

            if ((windowPosition.Flags & NativeMethods.WindowPositionFlags.NoMove) == 0)
            {
                // If we use the WPF SystemParameters, these should be "Logical" pixels
                Rect validArea = new Rect(SystemParameters.VirtualScreenLeft,
                                          SystemParameters.VirtualScreenTop,
                                          SystemParameters.VirtualScreenWidth,
                                          SystemParameters.VirtualScreenHeight);

                Rect snapToBorder = new Rect(SystemParameters.VirtualScreenLeft + SnapDistance.Left,
                                         SystemParameters.VirtualScreenTop + SnapDistance.Top,
                                         SystemParameters.VirtualScreenWidth - (SnapDistance.Left + SnapDistance.Right),
                                         SystemParameters.VirtualScreenHeight - (SnapDistance.Top + SnapDistance.Bottom));

                // Enforce left boundary
                if (windowPosition.Left < snapToBorder.Left)
                {
                    windowPosition.Left = (int)validArea.Left;
                    updated = true;
                }

                // Enforce top boundary
                if (windowPosition.Top < snapToBorder.Y)
                {
                    windowPosition.Top = (int)validArea.Top;
                    updated = true;
                }

                // Enforce right boundary
                if (windowPosition.Right > snapToBorder.Right)
                {
                    windowPosition.Left = (int)(validArea.Right - windowPosition.Width);
                    updated = true;
                }

                // Enforce bottom boundary
                if (windowPosition.Bottom > snapToBorder.Bottom)
                {
                    windowPosition.Top = (int)(validArea.Bottom - windowPosition.Height);
                    updated = true;
                }

                formRect = new Rectangle(windowPosition.Left, windowPosition.Top, windowPosition.Width, windowPosition.Height);

                // clean up inactive windows
                for (int i = 0; i < WindowManager.Windows.Count; i++)
                {
                    if (!WindowManager.Windows[i].IsVisible)
                    {
                        WindowManager.Windows.RemoveAt(i);
                    }
                }


                #region Try to snap with other windows
                foreach (Window sw in WindowManager.Windows)
                {
                    try
                    {
                        

                        
                            formRect = new Rectangle(windowPosition.Left, windowPosition.Top, windowPosition.Width, windowPosition.Height);

                            if (sw != OriginalForm)
                            {
                                formOffsetPoint.X = stickGap + 1;	// (more than) maximum gaps
                                formOffsetPoint.Y = stickGap + 1;


                                //EDIT BEDEVELOPED (If a window got hidden in the progress its width and hight will be inacessable so we need to use actual width, hight witch will be 0,0)
                                //LEGACY: Rectangle rect = new Rectangle(Convert.ToInt32(sw.Left), Convert.ToInt32(sw.Top), Convert.ToInt32(sw.Width), Convert.ToInt32(sw.Height));
                                Rectangle rect = new Rectangle(Convert.ToInt32(sw.Left), Convert.ToInt32(sw.Top), Convert.ToInt32(sw.ActualWidth), Convert.ToInt32(sw.ActualHeight));

                                Move_Stick(rect, true);

                                if (formOffsetPoint.X == stickGap + 1)
                                    formOffsetPoint.X = 0;
                                if (formOffsetPoint.Y == stickGap + 1)
                                    formOffsetPoint.Y = 0;

                                if ((formOffsetPoint.X != stickGap)
                                    && (formOffsetPoint.Y != stickGap))
                                {
                                    windowPosition.Left = windowPosition.Left + formOffsetPoint.X;
                                    windowPosition.Top = windowPosition.Top + formOffsetPoint.Y;
                                }

                                //WindowList[0].Title = "Left:" + windowPosition.Top + "  Top:" + windowPosition.Top;

                                updated = true;
                                // }
                            }
                       
                    }
                    catch (Exception)
                    {
                        throw;
                        //  we should Not get here...
                    }

                }
                #endregion


            }
            if (updated)
            {
                Marshal.StructureToPtr(windowPosition, lParam, true);
            }

            return IntPtr.Zero;
        }

        private Window originalForm;		// the form

        public Window OriginalForm
        {
            get
            {
                return originalForm;
            }
            set
            {
                originalForm = value;
            }
        }
        private Rectangle formRect;			// form bounds


        private static int stickGap = 20;		// distance to stick

        private System.Drawing.Point formOffsetPoint;	// calculated offset rect to be added !! (min distances in all directions!!)


        private void Move_Stick(Rectangle toRect, bool bInsideStick)
        {
            // compare distance from toRect to formRect
            // and then with the found distances, compare the most closed position
            if (formRect.Bottom >= (toRect.Top - stickGap) && formRect.Top <= (toRect.Bottom + stickGap))
            {
                if (bInsideStick)
                {
                    if ((Math.Abs(formRect.Left - toRect.Right) <= Math.Abs(formOffsetPoint.X)))
                    {	// left 2 right
                        formOffsetPoint.X = toRect.Right - formRect.Left;
                    }
                    if ((Math.Abs(formRect.Left + formRect.Width - toRect.Left) <= Math.Abs(formOffsetPoint.X)))
                    {	// right 2 left
                        formOffsetPoint.X = toRect.Left - formRect.Width - formRect.Left;
                    }
                }

                if (Math.Abs(formRect.Left - toRect.Left) <= Math.Abs(formOffsetPoint.X))
                {	// snap left 2 left
                    formOffsetPoint.X = toRect.Left - formRect.Left;
                }
                if (Math.Abs(formRect.Left + formRect.Width - toRect.Left - toRect.Width) <= Math.Abs(formOffsetPoint.X))
                {	// snap right 2 right
                    formOffsetPoint.X = toRect.Left + toRect.Width - formRect.Width - formRect.Left;
                }
            }
            if (formRect.Right >= (toRect.Left - stickGap) && formRect.Left <= (toRect.Right + stickGap))
            {
                if (bInsideStick)
                {
                    if (Math.Abs(formRect.Top - toRect.Bottom) <= Math.Abs(formOffsetPoint.Y) && bInsideStick)
                    {	// Stick Top to Bottom
                        formOffsetPoint.Y = toRect.Bottom - formRect.Top;
                    }
                    if (Math.Abs(formRect.Top + formRect.Height - toRect.Top) <= Math.Abs(formOffsetPoint.Y) && bInsideStick)
                    {	// snap Bottom to Top
                        formOffsetPoint.Y = toRect.Top - formRect.Height - formRect.Top;
                    }
                }

                // try to snap top 2 top also
                if (Math.Abs(formRect.Top - toRect.Top) <= Math.Abs(formOffsetPoint.Y))
                {	// top 2 top
                    formOffsetPoint.Y = toRect.Top - formRect.Top;
                }
                if (Math.Abs(formRect.Top + formRect.Height - toRect.Top - toRect.Height) <= Math.Abs(formOffsetPoint.Y))
                {	// bottom 2 bottom
                    formOffsetPoint.Y = toRect.Top + toRect.Height - formRect.Height - formRect.Top;
                }
            }
        }


        #region Additional Dependency Properties
        /// <summary>
        /// The DependencyProperty as the backing store for SnapDistance. <remarks>Just you can set it from XAML.</remarks>
        /// </summary>
        public static readonly DependencyProperty SnapDistanceProperty =
            DependencyProperty.Register("SnapDistance", typeof(Thickness), typeof(SnapToBehavior), new UIPropertyMetadata(new Thickness(20)));

        public static readonly DependencyProperty WindowListProperty =
                    DependencyProperty.Register("WindowList", typeof(List<Window>), typeof(SnapToBehavior), new UIPropertyMetadata(new List<Window>()));


        public List<Window> WindowList
        {
            get
            {
                if (WindowListProperty == null)
                    SetValue(WindowListProperty, new List<Window>());

                return (List<Window>)GetValue(WindowListProperty);
            }

            set
            {
                SetValue(WindowListProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the snap distance.
        /// </summary>
        /// <value>The snap distance.</value>
        public Thickness SnapDistance
        {
            get
            {
                return (Thickness)GetValue(SnapDistanceProperty);
            }
            set
            {
                SetValue(SnapDistanceProperty, value);
            }
        }
        #endregion Additional Dependency Properties
    }
}
