using Aktiv.RtAdmin.Operations;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public static class PinUnlocker
    {
        public static void Unlock(Slot slot, string adminPin)
        {
            new PinUnlockOperation().Invoke(slot, new BaseTokenOperationParams
            {
                LoginType = CKU.CKU_SO,
                LoginPin = adminPin
            });
        }
    }
}
