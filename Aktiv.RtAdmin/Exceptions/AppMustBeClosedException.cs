using System;

namespace Aktiv.RtAdmin
{
    public class AppMustBeClosedException : Exception
    {
        public AppMustBeClosedException(int retCode)
        {
            RetCode = retCode;
        }

        public int RetCode { get; }
    }
}
