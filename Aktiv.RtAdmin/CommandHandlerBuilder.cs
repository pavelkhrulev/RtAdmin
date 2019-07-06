using Aktiv.RtAdmin.Properties;
using Microsoft.Extensions.Logging;
using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.HighLevelAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Net.Pkcs11Interop.Common;
using RutokenPkcs11Interop.Common;

namespace Aktiv.RtAdmin
{
    public class CommandHandlerBuilder
    {
        private readonly ILogger _logger;
        private Slot _slot;
        private CommandLineOptions _commandLineOptions;
        private readonly TokenParams _tokenParams;
        private readonly PinsStore _pinsStore;
        private readonly VolumeOwnersStore _volumeOwnersStore;
        private readonly ConcurrentQueue<Action> _commands;
        private readonly LogMessageBuilder _logMessageBuilder;

        public CommandHandlerBuilder(ILogger<RtAdmin> logger, TokenParams tokenParams,
            PinsStore pinsStore, VolumeOwnersStore volumeOwnersStore, LogMessageBuilder logMessageBuilder)
        {
            _logger = logger;
            _tokenParams = tokenParams;
            _pinsStore = pinsStore;
            _volumeOwnersStore = volumeOwnersStore;
            _logMessageBuilder = logMessageBuilder;

            _commands = new ConcurrentQueue<Action>();
        }

        public CommandHandlerBuilder ConfigureWith(Slot slot, CommandLineOptions options)
        {
            _slot = slot;
            _commandLineOptions = options;

            if (!string.IsNullOrWhiteSpace(_commandLineOptions.TokenLabelCp1251))
            {
                _tokenParams.TokenLabel = _commandLineOptions.TokenLabelCp1251;
            }

            if (!string.IsNullOrWhiteSpace(_commandLineOptions.TokenLabelUtf8))
            {
                _tokenParams.TokenLabel = _commandLineOptions.TokenLabelUtf8;
            }

            var tokenInfo = slot.GetTokenInfo();
            _tokenParams.TokenSerial = tokenInfo.SerialNumber;
            _tokenParams.TokenSerialDecimal = Convert.ToInt64(_tokenParams.TokenSerial, 16).ToString();

            var tokenExtendedInfo = slot.GetTokenInfoExtended();

            _tokenParams.TokenType = tokenExtendedInfo.TokenType;
            // TODO: всегда ли тут разделитель точка?
            _tokenParams.HardwareMajorVersion = !string.IsNullOrWhiteSpace(tokenInfo.HardwareVersion) ?
                uint.Parse(tokenInfo.HardwareVersion.Substring(0, tokenInfo.HardwareVersion.IndexOf(".", StringComparison.OrdinalIgnoreCase))) :
                default;

            _tokenParams.OldUserPin = !string.IsNullOrWhiteSpace(_commandLineOptions.OldUserPin) ? 
                new PinCode(PinCodeOwner.User, _commandLineOptions.OldUserPin) : 
                new PinCode(PinCodeOwner.User);

            _tokenParams.OldAdminPin = !string.IsNullOrWhiteSpace(_commandLineOptions.OldAdminPin) ?
                new PinCode(PinCodeOwner.Admin, _commandLineOptions.OldAdminPin) :
                new PinCode(PinCodeOwner.Admin);

            _tokenParams.NewUserPin = !string.IsNullOrWhiteSpace(_commandLineOptions.UserPin) ?
                new PinCode(PinCodeOwner.User, _commandLineOptions.UserPin) :
                new PinCode(PinCodeOwner.User);

            _tokenParams.NewAdminPin = !string.IsNullOrWhiteSpace(_commandLineOptions.AdminPin) ?
                new PinCode(PinCodeOwner.Admin, _commandLineOptions.AdminPin) :
                new PinCode(PinCodeOwner.Admin);

            // TODO: сделать helper для битовых масок
            var adminCanChangeUserPin = (tokenExtendedInfo.Flags & (ulong)RutokenFlag.AdminChangeUserPin) == (ulong)RutokenFlag.AdminChangeUserPin;
            var userCanChangeUserPin = (tokenExtendedInfo.Flags & (ulong)RutokenFlag.UserChangeUserPin) == (ulong)RutokenFlag.UserChangeUserPin;

            _tokenParams.UserPinChangePolicy = UserPinChangePolicyFactory.Create(userCanChangeUserPin, adminCanChangeUserPin);

            _tokenParams.MinAdminPinLenFromToken = tokenExtendedInfo.MinAdminPinLen;
            _tokenParams.MaxAdminPinLenFromToken = tokenExtendedInfo.MaxAdminPinLen;
            _tokenParams.MinUserPinLenFromToken = tokenExtendedInfo.MinUserPinLen;
            _tokenParams.MaxUserPinLenFromToken = tokenExtendedInfo.MaxUserPinLen;

            return this;
        }

