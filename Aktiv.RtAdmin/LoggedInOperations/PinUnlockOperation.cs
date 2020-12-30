using Net.RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public class PinUnlockOperation : BaseTokenOperation<BaseTokenOperationParams>
    {
        protected override void Payload(IRutokenSession session, BaseTokenOperationParams operationParams) =>
            session.UnblockUserPIN();
    }
}
