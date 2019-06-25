using System;
using Aktiv.RtAdmin.Properties;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public static class PinUnlocker
    {
        public static void Unlock(Slot slot, PinCodeOwner pinOwner, string pin)
        {
            using var session = slot.OpenSession(SessionType.ReadWrite);
            var cku = pinOwner == PinCodeOwner.Admin ?
                CKU.CKU_SO : CKU.CKU_USER;

            try
            {
                session.Login(cku, pin);
            }
            catch (Pkcs11Exception ex) when (ex.RV == CKR.CKR_PIN_INCORRECT)
            {
                throw new InvalidOperationException(Resources.IncorrectPin);
            }

            try
            {
                session.UnblockUserPIN();
            }
            finally
            {
                session.Logout();
            }
        }
    }
}
