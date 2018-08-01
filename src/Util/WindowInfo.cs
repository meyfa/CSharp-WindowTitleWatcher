using System;
using System.Diagnostics;
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

        #endregion

        public readonly IntPtr Handle;
        private readonly WindowPoller Poller;

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
    }
}
