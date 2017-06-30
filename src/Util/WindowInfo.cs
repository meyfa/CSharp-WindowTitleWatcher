using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

        #endregion

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
        
        public bool IsVisible
        {
            get
            {
                return IsWindowVisible(Handle);
            }
        }

        public WindowInfo(IntPtr hWnd)
        {
            Handle = hWnd;
        }
    }
}
