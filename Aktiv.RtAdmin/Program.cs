using Aktiv.RtAdmin.Properties;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aktiv.RtAdmin
{
    public class RtAdmin
    {
        private static IServiceProvider _serviceProvider;
        private static int _retCode;

        private static readonly Dictionary<Predicate<CommandLineOptions>, Action<CommandHandlerBuilder>> _optionsMapping =
            new Dictionary<Predicate<CommandLineOptions>, Action<CommandHandlerBuilder>>
            {
                {options => options.AdminPinLength.HasValue, builder => builder.WithNewlyGeneratedPin(PinCodeOwner.Admin)},
                {options => options.UserPinLength.HasValue, builder => builder.WithNewlyGeneratedPin(PinCodeOwner.User)},
                {options => !options.Format &&
                            !string.IsNullOrWhiteSpace(options.TokenLabelCp1251), builder => builder.WithNewTokenName()},
                {options => !options.Format &&
                            !string.IsNullOrWhiteSpace(options.TokenLabelUtf8), builder => builder.WithNewTokenName()},
                {options => !options.Format, builder => builder.WithPinsChange()},
                {options => options.Format, builder => builder.WithFormat()},
                {options => options.Format && options.GenerateActivationPasswords.Any(), builder => builder.WithGenerationActivationPassword()},
                {options => options.SetLocalPin.Any(), builder => builder.WithNewLocalPin()},
                {options => options.SetPin2Mode, builder => builder.WithNewPin2()},
                {options => options.FormatVolumeParams.Any(), builder => builder.WithDriveFormat()},
                {options => options.LoginWithLocalPin.Any(), builder => builder.WithUsingLocalPin()},
                {options => options.ChangeVolumeAttributes.Any(), builder => builder.WithChangeVolumeAttributes()},
                {options => !string.IsNullOrWhiteSpace(options.VolumeInfoParams), builder => builder.WithShowVolumeInfoParams()},
                {options => options.UnblockPins, builder => builder.WithPinsUnblock()}
            };

        static int Main(string[] args)
        {
            var arguments = Parser.Default.ParseArguments<CommandLineOptions>(args);

            arguments.WithParsed(options =>
            {
                _serviceProvider = Startup.Configure(options.LogFilePath);

                var core = _serviceProvider.GetService<TokenSlot>();
                var logger = _serviceProvider.GetService<ILogger<RtAdmin>>();
                var pinsStore = _serviceProvider.GetService<PinsStorage>();
                var configLinesStore = _serviceProvider.GetService<ConfigLinesStorage>();
                var logMessageBuilder = _serviceProvider.GetService<LogMessageBuilder>();

                if (!string.IsNullOrWhiteSpace(options.PinFilePath))
                {
                    pinsStore.Load(options.PinFilePath);
                }

                if (!string.IsNullOrWhiteSpace(options.ConfigurationFilePath))
                {
                    configLinesStore.Load(options.ConfigurationFilePath);
                    Main(configLinesStore.GetNext().Split(" "));
                }

                try
                {
                    var initialSlots = core.GetInitialSlots();

                    if (!initialSlots.Any())
                    {
                        logger.LogInformation(Resources.WaitingNextToken);
                    }

                    while (true)
                    {
                        if (!initialSlots.TryPop(out var slot))
                        {
                            slot = core.WaitToken();
                        }

                        var commandHandlerBuilder = _serviceProvider.GetService<CommandHandlerBuilder>()
                                                                    .ConfigureWith(slot, options);

                        if (pinsStore.Initialized)
                        {
                            commandHandlerBuilder.WithPinsFromStore();
                        }

                        foreach (var (key, value) in _optionsMapping)
                        {
                            if (key.Invoke(options))
                            {
                                value.Invoke(commandHandlerBuilder);
                            }
                        }

                        try
                        {
                            commandHandlerBuilder.Execute();
                        }
                        catch (TokenMustBeChangedException)
                        {
                            // ignored (перейти к следующему токену)
                        }

                        if (options.OneIterationOnly)
                        {
                            break;
                        }

                        logger.LogInformation(Resources.WaitingNextToken);
                    }

                    _retCode = (int)CKR.CKR_OK;
                }
                catch (Pkcs11Exception ex)
                {
                    logger.LogError(logMessageBuilder.WithPKCS11Error(ex.RV));
                    _retCode = (int)ex.RV;
                }
                catch (CKRException ex)
                {
                    logger.LogError(ex.Message);
                    _retCode = (int)ex.ReturnCode;
                }
                catch (Exception ex)
                {
                    logger.LogError(logMessageBuilder.WithUnhandledError(ex.Message));
                    _retCode = -1;
                }
                finally
                {
                    DisposeServices();
                }
            });

            return _retCode;
        }

        private static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }

            var pkcs11 = _serviceProvider.GetService<Pkcs11>();
            pkcs11.Dispose();

            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
