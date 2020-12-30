using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System.Collections.Generic;
using System.Linq;
using Net.RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public class TokenSlot
    {
        private readonly IRutokenPkcs11Library _pkcs11;

        public TokenSlot(IRutokenPkcs11Library pkcs11)
        {
            _pkcs11 = pkcs11;
        }

        public Stack<IRutokenSlot> GetInitialSlots() => new Stack<IRutokenSlot>(_pkcs11.GetRutokenSlotList(SlotsType.WithTokenPresent));

        public IRutokenSlot WaitToken()
        {
            IRutokenSlot slot;
            do
            {
                _pkcs11.WaitForSlotEvent(WaitType.Blocking, out _, out var slotId);
                slot = _pkcs11.GetRutokenSlotList(SlotsType.WithTokenPresent)
                              .SingleOrDefault(x => x.SlotId == slotId);
            }
            while (slot == null);

            return slot;
        }
    }
}
