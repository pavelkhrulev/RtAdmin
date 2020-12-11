using RutokenPkcs11Interop.Common;
using System.Collections.Generic;

namespace RutokenPkcs11Interop.Common
{
    public class PinPolicyWithNames: PinPolicy
    {
        public const string MinPinLengthName = nameof(MinPinLength);
        public const string PinHistoryDepthName = nameof(PinHistoryDepth);
        public const string AllowDefaultPinUsageName = nameof(AllowDefaultPinUsage);
        public const string PinContainsDigitName = nameof(PinContainsDigit);
        public const string PinContainsUpperLetterName = nameof(PinContainsUpperLetter);
        public const string PinContainsLowerLetterName = nameof(PinContainsLowerLetter);
        public const string PinContainsSpecCharName = nameof(PinContainsSpecChar);
        public const string RestrictOneCharPinName = nameof(RestrictOneCharPin);
        public const string AllowChangePinPolicyName = nameof(AllowChangePinPolicy);
        public const string RemovePinPolicyAfterFormatName = nameof(RemovePinPolicyAfterFormat);
    }
}
