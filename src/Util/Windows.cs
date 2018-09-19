using System;
using System.Runtime.InteropServices;

namespace WindowTitleWatcher.Util
{
    /// <summary>
    /// Utility class for enumerating and finding specific windows to be used
    /// in Watcher instances.
    /// </summary>
    public class Windows
    {
        #region imports
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, RedrawWindowFlags flags);
        #endregion imports

        [Flags()]
        private enum RedrawWindowFlags : uint
        {
            /// <summary>
            /// Invalidates the rectangle or region that you specify in lprcUpdate or hrgnUpdate.
            /// You can set only one of these parameters to a non-NULL value. If both are NULL, RDW_INVALIDATE invalidates the entire window.
            /// </summary>
            Invalidate = 0x1,

            /// <summary>Causes the OS to post a WM_PAINT message to the window regardless of whether a portion of the window is invalid.</summary>
            InternalPaint = 0x2,

            /// <summary>
            /// Causes the window to receive a WM_ERASEBKGND message when the window is repainted.
            /// Specify this value in combination with the RDW_INVALIDATE value; otherwise, RDW_ERASE has no effect.
            /// </summary>
            Erase = 0x4,

            /// <summary>
            /// Validates the rectangle or region that you specify in lprcUpdate or hrgnUpdate.
            /// You can set only one of these parameters to a non-NULL value. If both are NULL, RDW_VALIDATE validates the entire window.
            /// This value does not affect internal WM_PAINT messages.
            /// </summary>
            Validate = 0x8,

            NoInternalPaint = 0x10,

            /// <summary>Suppresses any pending WM_ERASEBKGND messages.</summary>
            NoErase = 0x20,

            /// <summary>Excludes child windows, if any, from the repainting operation.</summary>
            NoChildren = 0x40,

            /// <summary>Includes child windows, if any, in the repainting operation.</summary>
            AllChildren = 0x80,

            /// <summary>Causes the affected windows, which you specify by setting the RDW_ALLCHILDREN and RDW_NOCHILDREN values, to receive WM_ERASEBKGND and WM_PAINT messages before the RedrawWindow returns, if necessary.</summary>
            UpdateNow = 0x100,

            /// <summary>
            /// Causes the affected windows, which you specify by setting the RDW_ALLCHILDREN and RDW_NOCHILDREN values, to receive WM_ERASEBKGND messages before RedrawWindow returns, if necessary.
            /// The affected windows receive WM_PAINT messages at the ordinary time.
            /// </summary>
            EraseNow = 0x200,

            Frame = 0x400,

            NoFrame = 0x800
        }

        public delegate bool EnumCallback(WindowInfo windowHandle);

        private enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1,
            ShowMinimized = 2,
            ShowMaximized = 3,
            Maximize = 3,
            ShowNormalNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActivate = 7,
            ShowNoActivate = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimized = 11,
        };

        /// <summary>
        /// Enumerates all windows synchronously and applies the given callback
        /// until it returns false or the end is reached.
        /// </summary>
        /// <param name="callback">The callback to apply to each window.</param>
        public static void ForEach(EnumCallback callback)
        {
            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindow(hWnd))
                {
                    // skip this
                    return true;
                }
                return callback(new WindowInfo(hWnd));
            }, IntPtr.Zero);
        }

        /// <summary>
        /// Finds the first out of all windows for which the filter returns true.
        /// </summary>
        /// <param name="filter">The predicate to apply.</param>
        /// <returns>The window or null if none match.</returns>
        public static WindowInfo FindFirst(Predicate<WindowInfo> filter)
        {
            WindowInfo result = null;
            ForEach(window =>
            {
                if (filter(window))
                {
                    result = window;
                    return false;
                }
                return true;
            });
            return result;
        }

        public static bool Activate(WindowInfo win)
        {
            return SetForegroundWindow(win.Handle);
        }

        /// <summary>
        /// Returns true if the window had been minimized and was restored,
        /// false if the window was already restored, and null if it failed.
        /// </summary>
        /// <param name="win"></param>
        /// <returns></returns>
        public static bool? RestoreWindow(WindowInfo win)
        {
            if (!IsIconic(win.Handle))
            {
                return false;
            }

            var result = ShowWindow(win.Handle, ShowWindowEnum.Restore);
            if (result)
            {
                return true;
            }

            // TODO: This bit doesn't actually repaint the window; we probably need to wait for some events before, but I don't have the time to investigate this now.
            UpdateWindow(win.Handle);
            RedrawWindow(win.Handle, IntPtr.Zero, IntPtr.Zero, RedrawWindowFlags.Frame | RedrawWindowFlags.UpdateNow | RedrawWindowFlags.Invalidate);
            return null;
        }
    }
}
