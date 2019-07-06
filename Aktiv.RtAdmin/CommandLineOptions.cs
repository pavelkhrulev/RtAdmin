using CommandLine;
using RutokenPkcs11Interop.Common;
using System.Collections.Generic;

namespace Aktiv.RtAdmin
{
    public class CommandLineOptions
    {
        [Option('f', "format", HelpText = "FormatTokenOption", ResourceType = typeof(Properties.Resources))]
        public bool Format { get; set; }

        [Option('G', HelpText = "GenerateRandomAdminPinOption", ResourceType = typeof(Properties.Resources))]
        public uint? AdminPinLength { get; set; }

        [Option('g', HelpText = "GenerateRandomUserPinOption", ResourceType = typeof(Properties.Resources))]
        public uint? UserPinLength { get; set; }

        [Option('U', HelpText = "Utf8Option", ResourceType = typeof(Properties.Resources))]
        public bool UTF8InsteadOfcp1251 { get; set; }

        [Option('L', HelpText = "TokenLabelCp1251Option", ResourceType = typeof(Properties.Resources))]
        public string TokenLabelCp1251 { get; set; }

        [Option('D', HelpText = "TokenLabelUtf8Option", ResourceType = typeof(Properties.Resources))]
        public string TokenLabelUtf8 { get; set; }

        [Option('o', HelpText = "OldAdminPinOption", ResourceType = typeof(Properties.Resources))]
        public string OldAdminPin { get; set; }

        [Option('c', HelpText = "OldUserPinOption", ResourceType = typeof(Properties.Resources))]
        public string OldUserPin { get; set; }

        [Option('a', HelpText = "AdminPinOption", ResourceType = typeof(Properties.Resources))]
        public string AdminPin { get; set; }

        [Option('u', HelpText = "UserPinOption", ResourceType = typeof(Properties.Resources))]
        public string UserPin { get; set; }

        [Option('t', HelpText = "SetPin2ModeOption", ResourceType = typeof(Properties.Resources))]
        public bool SetPin2Mode { get; set; }

        [Option('n', HelpText = "ConfigurationFilePathOption", ResourceType = typeof(Properties.Resources))]
        public string ConfigurationFilePath { get; set; }

        [Option('l', HelpText = "LogFilePathOption", ResourceType = typeof(Properties.Resources))]
        public string LogFilePath { get; set; }

        [Option('b', HelpText = "PinFilePathOption", ResourceType = typeof(Properties.Resources))]
        public string PinFilePath { get; set; }

        [Option('P', HelpText = "UnblockPinsOption", ResourceType = typeof(Properties.Resources))]
        public bool UnblockPins { get; set; }

        [Option('q', HelpText = "OneIterationOnlyOption", ResourceType = typeof(Properties.Resources))]
        public bool OneIterationOnly { get; set; }

        [Option('z', HelpText = "NativeLibraryPathOption", ResourceType = typeof(Properties.Resources))]
        public string NativeLibraryPath { get; set; }

        [Option('M', Default = DefaultValues.MinAdminPinLength, HelpText = "MinAdminPinLengthOption", ResourceType = typeof(Properties.Resources))]
        public uint MinAdminPinLength { get; set; }

        [Option('m', Default = DefaultValues.MinUserPinLength, HelpText = "MinUserPinLengthOption", ResourceType = typeof(Properties.Resources))]
        public uint MinUserPinLength { get; set; }

        [Option('R', Default = DefaultValues.MaxAdminPinAttempts, HelpText = "MaxAdminPinAttemptsOption", ResourceType = typeof(Properties.Resources))]
        public uint MaxAdminPinAttempts { get; set; }

        [Option('r', Default = DefaultValues.MaxUserPinAttempts, HelpText = "MaxUserPinAttemptsOption", ResourceType = typeof(Properties.Resources))]
        public uint MaxUserPinAttempts { get; set; }

        [Option('p', Default = DefaultValues.PinChangePolicy, HelpText = "PinChangePolicyOption", ResourceType = typeof(Properties.Resources))]
        public UserPinChangePolicy PinChangePolicy { get; set; }

        [Option('s', HelpText = "SmModeOption", ResourceType = typeof(Properties.Resources))]
        public IEnumerable<string> GenerateActivationPasswords { get; set; }

        [Option('F', HelpText = "FormatVolumeParamsOption", ResourceType = typeof(Properties.Resources))]
        public IEnumerable<string> FormatVolumeParams { get; set; }

        [Option('C', HelpText = "ChangeVolumeAttributesOption", ResourceType = typeof(Properties.Resources))]
        public IEnumerable<string> ChangeVolumeAttributes { get; set; }

        [Option('i', HelpText = "VolumeInfoParamsOption", ResourceType = typeof(Properties.Resources))]
        public string VolumeInfoParams { get; set; }

        [Option('O', HelpText = "LoginWithLocalPinOption", ResourceType = typeof(Properties.Resources))]
        public IEnumerable<string> LoginWithLocalPin { get; set; }

        [Option('B', HelpText = "SetLocalPinOption", ResourceType = typeof(Properties.Resources))]
        public IEnumerable<string> SetLocalPin { get; set; }
    }
}
