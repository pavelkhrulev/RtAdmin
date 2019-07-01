using System.Collections.Generic;

namespace Aktiv.RtAdmin
{
    public class TokenParams
    {
        public ulong MinUserPinLenFromToken { get; set; }
        public ulong MaxUserPinLenFromToken { get; set; }
        public ulong MinAdminPinLenFromToken { get; set; }
        public ulong MaxAdminPinLenFromToken { get; set; }
        public string TokenSerial { get; set; }

        public PinCode OldAdminPin { get; set; }

        public PinCode OldUserPin { get; set; }

        public PinCode NewAdminPin { get; set; }

        public PinCode NewUserPin { get; set; }

        public Dictionary<uint, string> LocalUserPins { get; set; }

        public string TokenLabel { get; set; } = DefaultValues.TokenLabel;

        public bool AdminCanChangeUserPin { get; set; }

        public bool UserCanChangeUserPin { get; set; }
    }
}
