using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.Common;
using RutokenPkcs11Interop.HighLevelAPI;
using System.Collections.Generic;

namespace Aktiv.RtAdmin
{
    public class GenerateActivationPasswordsOperation : BaseTokenOperation<GenerateActivationPasswordsOperationParams>
    {
        protected override void Payload(Session session, GenerateActivationPasswordsOperationParams operationParams)
        {
            var passwordsCount = operationParams.SmMode == 3 ? 6 : 1;

            operationParams.ActivationPasswords = new List<byte[]>(passwordsCount);

            for (var i = 1; i <= passwordsCount; i++)
            {
                operationParams.ActivationPasswords.Add(
                    session.GenerateActivationPassword(
                        (ActivationPasswordNumber)i,
                        operationParams.CharacterSet));
            }
        }
    }
}