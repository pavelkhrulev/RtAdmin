using Net.Pkcs11Interop.Common;

namespace Aktiv.RtAdmin.Operations
{
    public class BaseTokenOperationParams
    {
        public CKU LoginType { get; set; }
        public string LoginPin { get; set; }
    }
}