using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;
using RutokenPkcs11Interop.HighLevelAPI;


namespace Aktiv.RtAdmin
{
    public class PinPolicyChangeOperation : BaseTokenOperation<PinPolicyChangeOperationParams>
    {
        protected override void Payload(Session session, PinPolicyChangeOperationParams pinPolicyParams) =>
            session.SetPinPolicy(pinPolicyParams.PinPolicy, CKU.CKU_USER);
    }
}