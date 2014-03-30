using System;

namespace Be.Mcq8.EidReader
{
    public class ReaderException : Exception
    {
        public Reader Reader { get; private set; }

        public ReaderException(Reader reader, String message)
            : base(message)
        {
            this.Reader = reader;
        }
    }
}
