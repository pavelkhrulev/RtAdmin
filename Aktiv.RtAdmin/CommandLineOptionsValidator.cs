using Aktiv.RtAdmin.Properties;
using RutokenPkcs11Interop.Common;
using System;
using System.Linq;

namespace Aktiv.RtAdmin
{
    public class CommandLineOptionsValidator
    {
        private readonly CommandLineOptions _commandLineOptions;
        private readonly RuntimeTokenParams _runtimeTokenParams;
        private readonly VolumeOwnersStore _volumeOwnersStore;

        public CommandLineOptionsValidator(CommandLineOptions commandLineOptions, 
            RuntimeTokenParams runtimeTokenParams, VolumeOwnersStore volumeOwnersStore)
        {
            _commandLineOptions = commandLineOptions;
            _runtimeTokenParams = runtimeTokenParams;
            _volumeOwnersStore = volumeOwnersStore;
        }

        public void ValidatePinsLengthBeforeFormat()
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

        public void ValidatePinsLengthBeforePinsChange()
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

        public void ValidatePinsLengthBeforeAdminPinGeneration()
        {
            if ((_commandLineOptions.AdminPinLength.HasValue &&
                (_commandLineOptions.AdminPinLength < _runtimeTokenParams.MinAdminPinLenFromToken) ||
                 _commandLineOptions.AdminPinLength > _runtimeTokenParams.MaxAdminPinLenFromToken))
            {
                throw new ArgumentException(string.Format(Resources.RandomAdminPinLengthMismatch,
                    _runtimeTokenParams.MinAdminPinLenFromToken, _runtimeTokenParams.MaxAdminPinLenFromToken));
            }
        }

        public void ValidatePinsLengthBeforeUserPinGeneration()
        {
            if ((_commandLineOptions.UserPinLength.HasValue &&
                 (_commandLineOptions.UserPinLength < _runtimeTokenParams.MinUserPinLenFromToken) ||
                 _commandLineOptions.UserPinLength > _runtimeTokenParams.MaxUserPinLenFromToken))
            {
                throw new ArgumentException(string.Format(Resources.RandomUserPinLengthMismatch,
                    _runtimeTokenParams.MinUserPinLenFromToken, _runtimeTokenParams.MaxUserPinLenFromToken));
            }
        }

        public void ValidateTokenNameChange()
        {
            if (_runtimeTokenParams.OldUserPin == null ||
               !_runtimeTokenParams.OldUserPin.EnteredByUser)
            {
                throw new ArgumentException(Resources.TokenLabelPinError);
            }
        }

        public void ValidateFormatTokenParams()
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

        public void ValidateNewLocalPinParams()
        {
            var commandParams = _commandLineOptions.SetLocalPin.ToList();
            if (commandParams.Count != DefaultValues.NewLocalPinCommandParamsCount)
            {
                throw new ArgumentException(Resources.NewLocalPinInvalidCommandParamsCount);
            }

            if (!_volumeOwnersStore.TryGetOwnerId(commandParams[0], out var localIdToCreate))
            {
                throw new ArgumentException(Resources.NewLocalPinInvalidOwnerId);
            }

            _runtimeTokenParams.NewLocalPin = commandParams[1];
            _runtimeTokenParams.LocalIdToCreate = localIdToCreate;
        }

        public void ValidateGenerateActivationPasswordParams()
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
    }
}
