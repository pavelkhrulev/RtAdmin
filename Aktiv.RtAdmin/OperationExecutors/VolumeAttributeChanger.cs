using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public static class VolumeAttributeChanger
    {
        public static void Change(Slot slot, 
            ChangeVolumeAttributesParams volumeAttributes)
        {
            slot.ChangeVolumeAttributes(
                volumeAttributes.VolumeOwner,
                volumeAttributes.OwnerPin,
                (uint)volumeAttributes.VolumeId,
                volumeAttributes.AccessMode,
                volumeAttributes.Permanent);
        }
    }
}