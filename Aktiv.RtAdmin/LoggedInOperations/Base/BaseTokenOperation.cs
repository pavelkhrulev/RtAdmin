using Aktiv.RtAdmin.Properties;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System;

namespace Aktiv.RtAdmin.Operations
{
    public abstract class BaseTokenOperation<T> where T : BaseTokenOperationParams
    {
        public void Invoke(Slot slot, T operationParams)
        {
            using var session = slot.OpenSession(SessionType.ReadWrite);

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

        protected abstract void Payload(Session session, T operationParams);
    }
}
