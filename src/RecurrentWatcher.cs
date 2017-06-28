using System;
using System.Threading;
using WindowTitleWatcher.Internal;
using WindowTitleWatcher.Util;

namespace WindowTitleWatcher
{
    /// <summary>
    /// Watcher that retries finding a window instance after disposal, i.e.
    /// where the window can be closed, the process can even exit, and it will
    /// start notifying again when it reappears.
    /// </summary>
    public class RecurrentWatcher : Watcher, IDisposable
    {
        private readonly Func<IntPtr> handleGetter;

        private WindowPoller poller;
        private WindowPoller.Results lastPoll;
        private bool isRunning = true;

        /// <summary>
        /// Invokes the given <see cref="WindowInfo"/> getter for finding the
        /// window to be watched in the background.
        /// </summary>
        /// <param name="windowGetter">The window getter.</param>
        public RecurrentWatcher(Func<WindowInfo> windowGetter)
            : this(() => windowGetter()?.Handle ?? IntPtr.Zero, false)
        {
        }

        /// <summary>
        /// Invokes the given window handle getter for finding the window to be
        /// watched in the background.
        /// </summary>
        /// <param name="handleGetter">The window getter.</param>
        public RecurrentWatcher(Func<IntPtr> handleGetter)
            : this(handleGetter, false)
        {
        }

        /// <summary>
        /// Invokes the given window handle getter for finding the window to be
        /// watched, optionally keeping the process alive while doing so.
        /// </summary>
        /// <param name="handleGetter">The window getter.</param>
        /// <param name="keepAlive">Whether to keep this process alive.</param>
        public RecurrentWatcher(Func<IntPtr> handleGetter, bool keepAlive)
        {
            this.handleGetter = handleGetter;

            InitPoller();
            Update();

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = !keepAlive;

                while (isRunning)
                {
                    Thread.Sleep(10);
                    Update();
                }
            }).Start();
        }

        /// <summary>
        /// Disposes this watcher (stops any threads used and stops reporting).
        /// </summary>
        public void Dispose()
        {
            isRunning = false;
        }

        private void Update()
        {
            WindowPoller.Results results = poller?.Poll();

            if (results == null || results.IsDisposed)
            {
                // init and try again next time
                InitPoller();
                return;
            }

            WindowPoller.Results prev = lastPoll;
            lastPoll = results;

            IsVisible = results.IsVisible;
            Title = results.Title;

            if (prev != null && results.IsVisible != prev.IsVisible)
            {
                RaiseVisibilityChanged(EventArgs.Empty);
            }

            if (prev != null && results.Title != prev.Title)
            {
                RaiseTitleChanged(new TitleEventArgs(prev.Title, results.Title));
            }
        }

        private void InitPoller()
        {
            IntPtr handle = handleGetter();
            if (handle == IntPtr.Zero)
            {
                poller = null;
                return;
            }
            poller = new WindowPoller(handle);
        }
    }
}
