using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace WindowTitleWatcher
{
    public class Watcher : IDisposable
    {
        #region imports

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        protected static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        protected static extern int GetWindowTextLength(IntPtr hWnd);

        #endregion

        /// <summary>
        /// Gets the current window title.
        /// </summary>
        public string Title
        {
            get
            {
                return mTitle;
            }
        }

        public event EventHandler TitleChanged;

        private readonly IntPtr windowHandle;
        private bool isRunning = true;
        private string mTitle;

        public Watcher(Process proc)
            : this(proc.MainWindowHandle)
        {
        }

        public Watcher(IntPtr windowHandle)
        {
            this.windowHandle = windowHandle;

            Poll();

            new Thread(() =>
            {
                while (isRunning)
                {
                    Thread.Sleep(10);
                    Poll();
                }
            }).Start();
        }

        public void Dispose()
        {
            isRunning = false;
        }

        private void Poll()
        {
            int size = GetWindowTextLength(windowHandle);

            StringBuilder sb = new StringBuilder(size + 1);
            GetWindowText(windowHandle, sb, sb.Capacity);

            string title = sb.ToString();
            if (title != mTitle)
            {
                mTitle = title;
                TitleChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