        public CommandHandlerBuilder WithFormat()
        {
            _commands.Enqueue(() =>
            {
                try
                {
                    if (_tokenParams.TokenType == RutokenType.RUTOKEN &&
                        _tokenParams.HardwareMajorVersion == DefaultValues.RutokenS_InvalidHardwareMajorVersion)
                    {
                        throw new CKRException(CKR.CKR_GENERAL_ERROR);
                    }

                    var minAdminPinLength = _tokenParams.TokenType == RutokenType.RUTOKEN
                        ? DefaultValues.MinAdminPinLength
                        : _commandLineOptions.MinAdminPinLength;
                    var minUserPinLength = _tokenParams.TokenType == RutokenType.RUTOKEN
                        ? DefaultValues.MinUserPinLength
                        : _commandLineOptions.MinUserPinLength;

                    TokenFormatter.Format(_slot,
                        _tokenParams.OldAdminPin.Value, _tokenParams.NewAdminPin.Value,
                        _tokenParams.NewUserPin.Value,
                        _tokenParams.TokenLabel,
                        (RutokenFlag)_commandLineOptions.PinChangePolicy,
                        minAdminPinLength, minUserPinLength,
                        _commandLineOptions.MaxAdminPinAttempts, _commandLineOptions.MaxUserPinAttempts, _tokenParams.SmMode);

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
                _tokenParams.NewAdminPin = new PinCode(PinCodeOwner.Admin, _pinsStore.GetNext());
                _tokenParams.NewUserPin = new PinCode(PinCodeOwner.User, _pinsStore.GetNext());
            });

            return this;
        }

        public CommandHandlerBuilder WithNewAdminPin()
        {
            _commands.Enqueue(() =>
            {
                if (!_commandLineOptions.AdminPinLength.HasValue)
                {
                    throw new ArgumentNullException(nameof(_commandLineOptions.AdminPinLength));
                }

                if (_commandLineOptions.AdminPinLength < _tokenParams.MinAdminPinLenFromToken ||
                    _commandLineOptions.AdminPinLength > _tokenParams.MaxAdminPinLenFromToken)
                {
                    throw new InvalidOperationException(string.Format(Resources.RandomAdminPinLengthMismatch, 
                        _tokenParams.MinAdminPinLenFromToken, _tokenParams.MaxAdminPinLenFromToken));
                }

                _tokenParams.NewAdminPin = new PinCode(PinCodeOwner.Admin, GeneratePin(_commandLineOptions.AdminPinLength.Value));
            });

            return this;
        }

        public CommandHandlerBuilder WithNewUserPin()
        {
            _commands.Enqueue(() =>
            {
                if (!_commandLineOptions.UserPinLength.HasValue)
                {
                    throw new ArgumentNullException(nameof(_commandLineOptions.UserPinLength));
                }

                if (_commandLineOptions.UserPinLength < _tokenParams.MinUserPinLenFromToken ||
                    _commandLineOptions.UserPinLength > _tokenParams.MaxUserPinLenFromToken)
                {
                    throw new InvalidOperationException(string.Format(Resources.RandomUserPinLengthMismatch,
                        _tokenParams.MinUserPinLenFromToken, _tokenParams.MaxUserPinLenFromToken));
                }

                _tokenParams.NewUserPin = new PinCode(PinCodeOwner.User, GeneratePin(_commandLineOptions.UserPinLength.Value));
            });

            return this;
        }

        public CommandHandlerBuilder WithNewUtf8TokenName()
        {
            _commands.Enqueue(() =>
            {
                if (_tokenParams.OldUserPin == null ||
                    !_tokenParams.OldUserPin.EnteredByUser)
                {
                    throw new InvalidOperationException(Resources.ChangeTokenLabelPinError);
                }

                TokenName.SetNew(_slot, _tokenParams.OldUserPin.Value, 
                    Helpers.StringToUtf8String(_tokenParams.TokenLabel));

                _logger.LogInformation(_logMessageBuilder.WithTokenId(Resources.TokenLabelChangeSuccess));
            });

            return this;
        }

        public CommandHandlerBuilder WithNewCp1251TokenName()
        {
            _commands.Enqueue(() =>
            {
                if (!_tokenParams.OldUserPin.EnteredByUser)
                {
                    throw new InvalidOperationException(Resources.ChangeTokenLabelPinError);
                }

                TokenName.SetNew(_slot, _tokenParams.OldUserPin.Value,
                    Helpers.StringToCp1251String(_tokenParams.TokenLabel));

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
                            PinChanger.ChangeUserPinByAdmin(_slot,
                                ownerPinCode.Value,
                                _tokenParams.NewUserPin.Value);
                            changeBy = Resources.PinChangeByAdmin;
                        }
                        else
                        {
                            PinChanger.Change(_slot,
                                ownerPinCode.Value,
                                _tokenParams.NewUserPin.Value,
                                PinCodeOwner.User);
                        }

                        _logger.LogInformation(_logMessageBuilder.WithTokenId(
                            string.Format(Resources.PinChangePassed, Resources.UserPinOwner,
                                changeBy,
                                ownerPinCode.Value,
                                _tokenParams.NewUserPin.Value)));
                        _logger.LogInformation(Resources.PinChangedSuccess);
                    }
                    catch
                    {
                        _logger.LogInformation(Resources.ChangingPinError);
                        _logger.LogInformation(_logMessageBuilder.WithTokenId(
                                                   string.Format(Resources.PinChangeFailed, Resources.UserPinOwner)) +
                                                   $"{changeBy} : {ownerPinCode.Value} : {_tokenParams.NewUserPin.Value}");
                        throw;
                    }
                }
                else
                {
                    _logger.LogError(_logMessageBuilder.WithTokenId(
                        string.Format(Resources.PinChangeFailed, Resources.UserPinOwner)));
                    _logger.LogError(_logMessageBuilder.WithPolicyDescription(_tokenParams.UserPinChangePolicy));
                }
            }

