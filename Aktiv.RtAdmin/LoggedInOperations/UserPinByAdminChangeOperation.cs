using Net.Pkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public class UserPinByAdminChangeOperation : BaseTokenOperation<PinChangeOperationParams>
    {
        protected override void Payload(Session session, PinChangeOperationParams operationParams) => 
            session.InitPin(operationParams.NewPin);
    }
}