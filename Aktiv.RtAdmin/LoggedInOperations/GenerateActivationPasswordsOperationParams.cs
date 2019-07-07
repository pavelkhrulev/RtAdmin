using System.Collections.Generic;
using Aktiv.RtAdmin.Operations;
using RutokenPkcs11Interop.Common;

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