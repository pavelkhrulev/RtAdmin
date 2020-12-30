using Net.Pkcs11Interop.Common;
using Net.RutokenPkcs11Interop.Common;

namespace Aktiv.RtAdmin
{
    public class ChangeVolumeAttributesParams
    {
        public FlashAccessMode AccessMode { get; set; }

        public CKU VolumeOwner { get; set; }

        public ulong VolumeId { get; set; }

        public bool Permanent { get; set; }

        public string OwnerPin { get; set; }
    }
}