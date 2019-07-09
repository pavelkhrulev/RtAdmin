using Aktiv.RtAdmin.Properties;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public static class PinChanger
    {
        public static void Change<TOperation>(Slot slot,
            string oldPin, string newPin, CKU loginType) where TOperation : BaseTokenOperation<PinChangeOperationParams>, new()
        {
            new TOperation().Invoke(slot, new PinChangeOperationParams
            {
                LoginType = loginType,
                OldPin = oldPin,
                NewPin = newPin
            });
        }

        public static void ChangeLocalPin(Slot slot,
            string userPin, string localPin, uint localPinId)
        {
            try
            {
                slot.SetLocalPIN(userPin, localPin, localPinId);
            }
            catch (Pkcs11Exception ex) when (ex.RV == CKR.CKR_PIN_INCORRECT)
            {
                throw new CKRException(ex.RV, Resources.IncorrectPin);
            }
        }
    }
}
