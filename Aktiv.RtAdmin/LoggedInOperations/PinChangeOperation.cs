using Net.RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public class PinChangeOperation : BaseTokenOperation<PinChangeOperationParams>
    {
        protected override void Payload(IRutokenSession session, PinChangeOperationParams operationParams) =>
            session.SetPin(operationParams.OldPin, operationParams.NewPin);
    }
}