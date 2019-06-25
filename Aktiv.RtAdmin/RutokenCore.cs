using Microsoft.Extensions.Logging;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System;
using System.Linq;
using Aktiv.RtAdmin.Properties;

namespace Aktiv.RtAdmin
{
    public class RutokenCore
    {
        private readonly Pkcs11 _pkcs11;
        private readonly ILogger _logger;

        public RutokenCore(Pkcs11 pkcs11, ILogger<RtAdmin> logger)
        {
            _pkcs11 = pkcs11 ?? throw new ArgumentNullException(nameof(pkcs11));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogInformation(Resources.InitializedInfo);
        }

        public Slot WaitToken()
        {
            Slot slot;
            do
            {
                _pkcs11.WaitForSlotEvent(WaitType.Blocking, out _, out var slotId);
                slot = _pkcs11.GetSlotList(SlotsType.WithTokenPresent)
                      .SingleOrDefault(x => x.SlotId == slotId);
            }
            while (slot == null);

            return slot;
        }
    }
}
