using System.Text;
using Net.Pkcs11Interop.Common;
using Net.RutokenPkcs11Interop.HighLevelAPI;
using Net.RutokenPkcs11Interop.Common;

namespace Aktiv.RtAdmin
{
    public static class PinGenerator
    {
        private static readonly byte[] oneByteLetters = 
        {
            0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, //digits
		    0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, //capital en
		    0x4B, 0x4C, 0x4D, 0x4E, 0x4F, 0x50, 0x51, 0x52, 0x53, 0x54,
            0x55, 0x56, 0x57, 0x58, 0x59, 0x5A,
            0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, //small en
		    0x6B, 0x6C, 0x6D, 0x6E, 0x6F, 0x70, 0x71, 0x72, 0x73, 0x74,
            0x75, 0x76, 0x77, 0x78, 0x79, 0x7A
        };

        public static string Generate(IRutokenSlot slot, RutokenType tokenType, uint pinLength)
        {
            using var session = slot.OpenSession(SessionType.ReadOnly);

            var pin = new byte[pinLength];

            for (var i = 0; i < pinLength; i++)
            {
                var random = session.GenerateRandom(2);

                if (tokenType == RutokenType.PINPAD_FAMILY)
                {
                    pin[i] = (byte)(random[1] % 10 + 0x30);
                }
                else
                {
                    pin[i] = oneByteLetters[random[1] % oneByteLetters.Length];
                }
            }

            return Encoding.ASCII.GetString(pin);
        }
    }
}
