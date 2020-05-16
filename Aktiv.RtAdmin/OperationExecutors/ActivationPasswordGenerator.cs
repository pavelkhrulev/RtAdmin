using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.Common;
using System.Collections.Generic;

namespace Aktiv.RtAdmin
{
    public static class ActivationPasswordGenerator
    {
        public static IEnumerable<byte[]> Generate(Slot slot, string adminPin,
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
