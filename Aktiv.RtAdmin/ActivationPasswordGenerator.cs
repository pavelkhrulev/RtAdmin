using System;
using System.Collections.Generic;
using Aktiv.RtAdmin.Properties;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.Common;
using RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public static class ActivationPasswordGenerator
    {
        public static IEnumerable<byte[]> Generate(Slot slot, string adminPin, 
            ActivationPasswordCharacterSet characterSet, ulong smMode)
        {
            using var session = slot.OpenSession(SessionType.ReadWrite);

            try
            {
                session.Login(CKU.CKU_SO, adminPin);
            }
            catch (Pkcs11Exception ex) when (ex.RV == CKR.CKR_PIN_INCORRECT)
            {
                throw new InvalidOperationException(Resources.IncorrectPin);
            }

            try
            {
                var passwordsCount = smMode == 3 ? 6 : 1;

                for (var i = 1; i <= passwordsCount; i++)
                {
                    yield return session.GenerateActivationPassword((ActivationPasswordNumber) i, characterSet);
                }
            }
            finally
            {
                session.Logout();
            }
        }
    }
}
