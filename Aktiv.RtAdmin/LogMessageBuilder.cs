using System;
using Aktiv.RtAdmin.Properties;
using Net.Pkcs11Interop.Common;

namespace Aktiv.RtAdmin
{
    public class LogMessageBuilder
    {
        private readonly TokenParams _tokenParams;

        public LogMessageBuilder(TokenParams tokenParams)
        {
            _tokenParams = tokenParams ?? 
                           throw new ArgumentNullException(nameof(tokenParams), Resources.TokenParamsNotSet);
        }

        public string WithTokenId(string message) => 
            $"0x{_tokenParams.TokenSerial} / {_tokenParams.TokenSerialDecimal} : {message}";

        public string WithTokenIdSuffix(string message) =>
            $"{message}. {Resources.TokenId}: 0x{_tokenParams.TokenSerial}";

        public string WithPKCS11Error(CKR code) => $"{Resources.PKCS11Error} 0x{code:X}";

        public string WithUnhandledError(string message) => $"{Resources.UnhandledError} {message}";

        // TODO: _tokenParams.SmMode
        public string WithFormatResult(string message) =>
            string.Format(WithTokenId(message),
                _tokenParams.NewAdminPin, _tokenParams.NewUserPin, 0);
    }
}
