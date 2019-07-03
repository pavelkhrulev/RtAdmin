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
            // TODO: params.UseRepairMode = 1;// nikita.  now use repair mode WO admin pin
            //	/* temporary solution: old library (v1.2.5.x) needs old_so_pin to be not nullptr, and its length not to be null (PKCSECP-708)
            //so we just pass dummy old_so_pin. the correct behavior is formatting with UseRepairMode = 1 regardless of PIN. */

            //if (NULL == old_so_pin || 0 == strlen(old_so_pin))
            //    old_so_pin = "0";

            var rutokenInitParam = new RutokenInitParam(
                newAdminPin, newUserPin,
                tokenLabel,
                new List<RutokenFlag> { policy },
                minAdminPinLength, minUserPinLength,
                maxAdminAttempts, maxUserAttempts, smMode);

            try
            {
                // TODO: BlockToken

                slot.InitTokenExtended(currentAdminPin, rutokenInitParam);
            }
            catch (Pkcs11Exception ex) when (ex.RV == CKR.CKR_PIN_INCORRECT)
            {
                throw new CKRException(ex.RV, Resources.IncorrectPin);
            }
        }
    }
}
