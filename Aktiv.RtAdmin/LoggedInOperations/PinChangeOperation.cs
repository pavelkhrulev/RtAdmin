using Aktiv.RtAdmin.Operations;
using Net.Pkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public class PinChangeOperation : BaseTokenOperation<PinChangeOperationParams>
    {
        protected override void Payload(Session session, PinChangeOperationParams operationParams) => 
            session.SetPin(operationParams.OldPin, operationParams.NewPin);
    }
}