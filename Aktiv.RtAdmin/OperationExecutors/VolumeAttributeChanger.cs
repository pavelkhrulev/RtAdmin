using Aktiv.RtAdmin.Properties;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public static class VolumeAttributeChanger
    {
        public static void Change(Slot slot, 
            ChangeVolumeAttributesParams volumeAttributes)
        {
            try
            {
                slot.ChangeVolumeAttributes(
                    volumeAttributes.VolumeOwner,
                    volumeAttributes.OwnerPin,
                    (uint)volumeAttributes.VolumeId,
                    volumeAttributes.AccessMode,
                    volumeAttributes.Permanent);
            }
            catch (Pkcs11Exception ex) when (ex.RV == CKR.CKR_PIN_INCORRECT)
            {
                throw new CKRException(ex.RV, Resources.IncorrectPin);
            }
        }
    }
}