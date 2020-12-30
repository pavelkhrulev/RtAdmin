using Net.RutokenPkcs11Interop.Common;
using System.Collections.Generic;

namespace Aktiv.RtAdmin
{
    public class RuntimeTokenParams
    {
        public ulong MinUserPinLenFromToken { get; set; }

        public ulong MaxUserPinLenFromToken { get; set; }

        public ulong MinAdminPinLenFromToken { get; set; }

        public ulong MaxAdminPinLenFromToken { get; set; }

        public string TokenSerial { get; set; }

        public string TokenSerialDecimal { get; set; }

        public PinCode OldAdminPin { get; set; }

        public PinCode OldUserPin { get; set; }

        public PinCode NewAdminPin { get; set; }

        public PinCode NewUserPin { get; set; }

        public string NewLocalPin { get; set; }

        public Dictionary<uint, string> LocalUserPins { get; set; }

        public string TokenLabel { get; set; } = DefaultValues.TokenLabel;

        public UserPinChangePolicy UserPinChangePolicy { get; set; }

        public uint SmMode { get; set; }

        public ActivationPasswordCharacterSet CharacterSet { get; set; }

        public RutokenType TokenType { get; set; }

        public uint HardwareMajorVersion { get; set; }

        public uint LocalIdToCreate { get; set; }

        public bool FlashMemoryAvailable { get; set; }
    }
}
