using Net.RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public class UserPinByAdminChangeOperation : BaseTokenOperation<PinChangeOperationParams>
    {
        protected override void Payload(IRutokenSession session, PinChangeOperationParams operationParams) =>
            session.InitPin(operationParams.NewPin);
    }
}