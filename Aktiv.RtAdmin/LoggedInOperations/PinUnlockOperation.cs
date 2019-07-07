using Aktiv.RtAdmin.Operations;
using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public class PinUnlockOperation : BaseTokenOperation<BaseTokenOperationParams>
    {
        protected override void Payload(Session session, BaseTokenOperationParams operationParams) =>
            session.UnblockUserPIN();
    }
}
