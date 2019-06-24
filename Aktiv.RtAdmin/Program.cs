using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using CommandLine.Text;

namespace Aktiv.RtAdmin
{
    public class RtAdmin
    {
        private static IServiceProvider serviceProvider;
        private static int retCode;

        static int Main(string[] args)
        {
            var arguments = Parser.Default.ParseArguments<CommandLineOptions>(args);

            //arguments.WithNotParsed(errs =>
            //{
            //    var helpText = HelpText.AutoBuild(arguments, onError =>
            //    {
            //        var nHelpText = new HelpText(SentenceBuilder.Create(), "Ошибочка вышла!!!")
            //        {
            //            AdditionalNewLineAfterOption = false,
            //            AddDashesToOption = true,
            //            MaximumDisplayWidth = 4000
            //        };
            //        nHelpText.AddOptions(arguments);
            //        return HelpText.DefaultParsingErrorsHandler(arguments, nHelpText);
            //    },
            //    onExample => onExample);
            //    Console.Error.WriteLine(helpText);
            //});

            arguments.WithParsed(options =>
            {
                serviceProvider = Startup.Configure(options.LogFilePath);

                var core = serviceProvider.GetService<RutokenCore>();
                var logger = serviceProvider.GetService<ILogger<RtAdmin>>();
                var pinsStore = serviceProvider.GetService<PinsStore>();

                if (!string.IsNullOrWhiteSpace(options.PinFilePath))
                {
                    pinsStore.Load(options.PinFilePath);
                }

                try
                {
                    while (true)
                    {
                        var slot = core.WaitToken();

                        var commandHandlerBuilder = serviceProvider.GetService<CommandHandlerBuilder>()
                                                                   .ConfigureWith(slot, options);

                        if (pinsStore.Initialized)
                        {
                            commandHandlerBuilder.WithPinsFromStore();
                        }

                        // TODO: length validation
                        if (options.AdminPinLength.HasValue)
                        {
                            commandHandlerBuilder.WithNewAdminPin();
                        }

                        // TODO: length validation
                        if (options.UserPinLength.HasValue)
                        {
                            commandHandlerBuilder.WithNewUserPin();
                        }

                        if (!options.Format && 
                            !string.IsNullOrWhiteSpace(options.TokenLabelCp1251))
                        {
                            commandHandlerBuilder.WithNewCp1251TokenName();
                        }

                        if (!options.Format &&
                            !string.IsNullOrWhiteSpace(options.TokenLabelUtf8))
                        {
                            commandHandlerBuilder.WithNewUtf8TokenName();
                        }

                        if (!options.Format)
                        {
                            commandHandlerBuilder.WithPinsChange();
                        }

                        if (options.Format)
                        {
                            commandHandlerBuilder.WithFormat();
                        }

                        if (options.GenerateActivationPasswords.Any())
                        {
                            commandHandlerBuilder.WithGenerationActivationPassword();
                        }

                        if (options.SetLocalPin.Any())
                        {
                            commandHandlerBuilder.WithNewLocalPin();
                        }

                        if (options.SetPin2Mode)
                        {
                            commandHandlerBuilder.WithNewPin2();
                        }

                        commandHandlerBuilder.Execute();

                        if (options.OneIterationOnly)
                        {
                            break;
                        }
                    }

                    retCode = (int)CKR.CKR_OK;
                }
                catch (Pkcs11Exception ex)
                {
                    logger.LogError($"Operation failed [Method: {ex.Method}, RV: {ex.RV}]");
                    retCode = (int)ex.RV;
                }
                catch (Exception ex)
                {
                    logger.LogError($"Operation failed [Message: {ex.Message}]");
                    retCode = -1;
                }
                finally
                {
                    DisposeServices();
                }
            });

            return retCode;
        }

        private static void DisposeServices()
        {
            if (serviceProvider == null)
            {
                return;
            }

            var pkcs11 = serviceProvider.GetService<Pkcs11>();
            pkcs11.Dispose();

            if (serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
