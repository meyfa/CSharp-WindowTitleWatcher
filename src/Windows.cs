using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WindowTitleWatcher
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
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        #endregion

        /// <summary>
        /// Provides access to a window's information.
        /// </summary>
        public class WindowInfo
        {
            public readonly IntPtr Handle;

            public int ProcessId
            {
                get
                {
                    uint pid;
                    GetWindowThreadProcessId(Handle, out pid);

                    return (int)pid;
                }
            }
            public string ProcessName
            {
                get
                {
                    return Process.GetProcessById(ProcessId).ProcessName;
                }
            }

            public WindowInfo(IntPtr hWnd)
            {
                Handle = hWnd;
            }
        }

        public delegate bool EnumCallback(WindowInfo windowHandle);

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
        /// Enumerates all VISIBLE windows synchronously and applies the given
        /// callback until it returns false or the end is reached.
        /// </summary>
        /// <param name="callback">The callback to apply to each window.</param>
        public static void ForEachVisible(EnumCallback callback)
        {
            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindow(hWnd) || !IsWindowVisible(hWnd))
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

        /// <summary>
        /// Finds the first out of all VISIBLE windows for which the filter returns true.
        /// </summary>
        /// <param name="filter">The predicate to apply.</param>
        /// <returns>The window or null if none match.</returns>
        public static WindowInfo FindFirstVisible(Predicate<WindowInfo> filter)
        {
            WindowInfo result = null;
            ForEachVisible(window =>
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
    }
}
