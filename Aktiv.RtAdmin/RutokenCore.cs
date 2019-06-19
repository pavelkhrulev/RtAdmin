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
        private readonly Pkcs11 pkcs11;
        private readonly ILogger logger;

        public RutokenCore(Pkcs11 pkcs11, ILogger<RtAdmin> logger)
        {
            this.pkcs11 = pkcs11 ?? throw new ArgumentNullException(nameof(pkcs11));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.logger.LogInformation(Resources.InitializedInfo);
        }

        public Slot WaitToken()
        {
            Slot slot;
            do
            {
                pkcs11.WaitForSlotEvent(WaitType.Blocking, out _, out var slotId);
                slot = pkcs11.GetSlotList(SlotsType.WithTokenPresent)
                      .SingleOrDefault(x => x.SlotId == slotId);
            }
            while (slot == null);

            return slot;
        }
    }
}
