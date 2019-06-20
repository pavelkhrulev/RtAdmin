namespace Aktiv.RtAdmin
{
    public class TokenParams
    {
        public ulong MinUserPinLenFromToken { get; set; }
        public ulong MaxUserPinLenFromToken { get; set; }
        public ulong MinAdminPinLenFromToken { get; set; }
        public ulong MaxAdminPinLenFromToken { get; set; }
        public string TokenSerial { get; set; }

        public string NewAdminPin { get; set; }

        public string NewUserPin { get; set; }

        public string TokenLabel { get; set; } = DefaultValues.TokenLabel;
    }
}
