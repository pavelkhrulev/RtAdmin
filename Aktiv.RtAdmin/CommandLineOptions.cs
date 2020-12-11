using System.Collections.Generic;
using RutokenPkcs11Interop.Common;

namespace Aktiv.RtAdmin
{
    public class CommandLineOptions
    {
        public CommandLineOptions()
        {
            GenerateActivationPasswords = new List<string>();
            FormatVolumeParams = new List<string>();
            ChangeVolumeAttributes = new List<string>();
            LoginWithLocalPin = new List<string>();
            SetLocalPin = new List<string>();
            ExcludedTokens = new List<string>();
            PinPolicy = new PinPolicy();
        }

        public bool Format { get; set; }

        public uint? AdminPinLength { get; set; }

        public uint? UserPinLength { get; set; }

        public string TokenLabelCp1251 { get; set; }

        public string TokenLabelUtf8 { get; set; }

        public string OldAdminPin { get; set; }

        public string OldUserPin { get; set; }

        public string AdminPin { get; set; }

        public string UserPin { get; set; }

        public bool StdinPins { get; set; }

        public bool SetPin2Mode { get; set; }

        public string ConfigurationFilePath { get; set; }

        public string LogFilePath { get; set; }

        public string PinFilePath { get; set; }

        public string NativeLibraryPath { get; set; }

        public string SerialNumber { get; set; }

        public bool UnblockPins { get; set; }

        public bool OneIterationOnly { get; set; }

        public ICollection<string> GenerateActivationPasswords { get; }

        public ICollection<string> FormatVolumeParams { get; }

        public ICollection<string> ChangeVolumeAttributes { get; }

        public string VolumeInfoParams { get; set; }

        public ICollection<string> LoginWithLocalPin { get; }

        public ICollection<string> SetLocalPin { get; }

        public PinPolicy PinPolicy { get; set; }

        public bool ShowExtendedPinPolicy { get; set; }

        #region Format options

        public uint MinAdminPinLength { get; set; } = DefaultValues.MinAdminPinLength;

        public uint MinUserPinLength { get; set; } = DefaultValues.MinUserPinLength;

        public uint MaxAdminPinAttempts { get; set; } = DefaultValues.MaxAdminPinAttempts;

        public uint MaxUserPinAttempts { get; set; } = DefaultValues.MaxUserPinAttempts;

        public uint PinChangePolicy { get; set; } = (uint)DefaultValues.PinChangePolicy;

        public ICollection<string> ExcludedTokens { get; }
        #endregion

        #region Obsolete
        public bool UTF8InsteadOfcp1251 { get; set; }
        #endregion
    }
}
