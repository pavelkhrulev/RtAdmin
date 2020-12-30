using Net.RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public class SetTokenNameOperation : BaseTokenOperation<SetTokenNameOperationParams>
    {
        protected override void Payload(IRutokenSession session, SetTokenNameOperationParams operationParams) =>
            session.SetTokenName(operationParams.TokenName);
    }
}
