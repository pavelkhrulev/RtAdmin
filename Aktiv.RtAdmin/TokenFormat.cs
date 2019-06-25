using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.Common;
using RutokenPkcs11Interop.HighLevelAPI;
using System.Collections.Generic;

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

            slot.InitTokenExtended(currentAdminPin, rutokenInitParam);
        }
    }
}
