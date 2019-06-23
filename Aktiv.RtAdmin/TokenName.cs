using System;
using Aktiv.RtAdmin.Properties;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public static class TokenName
    {
        public static void SetNew(Slot slot, string userPin, string name)
        {
            using var session = slot.OpenSession(SessionType.ReadWrite);

            try
            {
                session.Login(CKU.CKU_USER, userPin);
            }
            catch (Pkcs11Exception ex) when (ex.RV == CKR.CKR_PIN_INCORRECT)
            {
                throw new InvalidOperationException(Resources.IncorrectPin);
            }

            try
            {

                session.SetTokenName(name);
            }
            finally
            {
                session.Logout();
            }
        }
    }
}
