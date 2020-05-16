using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public static class TokenNameSetter
    {
        public static void Set(Slot slot, string userPin, string tokenName)
        {
            new SetTokenNameOperation().Invoke(slot, new SetTokenNameOperationParams
            {
                LoginType = CKU.CKU_USER,
                LoginPin = userPin,
                TokenName = tokenName
            });
        }
    }
}