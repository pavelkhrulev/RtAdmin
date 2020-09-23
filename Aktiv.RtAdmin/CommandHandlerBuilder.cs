using Aktiv.RtAdmin.Properties;
using Microsoft.Extensions.Logging;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.Common;
using RutokenPkcs11Interop.HighLevelAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aktiv.RtAdmin
{
    public class CommandHandlerBuilder
    {
        private readonly ILogger _logger;
        private readonly RuntimeTokenParams _runtimeTokenParams;
        private readonly PinsStorage _pinsStorage;
        private readonly VolumeOwnersStore _volumeOwnersStore;
        private readonly VolumeAttributesStore _volumeAttributesStore;
        private readonly ConcurrentQueue<Action> _commands;
        private readonly ConcurrentQueue<Action> _prerequisites;
        private readonly LogMessageBuilder _logMessageBuilder;

        private Slot _slot;
        private CommandLineOptions _commandLineOptions;
        private CommandLineOptionsValidator _validator;

        public CommandHandlerBuilder(ILogger<RtAdmin> logger, RuntimeTokenParams runtimeTokenParams,
            PinsStorage pinsStorage, VolumeOwnersStore volumeOwnersStore, VolumeAttributesStore volumeAttributesStore,
            LogMessageBuilder logMessageBuilder)
        {
            _logger = logger;
            _runtimeTokenParams = runtimeTokenParams;
            _pinsStorage = pinsStorage;
            _volumeOwnersStore = volumeOwnersStore;
            _volumeAttributesStore = volumeAttributesStore;
            _logMessageBuilder = logMessageBuilder;

            _commands = new ConcurrentQueue<Action>();
            _prerequisites = new ConcurrentQueue<Action>();
        }

        public CommandHandlerBuilder ConfigureWith(Slot slot, CommandLineOptions options)
        {
            _slot = slot;
            _commandLineOptions = options;
            
            // Todo: Resolve через DI
            _validator = new CommandLineOptionsValidator(
                _commandLineOptions, _runtimeTokenParams, _volumeOwnersStore);

            if (!string.IsNullOrWhiteSpace(_commandLineOptions.TokenLabelCp1251))
            {
                _runtimeTokenParams.TokenLabel = _commandLineOptions.TokenLabelCp1251;
            }

            if (!string.IsNullOrWhiteSpace(_commandLineOptions.TokenLabelUtf8))
            {
                _runtimeTokenParams.TokenLabel = _commandLineOptions.TokenLabelUtf8;
            }

            var tokenInfo = slot.GetTokenInfo();
            _runtimeTokenParams.TokenSerial = tokenInfo.SerialNumber;
            _runtimeTokenParams.TokenSerialDecimal = Convert.ToInt64(_runtimeTokenParams.TokenSerial, 16).ToString();

            var tokenExtendedInfo = slot.GetTokenInfoExtended();

            _runtimeTokenParams.TokenType = tokenExtendedInfo.TokenType;
            // TODO: всегда ли тут разделитель точка?
            _runtimeTokenParams.HardwareMajorVersion = !string.IsNullOrWhiteSpace(tokenInfo.HardwareVersion) ?
                uint.Parse(tokenInfo.HardwareVersion.Substring(0, tokenInfo.HardwareVersion.IndexOf(".", StringComparison.OrdinalIgnoreCase))) :
                default;

            _runtimeTokenParams.OldUserPin = !string.IsNullOrWhiteSpace(_commandLineOptions.OldUserPin) ? 
                new PinCode(PinCodeOwner.User, _commandLineOptions.OldUserPin) : 
                new PinCode(PinCodeOwner.User);

            _runtimeTokenParams.OldAdminPin = !string.IsNullOrWhiteSpace(_commandLineOptions.OldAdminPin) ?
                new PinCode(PinCodeOwner.Admin, _commandLineOptions.OldAdminPin) :
                new PinCode(PinCodeOwner.Admin);

            _runtimeTokenParams.NewUserPin = !string.IsNullOrWhiteSpace(_commandLineOptions.UserPin) ?
                new PinCode(PinCodeOwner.User, _commandLineOptions.UserPin) :
                new PinCode(PinCodeOwner.User);

            _runtimeTokenParams.NewAdminPin = !string.IsNullOrWhiteSpace(_commandLineOptions.AdminPin) ?
                new PinCode(PinCodeOwner.Admin, _commandLineOptions.AdminPin) :
                new PinCode(PinCodeOwner.Admin);

            // TODO: сделать helper для битовых масок
            var adminCanChangeUserPin = (tokenExtendedInfo.Flags & (ulong)RutokenFlag.AdminChangeUserPin) == (ulong)RutokenFlag.AdminChangeUserPin;
            var userCanChangeUserPin = (tokenExtendedInfo.Flags & (ulong)RutokenFlag.UserChangeUserPin) == (ulong)RutokenFlag.UserChangeUserPin;

            _runtimeTokenParams.UserPinChangePolicy = UserPinChangePolicyFactory.Create(userCanChangeUserPin, adminCanChangeUserPin);

            _runtimeTokenParams.MinAdminPinLenFromToken = tokenExtendedInfo.MinAdminPinLen;
            _runtimeTokenParams.MaxAdminPinLenFromToken = tokenExtendedInfo.MaxAdminPinLen;
            _runtimeTokenParams.MinUserPinLenFromToken = tokenExtendedInfo.MinUserPinLen;
            _runtimeTokenParams.MaxUserPinLenFromToken = tokenExtendedInfo.MaxUserPinLen;

            _runtimeTokenParams.FlashMemoryAvailable = Convert.ToBoolean(tokenExtendedInfo.Flags & (uint) RutokenFlag.HasFlashDrive);

            return this;
        }

        public CommandHandlerBuilder WithFormat()
        {
            if (_commandLineOptions.ExcludedTokens.Contains($"0x{_runtimeTokenParams.TokenSerial}", StringComparer.OrdinalIgnoreCase) ||
                _commandLineOptions.ExcludedTokens.Contains(_runtimeTokenParams.TokenSerialDecimal, StringComparer.OrdinalIgnoreCase))
            {
                return this;
            }

            _prerequisites.Enqueue(() =>
            {
                ValidateNewPins(() =>
                {
                    _validator.ValidateFormatTokenParams();
                    _validator.ValidatePinsLengthBeforeFormat();
                });
            });

            _commands.Enqueue(() =>
            {
                try
                {
                    if (_runtimeTokenParams.TokenType == RutokenType.RUTOKEN &&
                        _runtimeTokenParams.HardwareMajorVersion == DefaultValues.RutokenS_InvalidHardwareMajorVersion)
                    {
                        throw new CKRException(CKR.CKR_GENERAL_ERROR);
                    }

                    var minAdminPinLength = _runtimeTokenParams.TokenType == RutokenType.RUTOKEN
                        ? DefaultValues.RutokenS_MinAdminPinLength
                        : _commandLineOptions.MinAdminPinLength;
                    var minUserPinLength = _runtimeTokenParams.TokenType == RutokenType.RUTOKEN
                        ? DefaultValues.RutokenS_MinUserPinLength
                        : _commandLineOptions.MinUserPinLength;

                    PinBlocker.Block(_slot, _runtimeTokenParams.TokenType);

                    TokenFormatter.Format(_slot,
                        _runtimeTokenParams.OldAdminPin.Value, _runtimeTokenParams.NewAdminPin.Value,
                        _runtimeTokenParams.NewUserPin.Value,
                        _runtimeTokenParams.TokenLabel,
                        (RutokenFlag)_commandLineOptions.PinChangePolicy,
                        minAdminPinLength, minUserPinLength,
                        _commandLineOptions.MaxAdminPinAttempts, _commandLineOptions.MaxUserPinAttempts, _runtimeTokenParams.SmMode);

                    Console.WriteLine(_logMessageBuilder.WithTokenIdSuffix(Resources.FormatTokenSuccess));
                    
                    if (!string.IsNullOrWhiteSpace(_commandLineOptions.PinFilePath))
                    {
                        _logger.LogInformation(_logMessageBuilder.WithFormatResult(Resources.FormatPassed));
                    }
                }
                catch
                {
                    Console.Error.WriteLine(_logMessageBuilder.WithTokenIdSuffix(Resources.FormatError));
                    _logger.LogError(_logMessageBuilder.WithFormatResult(Resources.FormatFailed));
                    throw;
                }

                if (_pinsStorage.Initialized && !_pinsStorage.CanGetNext)
                {
                    Console.WriteLine(Resources.PinCodesFilePinsHaveEnded);
                    throw new AppMustBeClosedException(0);
                }
            });
            
            return this;
        }

        public CommandHandlerBuilder WithNewlyGeneratedPin(PinCodeOwner pinCodeOwner)
        {
            uint pinLength;
            if (pinCodeOwner == PinCodeOwner.Admin)
            {
                _prerequisites.Enqueue(_validator.ValidatePinsLengthBeforeAdminPinGeneration);
                pinLength = _commandLineOptions.AdminPinLength.Value;
            }
            else
            {
                _prerequisites.Enqueue(_validator.ValidatePinsLengthBeforeUserPinGeneration);
                pinLength = _commandLineOptions.UserPinLength.Value;
            }

            _commands.Enqueue(() =>
            {
                try
                {
                    var generatedPin = new PinCode(pinCodeOwner, 
                        PinGenerator.Generate(_slot, _runtimeTokenParams.TokenType, pinLength));

                    if (pinCodeOwner == PinCodeOwner.Admin)
                    {
                        _runtimeTokenParams.NewAdminPin = generatedPin;
                    }
                    else
                    {
                        _runtimeTokenParams.NewUserPin = generatedPin;
                    }
                }
                catch
                {
                    Console.Error.WriteLine(Resources.PinGenerationError);
                    _logger.LogError(_logMessageBuilder.WithTokenId(Resources.PinGenerationFailed));

                    throw new TokenMustBeChangedException();
                }
            });

            return this;
        }

        public CommandHandlerBuilder WithNewTokenName()
        {
            _prerequisites.Enqueue(_validator.ValidateTokenNameChange);

            _commands.Enqueue(() =>
            {
                try
                {
                    TokenNameSetter.Set(_slot, _runtimeTokenParams.OldUserPin.Value,
                        _runtimeTokenParams.TokenLabel);

                    _logger.LogInformation(_logMessageBuilder.WithTokenId(Resources.TokenLabelChangeSuccess));
                }
                catch
                {
                    _logger.LogError(_logMessageBuilder.WithTokenId(Resources.TokenLabelChangeFailed));
                    throw;
                }
            });

            return this;
        }

        public CommandHandlerBuilder WithPinsChange()
        {
            void ChangeUserPin(PinCode ownerPinCode)
            {
                if (ownerPinCode.EnteredByUser)
                {
                    var changeBy = string.Empty;

                    try
                    {
                        if (ownerPinCode.Owner == PinCodeOwner.Admin)
                        {
                            changeBy = Resources.PinChangeByAdmin;

                            PinChanger.Change<UserPinByAdminChangeOperation>(_slot, 
                                ownerPinCode.Value, _runtimeTokenParams.NewUserPin.Value, CKU.CKU_SO);
                        }
                        else
                        {
                            PinChanger.Change<PinChangeOperation>(_slot,
                                ownerPinCode.Value, _runtimeTokenParams.NewUserPin.Value, CKU.CKU_USER);
                        }

                        Console.WriteLine(Resources.PinChangedSuccess);
                        _logger.LogInformation(_logMessageBuilder.WithTokenId(
                            string.Format(Resources.PinChangePassed, Resources.UserPinOwner,
                                changeBy,
                                ownerPinCode.Value,
                                _runtimeTokenParams.NewUserPin.Value)));
                    }
                    catch
                    {
                        Console.Error.WriteLine(Resources.ChangingPinError);
                        _logger.LogError(_logMessageBuilder.WithTokenId(
                                                   string.Format(Resources.PinChangeFailed, Resources.UserPinOwner)) +
                                                   $" {changeBy} : {ownerPinCode.Value} : {_runtimeTokenParams.NewUserPin.Value}");
                        throw;
                    }
                }
                else
                {
                    _logger.LogError(_logMessageBuilder.WithTokenId(
                        string.Format(Resources.PinChangeFailed, Resources.UserPinOwner)));
                    Console.Error.WriteLine(_logMessageBuilder.WithPolicyDescription(_runtimeTokenParams.UserPinChangePolicy));

                    throw new TokenMustBeChangedException();
                }
            }

            void ChangeAdminPin()
            {
                if (_runtimeTokenParams.OldAdminPin.EnteredByUser)
                {
                    try
                    {
                        PinChanger.Change<PinChangeOperation>(_slot,
                            _runtimeTokenParams.OldAdminPin.Value, _runtimeTokenParams.NewAdminPin.Value, CKU.CKU_SO);

                        Console.WriteLine(Resources.PinChangedSuccess);
                        _logger.LogInformation(_logMessageBuilder.WithTokenId(
                            string.Format(Resources.PinChangePassed, Resources.AdminPinOwner,
                                string.Empty,
                                _runtimeTokenParams.OldAdminPin.Value,
                                _runtimeTokenParams.NewAdminPin.Value)));
                    }
                    catch
                    {
                        Console.WriteLine(Resources.ChangingPinError);
                        _logger.LogError(_logMessageBuilder.WithTokenId(
                                                   string.Format(Resources.PinChangeFailed, Resources.AdminPinOwner)) +
                                               $" : { _runtimeTokenParams.OldAdminPin.Value} : {_runtimeTokenParams.NewAdminPin.Value}");
                        throw;
                    }
                }
                else
                {
                    _logger.LogError(_logMessageBuilder.WithTokenId(
                        string.Format(Resources.PinChangeFailed, Resources.AdminPinOwner)));
                    throw new ArgumentException(string.Format(Resources.AdminPinChangeError, Resources.AdminPin));
                }
            }

            _prerequisites.Enqueue(() =>
            {
                ValidateNewPins(() => _validator.ValidatePinsLengthBeforePinsChange());
            });

            _commands.Enqueue(() =>
            {
                if (_runtimeTokenParams.NewUserPin.EnteredByUser)
                {
                    switch (_runtimeTokenParams.UserPinChangePolicy)
                    {
                        case UserPinChangePolicy.ByUser:
                        {
                            ChangeUserPin(_runtimeTokenParams.OldUserPin);
                            break;
                        }

                        case UserPinChangePolicy.ByAdmin:
                        {
                            ChangeUserPin(_runtimeTokenParams.OldAdminPin);
                            break;
                        }

                        case UserPinChangePolicy.ByUserOrAdmin:
                        {
                            ChangeUserPin(_runtimeTokenParams.OldAdminPin.EnteredByUser
                                ? _runtimeTokenParams.OldAdminPin
                                : _runtimeTokenParams.OldUserPin);

                            break;
                        }
                    }
                }

                if (_runtimeTokenParams.NewAdminPin.EnteredByUser)
                {
                    ChangeAdminPin();
                }

                if (_pinsStorage.Initialized && !_pinsStorage.CanGetNext)
                {
                    Console.WriteLine(Resources.PinCodesFilePinsHaveEnded);
                    throw new AppMustBeClosedException(0);
                }
            });

            return this;
        }

        public CommandHandlerBuilder WithGenerationActivationPassword()
        {
            _prerequisites.Enqueue(_validator.ValidateGenerateActivationPasswordParams);

            _commands.Enqueue(() =>
            {
                try
                {
                    _logger.LogInformation(_logMessageBuilder.WithTokenId(Resources.GeneratingActivationPasswords));

                    foreach (var password in ActivationPasswordGenerator.Generate(_slot,
                        _runtimeTokenParams.OldAdminPin.Value,
                        _runtimeTokenParams.CharacterSet,
                        _runtimeTokenParams.SmMode))
                    {
                        _logger.LogInformation(Encoding.UTF8.GetString(password));
                    }

                    _logger.LogInformation(_logMessageBuilder.WithTokenId(Resources.ActivationPasswordsWereGenerated));
                }
                catch
                {
                    _logger.LogError(_logMessageBuilder.WithTokenId(Resources.ActivationPasswordsGenerationFailed));
                    throw;
                }
            });

            return this;
        }

        public CommandHandlerBuilder WithNewLocalPin()
        {
            _prerequisites.Enqueue(_validator.ValidateNewLocalPinParams);

            _commands.Enqueue(() =>
            {
                try
                {
                    PinChanger.ChangeLocalPin(_slot, _runtimeTokenParams.NewUserPin.EnteredByUser ?
                            _runtimeTokenParams.NewUserPin.Value : _runtimeTokenParams.OldUserPin.Value,
                        _runtimeTokenParams.NewLocalPin, _runtimeTokenParams.LocalIdToCreate);

                    _logger.LogInformation(_logMessageBuilder.WithTokenId(Resources.NewLocalPinSetSuccess));
                }
                catch
                {
                    _logger.LogError(_logMessageBuilder.WithTokenId(Resources.NewLocalPinSetFailed));
                    throw;
                }
            });

            return this;
        }

        public CommandHandlerBuilder WithUsingLocalPin()
        {
            _prerequisites.Enqueue(() =>
            {
                var commandParams = _commandLineOptions.LoginWithLocalPin.ToList();
                if (commandParams.Count % 2 != 0)
                {
                    throw new ArgumentException(Resources.NewLocalPinInvalidCommandParamsCount);
                }

                _runtimeTokenParams.LocalUserPins = new Dictionary<uint, string>();

                for (var i = 0; i < commandParams.Count; i+=2)
                {
                    var localPinParams = commandParams.Skip(i).Take(2).ToList();

                    if (!_volumeOwnersStore.TryGetOwnerId(localPinParams[0], out var localId))
                    {
                        throw new ArgumentException(Resources.NewLocalPinInvalidOwnerId);
                    }

                    _runtimeTokenParams.LocalUserPins.Add(localId, localPinParams[1]);
                }
            });

            return this;
        }

        public CommandHandlerBuilder WithNewPin2()
        {
            _commands.Enqueue(() =>
            {
                try
                {
                    PinChanger.ChangePin2(_slot, _volumeOwnersStore.GetPin2Id());

                    _logger.LogInformation(_logMessageBuilder.WithTokenId(Resources.Pin2SetSuccess));
                }
                catch
                {
                    _logger.LogError(_logMessageBuilder.WithTokenId(Resources.Pin2SetFailed));
                    throw;
                }
            });

            return this;
        }

        public CommandHandlerBuilder WithPinsUnblock()
        {
            _commands.Enqueue(() =>
            {
                try
                {
                    PinUnlocker.Unlock(_slot, _runtimeTokenParams.OldAdminPin.Value);

                    _logger.LogInformation(_logMessageBuilder.WithTokenId(Resources.PinUnlockSuccess));
                }
                catch
                {
                    _logger.LogError(_logMessageBuilder.WithTokenId(Resources.PinUnlockFailed));
                    throw;
                }
            });

            return this;
        }

        public CommandHandlerBuilder WithDriveFormat()
        {
            _prerequisites.Enqueue(CanUseFlashMemoryOperation);

            _commands.Enqueue(() =>
            {
                try
                {
                    var volumeInfos = FormatVolumeInfosFactory.Create(
                            _volumeOwnersStore,
                            _volumeAttributesStore, 
                            _commandLineOptions.FormatVolumeParams).ToList();

                    if (!_runtimeTokenParams.OldAdminPin.EnteredByUser)
                    {
                        Console.WriteLine(Resources.DefaultAdminPinWillBeUsed);
                    }

                    DriveFormatter.Format(_slot, _runtimeTokenParams.OldAdminPin.Value,
                        volumeInfos.Select(x => (VolumeFormatInfoExtended)x));

                    foreach (var volumeInfo in volumeInfos)
                    {
                        _logger.LogInformation(_logMessageBuilder.WithVolumeInfo(volumeInfo));
                    }
                }
                catch
                {
                    _logger.LogError(_logMessageBuilder.WithTokenId(Resources.DriveFormatVolumeCreateFailed));
                    throw;
                }
            });

            return this;
        }

        public CommandHandlerBuilder WithChangeVolumeAttributes()
        {
            _prerequisites.Enqueue(CanUseFlashMemoryOperation);

            _commands.Enqueue(() =>
            {
                try
                {
                    var volumesInfos = _slot.GetVolumesInfo();
                    var volumeAttributesList = ChangeVolumeAttributesParamsFactory.Create(
                                                _volumeAttributesStore,
                                                _commandLineOptions.ChangeVolumeAttributes,
                                                volumesInfos,
                                                _runtimeTokenParams).ToList();

                    foreach (var attributes in volumeAttributesList)
                    {
                        VolumeAttributeChanger.Change(_slot, attributes);
                        _logger.LogInformation(_logMessageBuilder.WithVolumeInfo(attributes));
                    }
                }
                catch
                {
                    _logger.LogError(_logMessageBuilder.WithTokenId(Resources.VolumeAccessModeChangeFailed));
                    throw;
                }
            });

            return this;
        }

        public CommandHandlerBuilder WithShowVolumeInfoParams()
        {
            _prerequisites.Enqueue(CanUseFlashMemoryOperation);

            // Todo: отрефакторить
            _commands.Enqueue(() =>
            {
                if (_commandLineOptions.VolumeInfoParams == "sz")
                {
                    try
                    {
                        var driveSize = _slot.GetDriveSize();
                        _logger.LogInformation(_logMessageBuilder.WithDriveSize(driveSize));
                    }
                    catch
                    {
                        _logger.LogError(_logMessageBuilder.WithTokenId(Resources.TotalDriveSizeFailed));
                        throw;
                    }
                }
                else
                {
                    try
                    {
                        var volumesInfo = _slot.GetVolumesInfo();
                        if (_commandLineOptions.VolumeInfoParams == "a")
                        {
                            foreach (var volumeInfo in volumesInfo)
                            {
                                _logger.LogInformation(_logMessageBuilder.WithVolumeInfo(volumeInfo, false));
                            }
                        }
                        else
                        {
                            if (uint.TryParse(_commandLineOptions.VolumeInfoParams, out var volumeId) && (volumeId >= 1 && volumeId <= 8))
                            {
                                var volumeInfo = volumesInfo.FirstOrDefault(x => x.VolumeId == volumeId);
                                _logger.LogInformation(volumeInfo != null
                                    ? _logMessageBuilder.WithVolumeInfo(volumeInfo, false)
                                    : _logMessageBuilder.WithTokenId(Resources.VolumeInfoNotFound));
                            }
                            else
                            {
                                throw new InvalidOperationException(_logMessageBuilder.WithTokenId(Resources.VolumeInfoInvalidVolumeId));
                            }
                        }
                    }
                    catch
                    {
                        _logger.LogError(_logMessageBuilder.WithTokenId(Resources.VolumeInfoGettingFailed));
                        throw;
                    }
                }
            });

            return this;
        }

        private void CanUseFlashMemoryOperation()
        {
            if (!_runtimeTokenParams.FlashMemoryAvailable)
            {
                throw new InvalidOperationException(Resources.FlashMemoryNotAvailable);
            }
        }

        private void ValidateNewPins(Action validationAction)
        {
            if (_pinsStorage.Initialized)
            {
                _runtimeTokenParams.NewUserPin = new PinCode(PinCodeOwner.User, _pinsStorage.GetNext());
                _runtimeTokenParams.NewAdminPin = new PinCode(PinCodeOwner.Admin, _pinsStorage.GetNext());

                validationAction();
            }
            else
            {
                validationAction();
            }
        }

        public void Execute()
        {
            foreach (var action in _prerequisites.Concat(_commands))
            {
                action?.Invoke();
            }
        }
    }
}
