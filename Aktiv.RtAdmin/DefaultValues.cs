using RutokenPkcs11Interop.Common;

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

        public const RutokenFlag PinChangePolicy = RutokenFlag.UserChangeUserPin;

        public const string NativeLibraryPath = "rtPKCS11.dll";

        public const string LogFilePath = "rtadmin.exe.log";
    }
}
