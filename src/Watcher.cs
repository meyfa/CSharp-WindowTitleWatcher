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

        [DllImport("user32.dll")]
        static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        protected static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        protected static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        protected static extern int GetWindowTextLength(IntPtr hWnd);

        #endregion

        /// <summary>
        /// Returns whether the window has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns whether the window is currently visible.
        /// </summary>
        public bool IsVisible
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current window title.
        /// </summary>
        public string Title
        {
            get;
            private set;
        }

        public event EventHandler<TitleEventArgs> TitleChanged;
        public event EventHandler VisibilityChanged;
        public event EventHandler Disposed;

        private readonly IntPtr windowHandle;
        private bool isRunning = true;

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
            // check whether disposed
            if (!IsWindow(windowHandle))
            {
                isRunning = false;

                IsDisposed = true;
                if (IsVisible)
                {
                    IsVisible = false;
                    VisibilityChanged?.Invoke(this, EventArgs.Empty);
                }

                Disposed?.Invoke(this, EventArgs.Empty);

                return;
            }

            // update visibility
            bool newVisibility = IsWindowVisible(windowHandle);
            if (newVisibility != IsVisible)
            {
                IsVisible = newVisibility;
                VisibilityChanged?.Invoke(this, EventArgs.Empty);
            }

            // update title
            int size = GetWindowTextLength(windowHandle);

            StringBuilder sb = new StringBuilder(size + 1);
            GetWindowText(windowHandle, sb, sb.Capacity);

            string prevTitle = Title;
            string newTitle = sb.ToString();
            if (newTitle != prevTitle)
            {
                Title = newTitle;
                TitleChanged?.Invoke(this, new TitleEventArgs(prevTitle, newTitle));
            }
        }
    }
}
