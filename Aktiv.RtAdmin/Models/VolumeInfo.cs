using Net.Pkcs11Interop.Common;
using RutokenPkcs11Interop.Common;
using RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public class VolumeInfo
    {
        public uint Id { get; set; }
        public ulong Size { get; set; }
        public FlashAccessMode AccessMode { get; set; }
        public uint Owner { get; set; }

        public static implicit operator VolumeFormatInfoExtended(VolumeInfo volumeInfo) => 
            new VolumeFormatInfoExtended(volumeInfo.Size, volumeInfo.AccessMode, (CKU)volumeInfo.Owner, 0);
    }
}
