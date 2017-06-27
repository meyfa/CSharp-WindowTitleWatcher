using System;

namespace WindowTitleWatcher
{
    public class TitleEventArgs : EventArgs
    {
        public readonly string PreviousTitle;
        public readonly string NewTitle;

        public TitleEventArgs(string previous, string current)
        {
            PreviousTitle = previous;
            NewTitle = current;
        }
    }
}
