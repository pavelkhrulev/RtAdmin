using Aktiv.RtAdmin.Properties;
using Net.Pkcs11Interop.Common;
using Net.RutokenPkcs11Interop.Common;
using Net.RutokenPkcs11Interop.HighLevelAPI;
using System.Collections.Generic;

namespace Aktiv.RtAdmin
{
    public static class TokenFormatter
    {
        public static void Format(IRutokenSlot slot, 
            string currentAdminPin,
            string newAdminPin, string newUserPin, string tokenLabel, RutokenFlag policy, 
            uint minAdminPinLength, uint minUserPinLength, uint maxAdminAttempts, uint maxUserAttempts, 
            uint smMode)
        {
            var rutokenInitParam = new RutokenInitParam(
                newAdminPin, newUserPin,
                tokenLabel,
                new List<RutokenFlag> { policy },
                minAdminPinLength, minUserPinLength,
                maxAdminAttempts, maxUserAttempts, smMode, true);

            try
            {
                slot.InitTokenExtended(currentAdminPin, rutokenInitParam);
            }
            catch (Pkcs11Exception ex) when (ex.RV == CKR.CKR_PIN_INCORRECT)
            {
                throw new CKRException(ex.RV, Resources.IncorrectPin);
            }
        }
    }
}
