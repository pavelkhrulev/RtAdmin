using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public static class LocalPinChanger
    {
        public static void Change(Slot slot,
            string userPin, string localPin, uint localPinId) => slot.SetLocalPIN(userPin, localPin, localPinId);
    }
}
