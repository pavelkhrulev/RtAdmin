using Aktiv.RtAdmin.Properties;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System;

namespace Aktiv.RtAdmin
{
    public static class PinChanger
    {
        public static void Change(Slot slot,
            string oldPin, string newPin, PinCodeOwner oldPinOwner)
        {
            using var session = slot.OpenSession(SessionType.ReadWrite);
            var cku = oldPinOwner == PinCodeOwner.Admin ?
                CKU.CKU_SO : CKU.CKU_USER;

            try
            {
                session.Login(cku, oldPin);
            }
            catch (Pkcs11Exception ex) when (ex.RV == CKR.CKR_PIN_INCORRECT)
            {
                throw new InvalidOperationException(Resources.IncorrectPin);
            }

            try
            {
                session.SetPin(oldPin, newPin);
            }
            finally
            {
                session.Logout();
            }
        }

        public static void ChangeUserPinByAdmin(Slot slot, 
            string currentAdminPin, string newUserPin)
        {
            using var session = slot.OpenSession(SessionType.ReadWrite);

            try
            {
                session.Login(CKU.CKU_SO, currentAdminPin);
            }
            catch (Pkcs11Exception ex) when (ex.RV == CKR.CKR_PIN_INCORRECT)
            {
                throw new InvalidOperationException(Resources.IncorrectPin);
            }

            try
            {
                session.InitPin(newUserPin);
            }
            finally
            {
                session.Logout();
            }
        }
    }
}