            void ChangeAdminPin()
            {
                if (_tokenParams.OldAdminPin.EnteredByUser)
                {
                    try
                    {
                        PinChanger.Change(_slot,
                            _tokenParams.OldAdminPin.Value, _tokenParams.NewAdminPin.Value,
                            PinCodeOwner.Admin);

                        _logger.LogInformation(_logMessageBuilder.WithTokenId(
                            string.Format(Resources.PinChangePassed, Resources.AdminPinOwner,
                                string.Empty,
                                _tokenParams.OldAdminPin.Value,
                                _tokenParams.NewAdminPin.Value)));
                        _logger.LogInformation(Resources.PinChangedSuccess);
                    }
                    catch
                    {
                        _logger.LogInformation(Resources.ChangingPinError);
                        _logger.LogInformation(_logMessageBuilder.WithTokenId(
                                                   string.Format(Resources.PinChangeFailed, Resources.AdminPinOwner)) +
                                               $" : { _tokenParams.OldAdminPin.Value} : {_tokenParams.NewAdminPin.Value}");
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

            _commands.Enqueue(() =>
            {
                if (_tokenParams.NewUserPin.EnteredByUser)
                {
                    switch (_tokenParams.UserPinChangePolicy)
                    {
                        case UserPinChangePolicy.ByUser:
                        {
                            ChangeUserPin(_tokenParams.OldUserPin);
                            break;
                        }

                        case UserPinChangePolicy.ByAdmin:
                        {
                            ChangeUserPin(_tokenParams.OldAdminPin);
                            break;
                        }

                        case UserPinChangePolicy.ByUserOrAdmin:
                        {
                            if (_tokenParams.OldAdminPin.EnteredByUser)
                            {
                                ChangeUserPin(_tokenParams.OldAdminPin);
                            }
                            else
                            {
                                ChangeUserPin(_tokenParams.OldUserPin);
                            }

                            break;
                        }
                    }
                }

                if (_tokenParams.NewAdminPin.EnteredByUser)
                {
                    ChangeAdminPin();
                }
            });

            return this;
        }

        public CommandHandlerBuilder WithGenerationActivationPassword()
        {
            _commands.Enqueue(() =>
            {
                var commandParams = _commandLineOptions.GenerateActivationPasswords.ToList();
                if (commandParams.Count != 2)
                {
                    // TODO: в ресурсы
                    throw new ArgumentException("Неверное число аргументов для генерации паролей активации");
                }

                _tokenParams.SmMode = uint.Parse(commandParams[0]);

                if (_tokenParams.SmMode < 1 || _tokenParams.SmMode > 3)
                {
                    // TODO: в ресурсы
                    throw new ArgumentException("Invalid SM mode! It must be from 1 to 3");
                }

                var symbolsMode = commandParams[1];
                ActivationPasswordCharacterSet characterSet;
                if (string.Equals(symbolsMode, "caps", StringComparison.OrdinalIgnoreCase))
                {
                    characterSet = ActivationPasswordCharacterSet.CapsOnly;
                }
                else if (string.Equals(symbolsMode, "digits", StringComparison.OrdinalIgnoreCase))
                {
                    characterSet = ActivationPasswordCharacterSet.CapsAndDigits;
                }
                else
                {
                    // TODO: в ресурсы
                    throw new ArgumentException("Неверный набор символов");
                }

                _logger.LogInformation(_logMessageBuilder.WithTokenId(Resources.GeneratingActivationPasswords));

                foreach (var password in ActivationPasswordGenerator.Generate(_slot, _tokenParams.OldAdminPin.Value, characterSet, _tokenParams.SmMode))
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

                LocalPinChanger.Change(_slot, _tokenParams.NewUserPin.EnteredByUser ?
                    _tokenParams.NewUserPin.Value : _tokenParams.OldUserPin.Value,
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

                _tokenParams.LocalUserPins = new Dictionary<uint, string>();

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

                    _tokenParams.LocalUserPins.Add(localId, localPin);
                }
            });

            return this;
        }

        public CommandHandlerBuilder WithNewPin2()
        {
            _commands.Enqueue(() =>
            {
                LocalPinChanger.Change(_slot, null, null, _volumeOwnersStore.GetPin2Id());

                _logger.LogInformation(_logMessageBuilder.WithTokenId(Resources.Pin2SetSuccess));
            });

            return this;
        }


        public CommandHandlerBuilder WithDriveFormat()
        {
            _commands.Enqueue(() =>
            {
                DriveFormat.Format(_slot, 
                    _tokenParams.NewAdminPin.EnteredByUser ?
                        _tokenParams.NewAdminPin.Value :
                        _tokenParams.OldAdminPin.Value,
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
                        _tokenParams));

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
                PinUnlocker.Unlock(_slot, PinCodeOwner.Admin, _tokenParams.OldAdminPin.Value);

                _logger.LogInformation(_logMessageBuilder.WithTokenId(Resources.PinUnlockSuccess));
            });

            return this;
        }

        public void Execute()
        {
            // Валидация введенных новых пин-кодов
            // TODO: вынести отсюда куда-то в другое место
            if ((_tokenParams.NewAdminPin.EnteredByUser &&
                    (_tokenParams.NewAdminPin.Length < _tokenParams.MinAdminPinLenFromToken) ||
                     _tokenParams.NewAdminPin.Length > _tokenParams.MaxAdminPinLenFromToken) ||
                _tokenParams.NewUserPin.EnteredByUser &&
                    (_tokenParams.NewUserPin.Length < _tokenParams.MinUserPinLenFromToken) ||
                    _tokenParams.NewUserPin.Length > _tokenParams.MaxUserPinLenFromToken)
            {
                throw new InvalidOperationException(string.Format(Resources.PinLengthMismatch, 
                    _tokenParams.MinAdminPinLenFromToken, _tokenParams.MaxAdminPinLenFromToken, 
                    _tokenParams.MinUserPinLenFromToken, _tokenParams.MaxUserPinLenFromToken));
            }

            foreach (var command in _commands)
            {
                command?.Invoke();
            }
        }

        private string GeneratePin(uint pinLength)
        {
            var tokenInfo = _slot.GetTokenInfoExtended();
            return PinGenerator.Generate(_slot, tokenInfo.TokenType, pinLength, _commandLineOptions.UTF8InsteadOfcp1251);
        }
    }
}
