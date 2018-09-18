using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using WindowTitleWatcher.Internal;

namespace WindowTitleWatcher.Util
{
    /// <summary>
    /// Provides access to a window's information.
    /// </summary>
    public class WindowInfo
    {
        #region imports

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        #endregion imports

        public readonly IntPtr Handle;
        private readonly WindowPoller Poller;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

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

        public bool IsVisible
        {
            get
            {
                return Poller.Poll().IsVisible;
            }
        }

        public string Title
        {
            get
            {
                return Poller.Poll().Title;
            }
        }

        public WindowInfo(IntPtr hWnd)
        {
            Handle = hWnd;
            Poller = new WindowPoller(hWnd);
        }

        /// <summary>
        /// Retrieves the location and size of this window.
        /// </summary>
        /// <returns></returns>
        public Rectangle GetRectangle()
        {
            var rct = new RECT();
            if (!GetWindowRect(Handle, ref rct))
            {
                return Rectangle.Empty;
            }

            return new Rectangle()
            {
                X = rct.Left,
                Y = rct.Top,
                Width = rct.Right - rct.Left,
                Height = rct.Bottom - rct.Top,
            };
        }
    }
}
