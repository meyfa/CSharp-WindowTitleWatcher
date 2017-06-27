using System;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowTitleWatcher.Internal
{
    /// <summary>
    /// Call <see cref="Poll"/> on instances of this class to retrieve current
    /// window information.
    /// </summary>
    internal class WindowPoller
    {
        #region imports

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        #endregion
        
        public readonly IntPtr WindowHandle;

        public WindowPoller(IntPtr handle)
        {
            WindowHandle = handle;
        }
        
        public Results Poll()
        {
            // check whether disposed
            if (!IsWindow(WindowHandle))
            {
                return Results.DISPOSED;
            }

            // update visibility
            bool visible = IsWindowVisible(WindowHandle);

            // update title
            int size = GetWindowTextLength(WindowHandle);
            StringBuilder sb = new StringBuilder(size + 1);
            GetWindowText(WindowHandle, sb, sb.Capacity);
            string title = sb.ToString();

            return new Results(visible, title);
        }

        /// <summary>
        /// Stores the results of a poll operation.
        /// </summary>
        public class Results
        {
            public static readonly Results DISPOSED = new Results(true, false, null);

            public readonly bool IsDisposed;
            public readonly bool IsVisible;
            public readonly string Title;

            public Results(bool visible, string title)
                : this(false, visible, title)
            {
            }

            private Results(bool disposed, bool visible, string title)
            {
                IsDisposed = disposed;
                IsVisible = visible;
                Title = title;
            }
        }
    }
}
