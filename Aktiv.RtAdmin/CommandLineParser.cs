using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Aktiv.RtAdmin.Properties;
using Mono.Options;

namespace Aktiv.RtAdmin
{
    public static class CommandLineParser
    {
        public static CommandLineOptions Parse(string[] args)
        {
            var options = new CommandLineOptions();
            var extraOptions = new List<string>();
            var currentParameter = string.Empty;
            var shouldShowHelp = false;
            var shouldShowVersion = false;
            var showSetFormatOption = false;

            var tokenLabelCp1251ShouldBeSet = false;
            var tokenLabelUtf8ShouldBeSet = false;
            var pinCodesFileShouldBeSet = false;
            var volumeInfoParamShouldBeSet = false;
            var excludedTokenShouldBeSet = false;
            var nativeLibraryFileShouldBeSet = false;
            var configurationFileShouldBeSet = false;

            var set = new OptionSet
            {
                {"f", Resources.FormatTokenOption, v =>
                {
                    options.Format = v != null;
                }},
                {"G", Resources.GenerateRandomAdminPinOption, v =>
                {
                    currentParameter = "G";
                    options.AdminPinLength = DefaultValues.RandomPinLength;
                }},
                {"g", Resources.GenerateRandomUserPinOption, v =>
                {
                    currentParameter = "g";
                    options.UserPinLength = DefaultValues.RandomPinLength;
                }},
                {"L", Resources.TokenLabelCp1251Option, v =>
                {
                    currentParameter = "L";
                    tokenLabelCp1251ShouldBeSet = true;
                }},
                {"D", Resources.TokenLabelUtf8Option, v =>
                {
                    currentParameter = "D";
                    tokenLabelUtf8ShouldBeSet = true;
                }},
                {"o=", Resources.OldAdminPinOption, v =>
                {
                    options.OldAdminPin = v;
                }},
                {"c=", Resources.OldUserPinOption, v =>
                {
                    options.OldUserPin = v;
                }},

                {"a", Resources.AdminPinOption, v =>
                {
                    currentParameter = "a";
                    options.AdminPin = DefaultValues.AdminPin;
                }},
                {"u", Resources.UserPinOption, v =>
                {
                    currentParameter = "u";
                    options.UserPin = DefaultValues.UserPin;
                }},
                {"I", Resources.StdinPinsOption, v =>
                {
                    options.StdinPins = v != null;
                }},
                {"t", Resources.SetPin2ModeOption, v =>
                {
                    options.SetPin2Mode = v != null;
                }},
                {"n", Resources.ConfigurationFilePathOption, v =>
                {
                    configurationFileShouldBeSet = true;
                    currentParameter = "n";
                }},
                {"l", Resources.LogFilePathOption, v =>
                {
                    currentParameter = "l";
                    options.LogFilePath = DefaultValues.LogFilePath;
                }},

                {"b", Resources.PinFilePathOption, v =>
                {
                    currentParameter = "b";
                    pinCodesFileShouldBeSet = true;
                }},

                {"z", Resources.NativeLibraryPathOption, v =>
                {
                    currentParameter = "z";
                    nativeLibraryFileShouldBeSet = true;
                }},
                {"P", Resources.UnblockPinsOption, v =>
                {
                    options.UnblockPins = v != null;
                }},
                {"q", Resources.OneIterationOnlyOption, v =>
                {
                    options.OneIterationOnly = v != null;
                }},
                {"i", Resources.VolumeInfoParamsOption, v =>
                {
                    currentParameter = "i";
                    volumeInfoParamShouldBeSet = true;
                }},

                {"M", Resources.MinAdminPinLengthOption, v =>
                {
                    currentParameter = "M";
                    options.MinAdminPinLength = DefaultValues.MinAdminPinLength;
                    showSetFormatOption = true;
                }},
                {"m", Resources.MinUserPinLengthOption, v =>
                {
                    currentParameter = "m";
                    options.MinUserPinLength = DefaultValues.MinUserPinLength;
                    showSetFormatOption = true;
                }},
                {"R", Resources.MaxAdminPinAttemptsOption, v =>
                {
                    currentParameter = "R";
                    options.MaxAdminPinAttempts = DefaultValues.MaxAdminPinAttempts;
                    showSetFormatOption = true;
                }},
                {"r", Resources.MaxUserPinAttemptsOption, v =>
                {
                    currentParameter = "r";
                    options.MaxUserPinAttempts = DefaultValues.MaxUserPinAttempts;
                    showSetFormatOption = true;
                }},

                {"p", Resources.PinChangePolicyOption, v =>
                {
                    currentParameter = "p";
                    options.PinChangePolicy = (uint) DefaultValues.PinChangePolicy;
                    showSetFormatOption = true;
                }},

                {"s", Resources.SmModeOption, v => currentParameter = "s"},
                {"F", Resources.FormatVolumeParamsOption, v => currentParameter = "F"},
                {"C", Resources.ChangeVolumeAttributesOption, v => currentParameter = "C"},
                {"O", Resources.LoginWithLocalPinOption, v => currentParameter = "O"},
                {"B", Resources.SetLocalPinOption, v => currentParameter = "B"},
                {"E", Resources.ExcludedTokensOption, v => 
                {
                    currentParameter = "E";
                    excludedTokenShouldBeSet = true;
                    showSetFormatOption = true;
                }},

                {"U", Resources.Utf8Option, v => { options.UTF8InsteadOfcp1251 = v != null; }},

                { "h|help", Resources.ShowHelp, h => shouldShowHelp = h != null },
                { "v|version", Resources.ShowVersion, v => shouldShowVersion = v != null },

                { "<>", v => {
                    switch (currentParameter) {
                        case "s":
                            options.GenerateActivationPasswords.Add(v);
                            break;
                        case "F":
                            options.FormatVolumeParams.Add(v);
                            break;
                        case "C":
                            options.ChangeVolumeAttributes.Add(v);
                            break;
                        case "O":
                            options.LoginWithLocalPin.Add(v);
                            break;
                        case "B":
                            options.SetLocalPin.Add(v);
                            break;
                        case "E":
                            ParseExcludedTokenOption(v, options);
                            break;

                        case "g":
                            options.UserPinLength = ParseUint(v, currentParameter);
                            break;
                        case "G":
                            options.AdminPinLength = ParseUint(v, currentParameter);
                            break;

                        case "m":
                            options.MinUserPinLength = ParseUint(v, currentParameter);
                            break;
                        case "M":
                            options.MinAdminPinLength = ParseUint(v, currentParameter);
                            break;

                        case "r":
                            options.MaxUserPinAttempts = ParseUint(v, currentParameter);
                            break;
                        case "R":
                            options.MaxAdminPinAttempts = ParseUint(v, currentParameter);
                            break;

                        case "L":
                            options.TokenLabelCp1251 = v;
                            break;
                        case "D":
                            options.TokenLabelUtf8 = v;
                            break;

                        case "a":
                            options.AdminPin = v;
                            break;
                        case "u":
                            options.UserPin = v;
                            break;

                        case "b":
                            options.PinFilePath = v;
                            break;

                        case "i":
                            options.VolumeInfoParams = v;
                            break;

                        case "p":
                            options.PinChangePolicy = ParseUint(v, currentParameter);
                            break;

                        case "l":
                            options.LogFilePath = v;
                            break;

                        case "z":
                            options.NativeLibraryPath = v;
                            break;

                        case "n":
                            options.ConfigurationFilePath = v;
                            break;

                        default:
                            extraOptions.Add(v);
                            break;
                    }
                }}
            };

            if (!args.Any())
            {
                ShowHelp(set, 0);
            }

            try
            {
                set.Parse(args);
            }
            catch (AppMustBeClosedException)
            {
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                ShowHelp(set, -1);
            }

            if (extraOptions.Any())
            {
                Console.WriteLine($@"{Resources.IllegalOptions}: {string.Join(',', extraOptions)}");
                ShowHelp(set, -1);
            }

            if (shouldShowHelp)
            {
                ShowHelp(set, 0);
            }

            if (showSetFormatOption && !options.Format)
            {
                Console.WriteLine(Resources.ShouldSetFormatOption);
                throw new AppMustBeClosedException(-1);
            }

            if ((tokenLabelUtf8ShouldBeSet && string.IsNullOrWhiteSpace(options.TokenLabelUtf8)) ||
                (tokenLabelCp1251ShouldBeSet && string.IsNullOrWhiteSpace(options.TokenLabelCp1251)))
            {
                Console.WriteLine(Resources.TokenLabelEmpty);
                throw new AppMustBeClosedException(-1);
            }

            if (pinCodesFileShouldBeSet && string.IsNullOrWhiteSpace(options.PinFilePath))
            {
                Console.WriteLine(Resources.PinCodesFileNameEmpty);
                throw new AppMustBeClosedException(-1);
            }

            if (nativeLibraryFileShouldBeSet && string.IsNullOrWhiteSpace(options.NativeLibraryPath))
            {
                Console.WriteLine(Resources.NativeLibraryFileEmpty);
                throw new AppMustBeClosedException(-1);
            }

            if (nativeLibraryFileShouldBeSet && !string.IsNullOrWhiteSpace(options.NativeLibraryPath) 
                                             && !File.Exists(options.NativeLibraryPath))
            {
                Console.WriteLine(Resources.NativeLibraryFileNotExist);
                throw new AppMustBeClosedException(-1);
            }

            if (configurationFileShouldBeSet && string.IsNullOrWhiteSpace(options.ConfigurationFilePath))
            {
                Console.WriteLine(Resources.ConfigurationFileEmpty);
                throw new AppMustBeClosedException(-1);
            }

            if (volumeInfoParamShouldBeSet && string.IsNullOrWhiteSpace(options.VolumeInfoParams))
            {
                Console.WriteLine(Resources.VolumeInfoEmpty);
                throw new AppMustBeClosedException(-1);
            }

            if (excludedTokenShouldBeSet && !options.ExcludedTokens.Any())
            {
                Console.WriteLine(Resources.ExcludedTokenSerialEmpty);
                throw new AppMustBeClosedException(-1);
            }

            if (shouldShowVersion)
            {
                var executablePath = Process.GetCurrentProcess().MainModule.FileName;
                Console.WriteLine($@"{executablePath} {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion}");

                throw new AppMustBeClosedException(0);
            }


            if (options.StdinPins && !pinCodesFileShouldBeSet)
            {
                if (options.OldUserPin == "stdin") options.OldUserPin = GetPasswordFromConsole("Old User PIN:");
                if (options.UserPin == "stdin") options.UserPin = GetPasswordFromConsole("User PIN:");
                if (options.OldAdminPin == "stdin") options.OldUserPin = GetPasswordFromConsole("Old Admin PIN:");
                if (options.AdminPin == "stdin") options.AdminPin = GetPasswordFromConsole("Admin PIN:");
            }

            return options;
        }

