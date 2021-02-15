using Net.Pkcs11Interop.Common;
using Net.RutokenPkcs11Interop.HighLevelAPI;


namespace Aktiv.RtAdmin
{
    public class PinPolicyChangeOperation : BaseTokenOperation<PinPolicyChangeOperationParams>
    {
        protected override void Payload(IRutokenSession session, PinPolicyChangeOperationParams pinPolicyParams) =>
            session.SetPinPolicy(pinPolicyParams.PinPolicy, CKU.CKU_USER);
    }
}