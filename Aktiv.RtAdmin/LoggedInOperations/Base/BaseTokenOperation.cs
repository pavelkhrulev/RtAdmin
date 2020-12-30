using Aktiv.RtAdmin.Properties;
using Net.Pkcs11Interop.Common;
using Net.RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public abstract class BaseTokenOperation<T> where T : BaseTokenOperationParams
    {
        public void Invoke(IRutokenSlot slot, T operationParams)
        {
            using var session = slot.OpenRutokenSession(SessionType.ReadWrite);

            try
            {
                session.Login(operationParams.LoginType, operationParams.LoginPin);
            }
            catch (Pkcs11Exception ex) when (ex.RV == CKR.CKR_PIN_INCORRECT)
            {
                throw new CKRException(ex.RV, Resources.IncorrectPin);
            }

            try
            {
                Payload(session, operationParams);
            }
            finally
            {
                session.Logout();
            }
        }

        protected abstract void Payload(IRutokenSession session, T operationParams);
    }
}
