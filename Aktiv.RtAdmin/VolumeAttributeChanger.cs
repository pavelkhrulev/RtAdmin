using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.HighLevelAPI;
using System.Collections.Generic;

namespace Aktiv.RtAdmin
{
    public static class VolumeAttributeChanger
    {
        public static void Change(Slot slot, 
            IEnumerable<ChangeVolumeAttributesParams> volumeAttributes)
        {
            foreach (var attributes in volumeAttributes)
            {
                slot.ChangeVolumeAttributes(
                    attributes.VolumeOwner,
                    attributes.OwnerPin,
                    (uint)attributes.VolumeId,
                    attributes.AccessMode,
                    attributes.Permanent);
            }
        }
    }
}
