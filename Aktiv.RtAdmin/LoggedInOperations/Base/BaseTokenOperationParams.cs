using Net.Pkcs11Interop.Common;

namespace Aktiv.RtAdmin
{
    public class BaseTokenOperationParams
    {
        public CKU LoginType { get; set; }
        public string LoginPin { get; set; }
    }
}