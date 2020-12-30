using Net.Pkcs11Interop.Common;
using Net.RutokenPkcs11Interop.HighLevelAPI;
using Net.RutokenPkcs11Interop.Common;
using System.Collections.Generic;

namespace Aktiv.RtAdmin
{
    public static class ActivationPasswordGenerator
    {
        public static IEnumerable<byte[]> Generate(IRutokenSlot slot, string adminPin,
            ActivationPasswordCharacterSet characterSet, uint smMode)
        {
            var operationParams = new GenerateActivationPasswordsOperationParams
            {
                LoginType = CKU.CKU_SO,
                LoginPin = adminPin,
                CharacterSet = characterSet,
                SmMode = smMode
            };

            new GenerateActivationPasswordsOperation().Invoke(slot, operationParams);

            return operationParams.ActivationPasswords;
        }
    }
}
