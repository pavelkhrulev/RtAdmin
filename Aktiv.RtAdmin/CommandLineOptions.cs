using CommandLine;
using RutokenPkcs11Interop.Common;
using System.Collections.Generic;

namespace Aktiv.RtAdmin
{
    public class CommandLineOptions
    {
        [Option('f', "format", 
                    HelpText = "Format memory. If your device has built-in Flash memory you may specify Admin PIN, " +
                            "otherwise you may lose all data on it")]
        public bool Format { get; set; }

        [Option('G', HelpText = "Generate random Admin PIN code of specified length. If this command is used -a is ignored")]
        public uint AdminPinLength { get; set; }

        [Option('g', HelpText = "Generate random User PIN code of specified length. If this command is used -u is ignored")]
        public uint UserPinLength { get; set; }

        [Option('U', HelpText = "Use UTF-8 instead of cp1251 in PIN codes")]
        public bool UTF8InsteadOfcp1251 { get; set; }

        [Option('L', HelpText = "Use cp1251 for new device label")]
        public string TokenLabelCp1251 { get; set; }

        [Option('D', HelpText = "Use UTF-8 for new device label")]
        public string TokenLabelUtf8 { get; set; }

        [Option('o', Default = DefaultValues.AdminPin,
            HelpText = "Old Admin PIN code. Must be specified for Admin PIN changing")]
        public string OldAdminPin { get; set; }

        [Option('c', Default = DefaultValues.UserPin,
            HelpText = "Old User PIN code. Must be specified for User PIN changing")]
        public string OldUserPin { get; set; }

        [Option('a', Default = DefaultValues.AdminPin, 
            HelpText = "Set Admin PIN code. Must be specified for Admin PIN changing. If not specified, the default value is used")]
        public string AdminPin { get; set; }

        [Option('u', Default = DefaultValues.UserPin,
            HelpText = "Set User PIN code. Must be specified for Admin PIN changing. If not specified, the default value is used")]
        public string UserPin { get; set; }

        [Option('t',
            HelpText = "Activate setting PIN2 code (on the screen) mode. Must be specified for PIN2 changing. If not specified, the default value is used")]
        public bool SetPin2Mode { get; set; }

        [Option('n',
            HelpText = "Use configuration file containing string of commands. It must be the last command in command line. DO NOT use this command in configuration file")]
        public string ConfigurationFilePath { get; set; }

        [Option('l', HelpText = "Make log file (UTF-8, default output: to stdout)")]
        public string LogFilePath { get; set; }

        [Option('b',
            HelpText = "Load new PIN codes from specified file. Use ont PIN per line, \n is separator. -a -u -G -g commands are ignored")]
        public string PinFilePath { get; set; }

        [Option('P', HelpText = "Unblock user and local PINs. You should use login with Admin PIN command to unblock user and local PINs")]
        public bool UnblockPins { get; set; }

        [Option('q', HelpText = "Stop after one iteration")]
        public bool OneIterationOnly { get; set; }

        [Option('z', HelpText = "Specify a library to use (default is rtpkcs11ecp.dll). It's recommended to always use this option")]
        public string NativeLibraryPath { get; set; }

        [Option('M', Default = DefaultValues.MinAdminPinLength,
            HelpText = "Min Admin PIN code length (len must be in the range from 1 to 31; default: 6). Ignore for Rutoken S(use 1 only)")]
        public uint MinAdminPinLength { get; set; }

        [Option('m', Default = DefaultValues.MinUserPinLength, 
            HelpText = "Min User PIN code length (len must be in the range from 1 to 31; default: 6). Ignore for Rutoken S(use 1 only)")]
        public uint MinUserPinLength { get; set; }

        [Option('R', Default = DefaultValues.MaxAdminPinAttempts,
            HelpText = "Max PIN code attempts count for Admin PIN (count must be in the range from 3 to 10; default: 6)")]
        public uint MaxAdminPinAttempts { get; set; }

        [Option('r', Default = DefaultValues.MaxUserPinAttempts,
            HelpText = "Max PIN code attempts count for User PIN (count must be in the range from 3 to 10; default: 6)")]
        public uint MaxUserPinAttempts { get; set; }

        [Option('p', Default = DefaultValues.PinChangePolicy,
            HelpText = "PIN change policy N : { 1 - administrator (SO) can change user PIN | 2 - user can change user PIN | 3 - both } (default: 2)")]
        public RutokenFlag PinChangePolicy { get; set; }

        [Option('s',
            HelpText = "Set SM mode (only for Bluetooth token). " +
                        "N : " + "{ 1 - Optional password | 2 - 1 password | 3 - 6 passwords }, mode { caps - only capital letters | digits - capital letters and digits }")]
        public IEnumerable<string> SmMode { get; set; }

        [Option('F',
            HelpText = "Format volume. " +
                        "Id : volume id {1 - 9}, " +
                        "size : volume size in MB, " +
                        "owner : volume owner {a - administrator, u - user}, " +
                        "aR : access rights {ro, rw, hi, cd}. " +
                        "One command for one volume. To create multiple partitions, use the appropriate number of -F commands in sequence")]
        public IEnumerable<string> FormatVolumeParams { get; set; }

        [Option('C',
            HelpText = "Change volume attributes. " +
                        "Id : volume id {1 - 9}, " +
                        "aR : access rights{ro, rw, hi, cd}, " +
                        "cT : change type { p - permanent, t - temporary }")]
        public IEnumerable<string> ChangeVolumeAttributes { get; set; }

        [Option('i',
            HelpText = "Get volume info. " +
                        "Id : volume id {1 - 9, a - all, sz - get flash size}")]
        public IEnumerable<string> VolumeInfoParams { get; set; }

        [Option('O',
            HelpText = "PIN Login with local PIN. " +
                        "Id : local PIN id {1 - 9}, " +
                        "PIN : local PIN. In order to use local user set his new PIN preliminarily")]
        public IEnumerable<string> LoginWithLocalPin { get; set; }

        [Option('B',
            HelpText = "PIN Set local PIN. " +
                        "Id : local PIN id {1 - 9}, " +
                        "PIN : local PIN. You should use login with local PIN command to set local user PIN")]
        public IEnumerable<string> SetLocalPin { get; set; }
    }
}
