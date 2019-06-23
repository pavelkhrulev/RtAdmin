using Aktiv.RtAdmin.Properties;
using Microsoft.Extensions.Logging;
using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.HighLevelAPI;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
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
        private readonly ConcurrentQueue<Action> _commands;

        public CommandHandlerBuilder(ILogger<RtAdmin> logger, TokenParams tokenParams, PinsStore pinsStore)
        {
            _logger = logger;
            _tokenParams = tokenParams;
            _pinsStore = pinsStore;

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

            var tokenExtendedInfo = slot.GetTokenInfoExtended();

            _tokenParams.OldUserPin = !string.IsNullOrWhiteSpace(_commandLineOptions.OldUserPin) ? 
                new PinCode(_commandLineOptions.OldUserPin) : 
                new PinCode(PinCodeOwner.User);

            _tokenParams.OldAdminPin = !string.IsNullOrWhiteSpace(_commandLineOptions.OldAdminPin) ?
                new PinCode(_commandLineOptions.OldAdminPin) :
                new PinCode(PinCodeOwner.Admin);

            _tokenParams.NewUserPin = !string.IsNullOrWhiteSpace(_commandLineOptions.UserPin) ?
                new PinCode(_commandLineOptions.UserPin) :
                new PinCode(PinCodeOwner.User);

            _tokenParams.NewAdminPin = !string.IsNullOrWhiteSpace(_commandLineOptions.AdminPin) ?
                new PinCode(_commandLineOptions.AdminPin) :
                new PinCode(PinCodeOwner.Admin);

            // TODO: сделать helper для битовых масок
            _tokenParams.AdminCanChangeUserPin = (tokenExtendedInfo.Flags & (ulong)RutokenFlag.AdminChangeUserPin) == (ulong)RutokenFlag.AdminChangeUserPin;
            _tokenParams.UserCanChangeUserPin = (tokenExtendedInfo.Flags & (ulong)RutokenFlag.UserChangeUserPin) == (ulong)RutokenFlag.UserChangeUserPin;
            
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
                TokenFormatter.Format(_slot,
                                    _tokenParams.OldAdminPin.Value, _tokenParams.NewAdminPin.Value,
                                    _tokenParams.NewUserPin.Value,
                                    _tokenParams.TokenLabel,
                                    _commandLineOptions.PinChangePolicy,
                                    _commandLineOptions.MinAdminPinLength, _commandLineOptions.MinUserPinLength,
                                    _commandLineOptions.MaxAdminPinAttempts, _commandLineOptions.MaxUserPinAttempts, 0);

                _logger.LogInformation(string.Format(Resources.FormatTokenSuccess, _tokenParams.TokenSerial));
            });
            
            return this;
        }

        public CommandHandlerBuilder WithPinsFromStore()
        {
            _commands.Enqueue(() =>
            {
                _tokenParams.NewAdminPin = new PinCode(_pinsStore.GetNextPin());
                _tokenParams.NewUserPin = new PinCode(_pinsStore.GetNextPin());
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

                _tokenParams.NewAdminPin = new PinCode(GeneratePin(_commandLineOptions.AdminPinLength.Value));
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

                _tokenParams.NewUserPin = new PinCode(GeneratePin(_commandLineOptions.UserPinLength.Value));
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

                _logger.LogInformation(string.Format(Resources.TokenLabelChangeSuccess, _tokenParams.TokenSerial));
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

                _logger.LogInformation(string.Format(Resources.TokenLabelChangeSuccess, _tokenParams.TokenSerial));
            });

            return this;
        }

        public CommandHandlerBuilder WithPinsChange()
        {
            // TODO: добавить логирование
            _commands.Enqueue(() =>
            {
                if (_tokenParams.NewUserPin.EnteredByUser)
                {
                    if (_tokenParams.AdminCanChangeUserPin && !_tokenParams.UserCanChangeUserPin)
                    {
                        if (_tokenParams.OldAdminPin.EnteredByUser)
                        {
                            PinChanger.ChangeUserPinByAdmin(_slot,
                                _tokenParams.OldAdminPin.Value,
                                _tokenParams.NewUserPin.Value);

                            _logger.LogInformation(Resources.PinChangedSuccess);
                        }
                        else
                        {
                            // TODO: сделать общие ошибки, подменяя слова пользователя или администратора
                            throw new InvalidOperationException(
                                string.Format(Resources.UserPinChangeAdminPinError, _tokenParams.TokenSerial));
                        }
                    }
                    else if (!_tokenParams.AdminCanChangeUserPin && _tokenParams.UserCanChangeUserPin)
                    {
                        if (_tokenParams.OldUserPin.EnteredByUser)
                        {
                            PinChanger.Change(_slot, 
                                _tokenParams.OldUserPin.Value, _tokenParams.NewUserPin.Value, 
                                PinCodeOwner.User);
                            _logger.LogInformation(Resources.PinChangedSuccess);
                        }
                        else
                        {
                            // TODO: сделать общие ошибки, подменяя слова пользователя или администратора
                            throw new InvalidOperationException(
                                string.Format(Resources.UserPinChangeUserPinError, _tokenParams.TokenSerial));
                        }
                    }
                    else if (_tokenParams.AdminCanChangeUserPin && _tokenParams.UserCanChangeUserPin)
                    {
                        if (_tokenParams.OldAdminPin.EnteredByUser ||
                            _tokenParams.OldUserPin.EnteredByUser)
                        {
                            if (_tokenParams.OldAdminPin.EnteredByUser)
                            {
                                PinChanger.ChangeUserPinByAdmin(_slot,
                                    _tokenParams.OldAdminPin.Value,
                                    _tokenParams.NewUserPin.Value);
                            }
                            else
                            {
                                PinChanger.Change(_slot,
                                    _tokenParams.OldUserPin.Value, _tokenParams.NewUserPin.Value,
                                    PinCodeOwner.User);
                            }

                            _logger.LogInformation(Resources.PinChangedSuccess);
                        }
                        else
                        {
                            // TODO: сделать общие ошибки, подменяя слова пользователя или администратора
                            throw new InvalidOperationException(
                                string.Format(Resources.UserPinChangeError, _tokenParams.TokenSerial));
                        }
                    }
                }

                if (_tokenParams.NewAdminPin.EnteredByUser)
                {
                    if (_tokenParams.OldAdminPin.EnteredByUser)
                    {
                        PinChanger.Change(_slot,
                            _tokenParams.OldAdminPin.Value, _tokenParams.NewAdminPin.Value,
                            PinCodeOwner.User);

                        _logger.LogInformation(Resources.PinChangedSuccess);
                    }
                    else
                    {
                        // TODO: сделать общие ошибки, подменяя слова пользователя или администратора
                        throw new InvalidOperationException(
                            string.Format(Resources.AdminPinChangeError, _tokenParams.TokenSerial));
                    }
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

                var smMode = ulong.Parse(commandParams[0]);

                if (smMode < 1 || smMode > 3)
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

                _logger.LogInformation(string.Format(Resources.GeneratingActivationPasswords, _tokenParams.TokenSerial));

                foreach (var password in ActivationPasswordGenerator.Generate(_slot, _tokenParams.OldAdminPin.Value, characterSet, smMode))
                {
                    _logger.LogInformation(Encoding.UTF8.GetString(password));
                }

                _logger.LogInformation(string.Format(Resources.ActivationPasswordsWereGenerated, _tokenParams.TokenSerial));
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

            if (!_commandLineOptions.Format && _tokenParams.NewUserPin.EnteredByUser && 
                (!_tokenParams.OldUserPin.EnteredByUser || !_tokenParams.OldAdminPin.EnteredByUser)) // TODO: здесь в исходной программе было &&
            {
                throw new InvalidOperationException(Resources.ChangeUserPinOldPinsError);
            }

            if (!_commandLineOptions.Format && _tokenParams.NewAdminPin.EnteredByUser && 
                !_tokenParams.OldAdminPin.EnteredByUser)
            {
                throw new InvalidOperationException(Resources.ChangeAdminPinOldPinError);
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
