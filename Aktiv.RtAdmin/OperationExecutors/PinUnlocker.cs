using Net.Pkcs11Interop.Common;
using Net.RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public static class PinUnlocker
    {
        public static void Unlock(IRutokenSlot slot, string adminPin)
        {
            new PinUnlockOperation().Invoke(slot, new BaseTokenOperationParams
            {
                LoginType = CKU.CKU_SO,
                LoginPin = adminPin
            });
        }
    }
}
