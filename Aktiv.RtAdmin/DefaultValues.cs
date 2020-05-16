namespace Aktiv.RtAdmin
{
    public static class DefaultValues
    {
        public const string AdminPin = "87654321";
        public const string UserPin = "12345678";

        public const uint MaxAdminPinAttempts = 10;
        public const uint MaxUserPinAttempts = 10;

        public const uint MinAdminPinLength = 6;
        public const uint MinUserPinLength = 6;

        public const uint RandomPinLength = 8;

        public const uint RutokenS_MinAdminPinLength = 1;
        public const uint RutokenS_MinUserPinLength = 1;
        public const uint RutokenS_InvalidHardwareMajorVersion = 1;

        public const UserPinChangePolicy PinChangePolicy = UserPinChangePolicy.ByUser;

        public const string LogFilePath = "rtadmin.exe.log";

        public const string TokenLabel = "Rutoken";

        public const string LogTemplate = "{Message:lj}{NewLine}{Exception}";

        public const int MinAllowedMinimalUserPinLength = 1;
        public const int MinAllowedMinimalAdminPinLength = 1;
        public const int MaxAllowedMinimalUserPinLength = 31;
        public const int MaxAllowedMinimalAdminPinLength = 31;

        public const int MinAllowedMaxUserPinAttempts = 1;
        public const int MinAllowedMaxAdminPinAttempts = 3;
        public const int MaxAllowedMaxUserPinAttempts = 10;
        public const int MaxAllowedMaxAdminPinAttempts = 10;

        public const int GenerateActivationPasswordCommandParamsCount = 2;
        public const int NewLocalPinCommandParamsCount = 2;

        public const int MinAllowedSmMode = 1;
        public const int MaxAllowedSmMode = 3;

        public const string CapsCharacterSet = "caps";
        public const string DigitsCharacterSet = "digits";
    }
}
