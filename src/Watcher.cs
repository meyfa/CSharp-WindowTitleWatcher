using System;

namespace WindowTitleWatcher
{
    public abstract class Watcher
    {
        /// <summary>
        /// Returns whether the window is currently visible.
        /// </summary>
        public bool IsVisible
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the current window title.
        /// </summary>
        public string Title
        {
            get;
            protected set;
        }

        public event EventHandler<TitleEventArgs> TitleChanged;
        public event EventHandler VisibilityChanged;

        protected void RaiseTitleChanged(TitleEventArgs e)
        {
            TitleChanged?.Invoke(this, e);
        }

        protected void RaiseVisibilityChanged(EventArgs e)
        {
            VisibilityChanged?.Invoke(this, e);
        }
    }
}
