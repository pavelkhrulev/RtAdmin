using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public class SetTokenNameOperation : BaseTokenOperation<SetTokenNameOperationParams>
    {
        protected override void Payload(Session session, SetTokenNameOperationParams operationParams) =>
            session.SetTokenName(operationParams.TokenName);
    }
}
