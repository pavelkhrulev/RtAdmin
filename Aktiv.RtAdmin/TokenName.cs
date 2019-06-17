using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public static class TokenName
    {
        public static void SetNew(Slot slot, string userPin, string name)
        {
            using (var session = slot.OpenSession(SessionType.ReadWrite))
            {
                try
                {
                    session.Login(CKU.CKU_USER, userPin);

                    session.SetTokenName(name);
                }
                finally
                {
                    session.Logout();
                }
            }
        }
    }
}
