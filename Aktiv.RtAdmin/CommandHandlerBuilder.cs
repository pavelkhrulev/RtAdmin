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
        private Slot _slot;
        private CommandLineOptions _commandLineOptions;
        private readonly RuntimeTokenParams _runtimeTokenParams;
        private readonly PinsStorage _pinsStorage;
        private readonly VolumeOwnersStore _volumeOwnersStore;
        private readonly ConcurrentQueue<Action> _commands;
        private readonly ConcurrentQueue<Action> _prerequisites;
        private readonly LogMessageBuilder _logMessageBuilder;

        public CommandHandlerBuilder(ILogger<RtAdmin> logger, RuntimeTokenParams runtimeTokenParams,
            PinsStorage pinsStorage, VolumeOwnersStore volumeOwnersStore, LogMessageBuilder logMessageBuilder)
        {
            _logger = logger;
            _runtimeTokenParams = runtimeTokenParams;
            _pinsStorage = pinsStorage;
            _volumeOwnersStore = volumeOwnersStore;
            _logMessageBuilder = logMessageBuilder;

            _commands = new ConcurrentQueue<Action>();
            _prerequisites = new ConcurrentQueue<Action>();
        }

        public CommandHandlerBuilder ConfigureWith(Slot slot, CommandLineOptions options)
        {
            _slot = slot;
            _commandLineOptions = options;

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

            return this;
        }

        public CommandHandlerBuilder WithFormat()
        {
            _prerequisites.Enqueue(() =>
            {
                ValidateFormatTokenParams();
                ValidatePinsLengthBeforeFormat();
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

                    if (_commandLineOptions.ExcludedTokens.Contains(_runtimeTokenParams.TokenSerial, StringComparer.OrdinalIgnoreCase) ||
                        _commandLineOptions.ExcludedTokens.Contains(_runtimeTokenParams.TokenSerialDecimal, StringComparer.OrdinalIgnoreCase))
                    {
                        return;
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

                    _logger.LogInformation(_logMessageBuilder.WithTokenIdSuffix(Resources.FormatTokenSuccess));
                    _logger.LogInformation(_logMessageBuilder.WithFormatResult(Resources.FormatPassed));
                }
                catch
                {
                    _logger.LogError(_logMessageBuilder.WithTokenIdSuffix(Resources.FormatError));
                    _logger.LogError(_logMessageBuilder.WithFormatResult(Resources.FormatFailed));
                    throw;
                }
            });
            
            return this;
        }

        public CommandHandlerBuilder WithPinsFromStore()
        {
            _commands.Enqueue(() =>
            {
                _runtimeTokenParams.NewAdminPin = new PinCode(PinCodeOwner.Admin, _pinsStorage.GetNext());
                _runtimeTokenParams.NewUserPin = new PinCode(PinCodeOwner.User, _pinsStorage.GetNext());
            });

            return this;
        }

        public CommandHandlerBuilder WithNewAdminPin()
        {
            _prerequisites.Enqueue(ValidatePinsLengthBeforeAdminPinGeneration);

            _commands.Enqueue(() =>
            {
                _runtimeTokenParams.NewAdminPin =
                    new PinCode(PinCodeOwner.Admin, GeneratePin(_commandLineOptions.AdminPinLength.Value));
            });

            return this;
        }

        public CommandHandlerBuilder WithNewUserPin()
        {
            _prerequisites.Enqueue(ValidatePinsLengthBeforeUserPinGeneration);

            _commands.Enqueue(() =>
            {
                _runtimeTokenParams.NewUserPin = 
                    new PinCode(PinCodeOwner.User, GeneratePin(_commandLineOptions.UserPinLength.Value));
            });

            return this;
        }

        public CommandHandlerBuilder WithNewTokenName()
        {
            _commands.Enqueue(() =>
            {
                if (_runtimeTokenParams.OldUserPin == null ||
                    !_runtimeTokenParams.OldUserPin.EnteredByUser)
                {
                    throw new InvalidOperationException(Resources.ChangeTokenLabelPinError);
                }

                TokenNameSetter.Set(_slot, _runtimeTokenParams.OldUserPin.Value, 
                    _runtimeTokenParams.TokenLabel);

                _logger.LogInformation(_logMessageBuilder.WithTokenId(Resources.TokenLabelChangeSuccess));
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

                        _logger.LogInformation(_logMessageBuilder.WithTokenId(
                            string.Format(Resources.PinChangePassed, Resources.UserPinOwner,
                                changeBy,
                                ownerPinCode.Value,
                                _runtimeTokenParams.NewUserPin.Value)));
                        _logger.LogInformation(Resources.PinChangedSuccess);
                    }
                    catch
                    {
                        _logger.LogInformation(Resources.ChangingPinError);
                        _logger.LogInformation(_logMessageBuilder.WithTokenId(
                                                   string.Format(Resources.PinChangeFailed, Resources.UserPinOwner)) +
                                                   $"{changeBy} : {ownerPinCode.Value} : {_runtimeTokenParams.NewUserPin.Value}");
                        throw;
                    }
                }
                else
                {
                    _logger.LogError(_logMessageBuilder.WithTokenId(
                        string.Format(Resources.PinChangeFailed, Resources.UserPinOwner)));
                    _logger.LogError(_logMessageBuilder.WithPolicyDescription(_runtimeTokenParams.UserPinChangePolicy));
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

                        _logger.LogInformation(_logMessageBuilder.WithTokenId(
                            string.Format(Resources.PinChangePassed, Resources.AdminPinOwner,
                                string.Empty,
                                _runtimeTokenParams.OldAdminPin.Value,
                                _runtimeTokenParams.NewAdminPin.Value)));
                        _logger.LogInformation(Resources.PinChangedSuccess);
                    }
                    catch
                    {
                        _logger.LogInformation(Resources.ChangingPinError);
                        _logger.LogInformation(_logMessageBuilder.WithTokenId(
                                                   string.Format(Resources.PinChangeFailed, Resources.AdminPinOwner)) +
                                               $" : { _runtimeTokenParams.OldAdminPin.Value} : {_runtimeTokenParams.NewAdminPin.Value}");
                        throw;
                    }
                }
                else
                {
                    _logger.LogError(_logMessageBuilder.WithTokenId(
                        string.Format(Resources.PinChangeFailed, Resources.AdminPinOwner)));
                    _logger.LogError(string.Format(Resources.AdminPinChangeError, Resources.AdminPin));
                }
            }

            _prerequisites.Enqueue(ValidatePinsLengthBeforePinsChange);

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
            });

            return this;
        }

        public CommandHandlerBuilder WithGenerationActivationPassword()
        {
            _prerequisites.Enqueue(ValidateGenerateActivationPasswordParams);

            _commands.Enqueue(() =>
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
            });

            return this;
        }

        public CommandHandlerBuilder WithNewLocalPin()
        {
            _commands.Enqueue(() =>
            {
                var commandParams = _commandLineOptions.SetLocalPin.ToList();
                if (commandParams.Count != 2)
                {
                    // TODO: в ресурсы
                    throw new ArgumentException("Неверное число аргументов для установки локального PIN-кода");
                }

                if (!_volumeOwnersStore.TryGetOwnerId(commandParams[0], out var localIdToCreate))
                {
                    // TODO: в ресурсы
                    throw new ArgumentException("Неверный идентификатор локального пользователя");
                }

                var localPin = commandParams[1];

                PinChanger.ChangeLocalPin(_slot, _runtimeTokenParams.NewUserPin.EnteredByUser ?
                    _runtimeTokenParams.NewUserPin.Value : _runtimeTokenParams.OldUserPin.Value,
                    localPin, localIdToCreate);

                _logger.LogInformation(_logMessageBuilder.WithTokenId(Resources.LocalPinSetSuccess));
            });

            return this;
        }

        public CommandHandlerBuilder WithUsingLocalPin()
        {
            _commands.Enqueue(() =>
            {
                var commandParams = _commandLineOptions.LoginWithLocalPin.ToList();
                if (commandParams.Count % 2 != 0)
                {
                    // TODO: в ресурсы
                    throw new ArgumentException("Неверное число аргументов для использования локального PIN-кода");
                }

                _runtimeTokenParams.LocalUserPins = new Dictionary<uint, string>();

                // TODO: вынести в фабрику
                for (var i = 0; i < commandParams.Count; i+=2)
                {
                    var localPinParams = commandParams.Skip(i).Take(2).ToList();

                    if (!_volumeOwnersStore.TryGetOwnerId(localPinParams[0], out var localId))
                    {
                        // TODO: в ресурсы
                        throw new ArgumentException("Неверный идентификатор локального пользователя");
                    }

                    var localPin = localPinParams[1];

                    _runtimeTokenParams.LocalUserPins.Add(localId, localPin);
                }
            });

            return this;
        }

        public CommandHandlerBuilder WithNewPin2()
        {
            _commands.Enqueue(() =>
            {
                // TODO: доделать либу, чтобы можно было корректно обрабатывать пустые пины
                PinChanger.ChangeLocalPin(_slot, null, null, _volumeOwnersStore.GetPin2Id());

                _logger.LogInformation(_logMessageBuilder.WithTokenId(Resources.Pin2SetSuccess));
            });

            return this;
        }

        public CommandHandlerBuilder WithDriveFormat()
        {
            _commands.Enqueue(() =>
            {
                DriveFormatter.Format(_slot, 
                    _runtimeTokenParams.NewAdminPin.EnteredByUser ?
                        _runtimeTokenParams.NewAdminPin.Value :
                        _runtimeTokenParams.OldAdminPin.Value,
                        VolumeInfosFactory.Create(_commandLineOptions.FormatVolumeParams).ToList()
                    );

                _logger.LogInformation("Флешка отформатирована");
            });

            return this;
        }

        public CommandHandlerBuilder WithChangeVolumeAttributes()
        {
            _commands.Enqueue(() =>
            {
                var volumesInfo = _slot.GetVolumesInfo();

                VolumeAttributeChanger.Change(_slot,
                    ChangeVolumeAttributesParamsFactory.Create(
                        _commandLineOptions.ChangeVolumeAttributes,
                        volumesInfo,
                        _runtimeTokenParams));

                _logger.LogInformation("Аттрибуты раздела изменены");
            });

            return this;
        }

        public CommandHandlerBuilder WithShowVolumeInfoParams()
        {
            _commands.Enqueue(() =>
            {
                
                var volumesInfo = _slot.GetVolumesInfo();
                var driveSize = _slot.GetDriveSize();

                _logger.LogInformation("Аттрибуты раздела");
            });

            return this;
        }

        public CommandHandlerBuilder WithPinsUnblock()
        {
            _commands.Enqueue(() =>
            {
                PinUnlocker.Unlock(_slot, _runtimeTokenParams.OldAdminPin.Value);

                _logger.LogInformation(_logMessageBuilder.WithTokenId(Resources.PinUnlockSuccess));
            });

            return this;
        }

        private void ValidateFormatTokenParams()
        {
            if (_commandLineOptions.MinUserPinLength < DefaultValues.MinAllowedMinimalUserPinLength ||
                _commandLineOptions.MinUserPinLength > DefaultValues.MaxAllowedMinimalUserPinLength)
            {
                throw new ArgumentException(Resources.InvalidMinimalUserPinLength);
            }

            if (_commandLineOptions.MinAdminPinLength < DefaultValues.MinAllowedMinimalAdminPinLength ||
                _commandLineOptions.MinAdminPinLength > DefaultValues.MaxAllowedMinimalAdminPinLength)
            {
                throw new ArgumentException(Resources.InvalidMinimalAdminPinLength);
            }

            if (_commandLineOptions.MaxUserPinAttempts < DefaultValues.MinAllowedMaxUserPinAttempts ||
                _commandLineOptions.MaxUserPinAttempts > DefaultValues.MaxAllowedMaxUserPinAttempts)
            {
                throw new ArgumentException(Resources.InvalidMaxUserPinRetryCount);
            }

            if (_commandLineOptions.MaxAdminPinAttempts < DefaultValues.MinAllowedMaxAdminPinAttempts ||
                _commandLineOptions.MaxAdminPinAttempts > DefaultValues.MaxAllowedMaxAdminPinAttempts)
            {
                throw new ArgumentException(Resources.InvalidMaxAdminPinRetryCount);
            }

            if (!Enum.IsDefined(typeof(UserPinChangePolicy), _commandLineOptions.PinChangePolicy))
            {
                throw new ArgumentException(Resources.InvalidPolicyValue);
            }
        }

        private void ValidateGenerateActivationPasswordParams()
        {
            var commandParams = _commandLineOptions.GenerateActivationPasswords.ToList();
            if (commandParams.Count != DefaultValues.GenerateActivationPasswordCommandParamsCount)
            {
                throw new ArgumentException(Resources.ActivationPasswordInvalidCommandParamsCount);
            }

            if (!(uint.TryParse(commandParams[0], out var smMode)))
            {
                throw new ArgumentException(Resources.ActivationPasswordInvalidSmMode);
            }

            if (smMode < DefaultValues.MinAllowedSmMode ||
                smMode > DefaultValues.MaxAllowedSmMode)
            {
                throw new ArgumentException(Resources.ActivationPasswordInvalidSmMode);
            }

            var symbolsMode = commandParams[1];
            if (string.Equals(symbolsMode, DefaultValues.CapsCharacterSet, StringComparison.OrdinalIgnoreCase))
            {
                _runtimeTokenParams.CharacterSet = ActivationPasswordCharacterSet.CapsOnly;
            }
            else if (string.Equals(symbolsMode, DefaultValues.DigitsCharacterSet, StringComparison.OrdinalIgnoreCase))
            {
                _runtimeTokenParams.CharacterSet = ActivationPasswordCharacterSet.CapsAndDigits;
            }
            else
            {
                throw new ArgumentException(Resources.ActivationPasswordInvalidCharacterSet);
            }

            _runtimeTokenParams.SmMode = smMode;
        }

        private void ValidatePinsLengthBeforeFormat()
        {
            if ((_runtimeTokenParams.NewAdminPin.EnteredByUser &&
                 _runtimeTokenParams.NewAdminPin.Length < _commandLineOptions.MinAdminPinLength) ||
                (_runtimeTokenParams.NewUserPin.EnteredByUser &&
                _runtimeTokenParams.NewUserPin.Length < _commandLineOptions.MinUserPinLength))
            {
                throw new ArgumentException(string.Format(Resources.PinLengthMismatchBeforeFormat,
                    _runtimeTokenParams.MinAdminPinLenFromToken, _runtimeTokenParams.MaxAdminPinLenFromToken,
                    _runtimeTokenParams.MinUserPinLenFromToken, _runtimeTokenParams.MaxUserPinLenFromToken));
            }
        }

        private void ValidatePinsLengthBeforePinsChange()
        {
            if ((_runtimeTokenParams.NewAdminPin.EnteredByUser &&
                 (_runtimeTokenParams.NewAdminPin.Length < _runtimeTokenParams.MinAdminPinLenFromToken) ||
                 _runtimeTokenParams.NewAdminPin.Length > _runtimeTokenParams.MaxAdminPinLenFromToken) ||
                _runtimeTokenParams.NewUserPin.EnteredByUser &&
                (_runtimeTokenParams.NewUserPin.Length < _runtimeTokenParams.MinUserPinLenFromToken) ||
                _runtimeTokenParams.NewUserPin.Length > _runtimeTokenParams.MaxUserPinLenFromToken)
            {
                throw new ArgumentException(string.Format(Resources.PinLengthMismatch,
                    _runtimeTokenParams.MinAdminPinLenFromToken, _runtimeTokenParams.MaxAdminPinLenFromToken,
                    _runtimeTokenParams.MinUserPinLenFromToken, _runtimeTokenParams.MaxUserPinLenFromToken));
            }

            if (_runtimeTokenParams.NewUserPin.EnteredByUser && (!_runtimeTokenParams.OldUserPin.EnteredByUser &&
                                                                 !_runtimeTokenParams.OldAdminPin.EnteredByUser))
            {
                throw new ArgumentException(Resources.ChangeUserPinOldPinsError);
            }

            if (_runtimeTokenParams.NewAdminPin.EnteredByUser && !_runtimeTokenParams.OldAdminPin.EnteredByUser)
            {
                throw new ArgumentException(Resources.ChangeAdminPinOldPinError);
            }
        }

        private void ValidatePinsLengthBeforeAdminPinGeneration()
        {
            if ((_commandLineOptions.AdminPinLength.HasValue &&
                (_commandLineOptions.AdminPinLength < _runtimeTokenParams.MinAdminPinLenFromToken) ||
                 _commandLineOptions.AdminPinLength > _runtimeTokenParams.MaxAdminPinLenFromToken))
            {
                throw new ArgumentException(string.Format(Resources.RandomAdminPinLengthMismatch,
                    _runtimeTokenParams.MinAdminPinLenFromToken, _runtimeTokenParams.MaxAdminPinLenFromToken));
            }
        }

        private void ValidatePinsLengthBeforeUserPinGeneration()
        {
            if ((_commandLineOptions.UserPinLength.HasValue &&
                 (_commandLineOptions.UserPinLength < _runtimeTokenParams.MinUserPinLenFromToken) ||
                 _commandLineOptions.UserPinLength > _runtimeTokenParams.MaxUserPinLenFromToken))
            {
                throw new ArgumentException(string.Format(Resources.RandomUserPinLengthMismatch,
                    _runtimeTokenParams.MinUserPinLenFromToken, _runtimeTokenParams.MaxUserPinLenFromToken));
            }
        }

        public void Execute()
        {
            foreach (var action in _prerequisites.Concat(_commands))
            {
                action?.Invoke();
            }
        }

        private string GeneratePin(uint pinLength)
        {
            var tokenInfo = _slot.GetTokenInfoExtended();
            return PinGenerator.Generate(_slot, tokenInfo.TokenType, pinLength, _commandLineOptions.UTF8InsteadOfcp1251);
        }
    }
}
