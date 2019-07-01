using System;
using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.Common;
using RutokenPkcs11Interop.HighLevelAPI;
using System.Collections.Generic;
using Aktiv.RtAdmin.Properties;
using Net.Pkcs11Interop.Common;

namespace Aktiv.RtAdmin
{
    public static class TokenFormatter
    {
        public static void Format(Slot slot, 
            string currentAdminPin,
            string newAdminPin, string newUserPin, string tokenLabel, RutokenFlag policy, 
            uint minAdminPinLength, uint minUserPinLength, uint maxAdminAttempts, uint maxUserAttempts, 
            uint smMode)
        {
            var rutokenInitParam = new RutokenInitParam(newAdminPin, newUserPin,
            tokenLabel, 
            new List<RutokenFlag> { policy }, 
            minAdminPinLength, minUserPinLength,
            maxAdminAttempts, maxUserAttempts, smMode);

            // TODO: check old token

            try
            {
                // TODO: BlockToken

                slot.InitTokenExtended(currentAdminPin, rutokenInitParam);
            }
            catch (Pkcs11Exception ex) when (ex.RV == CKR.CKR_PIN_INCORRECT)
            {
                throw new InvalidOperationException(Resources.IncorrectPin);
            }
        }
    }
}