        private static uint ParseUint(string value, string option)
        {
            if (uint.TryParse(value, out var parsed))
            {
                return parsed;
            }
            else
            {
                Console.WriteLine(Resources.ArgumentMustBeIntegerType, option);
                throw new AppMustBeClosedException(-1);
            }
        }

        private static void ParseExcludedTokenOption(string value, CommandLineOptions options)
        {
            // Удаляем ведущие нули во введенном серийном номере токена
            var sb = new StringBuilder();
            if (value.StartsWith("0x"))
            {
                var trimmed = value.Substring(2).TrimStart('0');
                sb.Append("0x");
                sb.Append(trimmed);
            }
            else
            {
                sb.Append(value.TrimStart('0'));
            }

            if (Regex.Match(value, @"^[0-9]+$").Success || Regex.Match(value, @"0[x][0-9a-fA-F]+").Success)
            {
                options.ExcludedTokens.Add(sb.ToString());
            }
            else
            {
                Console.WriteLine(Resources.ExcludedTokenSerialEmpty);
                throw new AppMustBeClosedException(-1);
            }
        }

        private static void ShowHelp(OptionSet optionSet, int retCode)
        {
            var executablePath = Process.GetCurrentProcess().MainModule.FileName;

            Console.WriteLine($@"{Resources.Usage} {executablePath} OPTIONS");

            Console.WriteLine(Resources.Examples);
            Console.WriteLine($@"  {executablePath} -o 87654321 -a 0987654321   # {Resources.ExampleChangeAdminPin}");
            Console.WriteLine($@"  {executablePath} -o 87654321                 # {Resources.ExampleChangeAdminPinWithDefault}");
            Console.WriteLine($@"  {executablePath} -c 12345678 -u 1234567890   # {Resources.ExampleChangeUserPin}");
            Console.WriteLine($@"  {executablePath} -f -G 10                    # {Resources.ExampleFormatWithAdminPinGeneration}");
            Console.WriteLine($@"  {executablePath} -f                          # {Resources.ExampleFormatWithDefaultPins}");

            Console.WriteLine(Resources.Options);
            optionSet.WriteOptionDescriptions(Console.Out);

            throw new AppMustBeClosedException(retCode);
        }

        public static string GetPasswordFromConsole(string displayMessage, char mask = '*')
        {
            Console.Write(displayMessage);

            var sb = new StringBuilder();
            ConsoleKeyInfo keyInfo;
            while ((keyInfo = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (!char.IsControl(keyInfo.KeyChar))
                {
                    sb.Append(keyInfo.KeyChar);
                    Console.Write(mask);
                }
                else if (keyInfo.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);

                    if (Console.CursorLeft == 0)
                    {
                        Console.SetCursorPosition(Console.BufferWidth - 1, Console.CursorTop - 1);
                        Console.Write(' ');
                        Console.SetCursorPosition(Console.BufferWidth - 1, Console.CursorTop - 1);
                    }
                    else Console.Write("\b \b");
                }
            }
            Console.WriteLine();
            return sb.ToString();
        } 
    }
}
