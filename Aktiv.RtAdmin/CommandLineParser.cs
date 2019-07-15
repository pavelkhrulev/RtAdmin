using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
            var set = new OptionSet
            {
                {"f", Resources.FormatTokenOption, v => { options.Format = v != null; }},
                {"G=", Resources.GenerateRandomAdminPinOption, (uint v) => { options.AdminPinLength = v; }},
                {"g=", Resources.GenerateRandomUserPinOption, (uint v) => { options.UserPinLength = v; }},
                {"L=", Resources.TokenLabelCp1251Option, v => { options.TokenLabelCp1251 = v; }},
                {"D=", Resources.TokenLabelUtf8Option, v => { options.TokenLabelUtf8 = v; }},
                {"o=", Resources.OldAdminPinOption, v => { options.OldAdminPin = v; }},
                {"c=", Resources.OldUserPinOption, v => { options.OldUserPin = v; }},
                {"a=", Resources.AdminPinOption, v => { options.AdminPin = v; }},
                {"u=", Resources.UserPinOption, v => { options.UserPin = v; }},
                {"t", Resources.SetPin2ModeOption, v => { options.SetPin2Mode = v != null; }},
                {"n=", Resources.ConfigurationFilePathOption, v => { options.ConfigurationFilePath = v; }},
                {"l=", Resources.LogFilePathOption, v => { options.LogFilePath = v; }},
                {"b=", Resources.PinFilePathOption, v => { options.PinFilePath = v; }},
                {"z=", Resources.NativeLibraryPathOption, v => { options.NativeLibraryPath = v; }},
                {"P", Resources.UnblockPinsOption, v => { options.UnblockPins = v != null; }},
                {"q", Resources.OneIterationOnlyOption, v => { options.OneIterationOnly = v != null; }},
                {"i=", Resources.VolumeInfoParamsOption, v => { options.VolumeInfoParams = v; }},

                {"M=", Resources.MinAdminPinLengthOption, (uint v) => { options.MinAdminPinLength = v; }},
                {"m=", Resources.MinUserPinLengthOption, (uint v) => { options.MinUserPinLength = v; }},
                {"R=", Resources.MaxAdminPinAttemptsOption, (uint v) => { options.MaxAdminPinAttempts = v; }},
                {"r=", Resources.MaxUserPinAttemptsOption, (uint v) => { options.MaxUserPinAttempts = v; }},
                {"p=", Resources.PinChangePolicyOption, (uint v) => { options.PinChangePolicy = v; }},

                {"s", Resources.SmModeOption, v => currentParameter = "s"},
                {"F", Resources.FormatVolumeParamsOption, v => currentParameter = "F"},
                {"C", Resources.ChangeVolumeAttributesOption, v => currentParameter = "C"},
                {"O", Resources.LoginWithLocalPinOption, v => currentParameter = "O"},
                {"B", Resources.SetLocalPinOption, v => currentParameter = "B"},
                {"E", Resources.ExcludedTokensOption, v => currentParameter = "E"},

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
                            options.ExcludedTokens.Add(v);
                            break;
                        default:
                            extraOptions.Add(v);
                            break;
                    }
                }}
            };

            try
            {
                if (!args.Any())
                {
                    ShowHelp(set);
                }

                set.Parse(args);

                if (extraOptions.Any())
                {
                    Console.WriteLine($@"{Resources.IllegalOptions}: {string.Join(',', extraOptions)}");
                    ShowHelp(set);
                }

                if (shouldShowHelp)
                {
                    ShowHelp(set);
                }

                if (shouldShowVersion)
                {
                    var executablePath =
                        $"{AppDomain.CurrentDomain.BaseDirectory}{Assembly.GetExecutingAssembly().GetName().Name}.exe";
                    Console.WriteLine($@"{executablePath} {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion}");

                    throw new AppMustBeClosedException();
                }
            }
            catch (OptionException e)
            {
                if (e.Message.Contains("missing required", StringComparison.OrdinalIgnoreCase))
                {
                    switch (e.OptionName)
                    {
                        case "-G":
                            options.AdminPinLength = DefaultValues.RandomPinLength;
                            break;
                        case "-g":
                            options.UserPinLength = DefaultValues.RandomPinLength;
                            break;
                        case "-a":
                            options.AdminPin = DefaultValues.AdminPin;
                            break;
                        case "-u":
                            options.UserPin = DefaultValues.UserPin;
                            break;

                        case "-M":
                            options.MinAdminPinLength = DefaultValues.MinAdminPinLength;
                            break;
                        case "-m":
                            options.MinUserPinLength = DefaultValues.MinUserPinLength;
                            break;
                        case "-R":
                            options.MaxAdminPinAttempts = DefaultValues.MaxAdminPinAttempts;
                            break;
                        case "-r":
                            options.MaxUserPinAttempts = DefaultValues.MaxUserPinAttempts;
                            break;
                        case "-p":
                            options.PinChangePolicy = (uint)DefaultValues.PinChangePolicy;
                            break;
                    }
                }
                else
                {
                    Console.WriteLine(e.Message);
                    ShowHelp(set);
                }
            }

            return options;
        }

        private static void ShowHelp(OptionSet optionSet)
        {
            var executablePath =
                $"{AppDomain.CurrentDomain.BaseDirectory}{Assembly.GetExecutingAssembly().GetName().Name}.exe";

            Console.WriteLine($@"{Resources.Usage} {executablePath} OPTIONS");

            Console.WriteLine(Resources.Examples);
            Console.WriteLine($@"  {executablePath} -o 87654321 -a 0987654321   # {Resources.ExampleChangeAdminPin}");
            Console.WriteLine($@"  {executablePath} -o 87654321                 # {Resources.ExampleChangeAdminPinWithDefault}");
            Console.WriteLine($@"  {executablePath} -c 12345678 -u 1234567890   # {Resources.ExampleChangeUserPin}");
            Console.WriteLine($@"  {executablePath} -f -G 10                    # {Resources.ExampleFormatWithAdminPinGeneration}");
            Console.WriteLine($@"  {executablePath} -f                          # {Resources.ExampleFormatWithDefaultPins}");

            Console.WriteLine(Resources.Options);
            optionSet.WriteOptionDescriptions(Console.Out);

            throw new AppMustBeClosedException();
        }
    }
}
