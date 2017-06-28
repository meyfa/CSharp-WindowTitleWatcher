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

        #endregion

        public readonly IntPtr Handle;

        public int ProcessId
        {
            get
            {
                GetWindowThreadProcessId(Handle, out uint pid);
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
}
