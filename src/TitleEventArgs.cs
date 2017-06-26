using System;

namespace WindowTitleWatcher
{
    public class TitleEventArgs : EventArgs
    {
        private readonly string previous, current;

        public TitleEventArgs(string previous, string current)
        {
            this.previous = previous;
            this.current = current;
        }

        public string PreviousTitle
        {
            get { return previous; }
        }

        public string NewTitle
        {
            get { return current; }
        }
    }
}
