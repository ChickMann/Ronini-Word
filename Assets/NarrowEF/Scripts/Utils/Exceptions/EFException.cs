using System;

namespace EF.Generic
{
    public class EFException : Exception
    {
        public EFException() { }

        public EFException(string message)
            : base(message) { }

        public EFException(string message, Exception inner)
            : base(message, inner) { }
    }
}