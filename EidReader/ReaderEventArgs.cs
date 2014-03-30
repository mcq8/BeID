using System;

namespace Be.Mcq8.EidReader
{
    public class ReaderEventArgs : EventArgs
    {
        public ReaderEventArgs(Reader reader)
        {
            this.Reader = reader;
        }
        public Reader Reader { get; set; }
    }
}
