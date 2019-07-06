using System;
using Net.Pkcs11Interop.Common;

namespace Aktiv.RtAdmin
{
    public class CKRException : Exception
    {
        public CKRException(CKR ckr)
        {
            ReturnCode = ckr;
        }

        public CKRException(CKR ckr, string message) 
            : base(message)
        {
            ReturnCode = ckr;
        }

        public CKR ReturnCode { get; }
    }
}
