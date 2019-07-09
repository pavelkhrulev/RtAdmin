using RutokenPkcs11Interop.Common;
using System.Collections.Generic;

namespace Aktiv.RtAdmin
{
    public class GenerateActivationPasswordsOperationParams : BaseTokenOperationParams
    {
        public ActivationPasswordCharacterSet CharacterSet { get; set; }

        public uint SmMode { get; set; }

        // Out-param
        public ICollection<byte[]> ActivationPasswords { get; set; }
    }
}