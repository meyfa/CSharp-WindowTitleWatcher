using System;
using System.Diagnostics;

namespace WindowTitleWatcher
{
    public class Watcher
    {
        private Process process;
        
        /// <summary>
        /// Gets the current window title.
        /// </summary>
        private string Title
        {
            get
            {
                return process.HasExited ? null : process.MainWindowTitle;
            }
        }

        public Watcher(Process proc)
        {
            process = proc;
        }
    }
}
