using Net.Pkcs11Interop.Common;
using Net.RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public static class TokenNameSetter
    {
        public static void Set(IRutokenSlot slot, string userPin, string tokenName)
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