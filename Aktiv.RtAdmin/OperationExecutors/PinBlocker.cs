using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.Common;

namespace Aktiv.RtAdmin
{
    public static class PinBlocker
    {
        private const string _wrongPin1 = "1234567890123456789012345678901";
        private const string _wrongPin1_RutokenS = "1234567890123456789012345678901";
        private const string _wrongPin2 = "-234567890123456789012345678901";
        private const string _wrongPin2_RutokenS = "-234567890123456";

        public static void Block(Slot slot, RutokenType tokenType)
        {
            using var session = slot.OpenSession(SessionType.ReadWrite);

            var wrongPin = tokenType == RutokenType.RUTOKEN ? _wrongPin1_RutokenS : _wrongPin1;
            var wrongPin2 = tokenType == RutokenType.RUTOKEN ? _wrongPin2_RutokenS : _wrongPin2;

            while (true)
            {
                try
                {
                    session.Login(CKU.CKU_SO, wrongPin);
                    session.Logout();

                    // Если успешно залогинились, то логинимся еще с другим паролем
                    try
                    {
                        session.Login(CKU.CKU_SO, wrongPin2);
                        session.Logout();
                    }
                    catch (Pkcs11Exception ex) when (ex.RV == CKR.CKR_PIN_LOCKED)
                    {
                        return;
                    }
                }
                catch (Pkcs11Exception ex) when (ex.RV == CKR.CKR_PIN_LOCKED)
                {
                    return;
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}
