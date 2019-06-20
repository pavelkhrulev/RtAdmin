using Aktiv.RtAdmin.Properties;
using Microsoft.Extensions.Logging;
using Net.Pkcs11Interop.HighLevelAPI;
using RutokenPkcs11Interop.HighLevelAPI;
using System;
using System.Collections.Concurrent;

namespace Aktiv.RtAdmin
{
    public class CommandHandlerBuilder
    {
        private readonly ILogger logger;
        private Slot slot;
        private CommandLineOptions commandLineOptions;
        private readonly TokenParams tokenParams;

        private readonly ConcurrentQueue<Action> commands;

        public CommandHandlerBuilder(ILogger<RtAdmin> logger, TokenParams tokenParams)
        {
            this.logger = logger;
            this.tokenParams = tokenParams;

            commands = new ConcurrentQueue<Action>();
        }

        public CommandHandlerBuilder ConfigureWith(Slot slot, CommandLineOptions options)
        {
            this.slot = slot;
            this.commandLineOptions = options;

            if (!string.IsNullOrWhiteSpace(commandLineOptions.TokenLabelCp1251))
            {
                tokenParams.TokenLabel = commandLineOptions.TokenLabelCp1251;
            }

            if (!string.IsNullOrWhiteSpace(commandLineOptions.TokenLabelUtf8))
            {
                tokenParams.TokenLabel = commandLineOptions.TokenLabelUtf8;
            }

            var tokenInfo = slot.GetTokenInfo();
            tokenParams.TokenSerial = tokenInfo.SerialNumber;

            var tokenExtendedInfo = slot.GetTokenInfoExtended();
            tokenParams.MinAdminPinLenFromToken = tokenExtendedInfo.MinAdminPinLen;
            tokenParams.MaxAdminPinLenFromToken = tokenExtendedInfo.MaxAdminPinLen;
            tokenParams.MinUserPinLenFromToken = tokenExtendedInfo.MinUserPinLen;
            tokenParams.MaxUserPinLenFromToken = tokenExtendedInfo.MaxUserPinLen;

            return this;
        }

        public CommandHandlerBuilder WithFormat()
        {
            commands.Enqueue(() =>
            {
                TokenFormatter.Format(slot,
                                    commandLineOptions.AdminPin, commandLineOptions.AdminPin, commandLineOptions.UserPin,
                                    tokenParams.TokenLabel,
                                    commandLineOptions.PinChangePolicy,
                                    commandLineOptions.MinAdminPinLength, commandLineOptions.MinUserPinLength,
                                    commandLineOptions.MaxAdminPinAttempts, commandLineOptions.MaxUserPinAttempts, 0);

                logger.LogInformation(string.Format(Resources.FormatTokenSuccess, tokenParams.TokenSerial));
            });
            
            return this;
        }

        public CommandHandlerBuilder WithNewAdminPin()
        {
            commands.Enqueue(() =>
            {
                tokenParams.NewAdminPin = GeneratePin(commandLineOptions.AdminPinLength);
            });

            return this;
        }

        public CommandHandlerBuilder WithNewUserPin()
        {
            commands.Enqueue(() =>
            {
                tokenParams.NewUserPin = GeneratePin(commandLineOptions.UserPinLength);
            });

            return this;
        }

        public CommandHandlerBuilder WithNewUtf8TokenName()
        {
            commands.Enqueue(() =>
            {
                TokenName.SetNew(slot, commandLineOptions.UserPin, 
                    Helpers.StringToUtf8String(tokenParams.TokenLabel));

                logger.LogInformation(string.Format(Resources.TokenLabelChangeSuccess, tokenParams.TokenSerial));
            });

            return this;
        }

        public CommandHandlerBuilder WithNewCp1251TokenName()
        {
            commands.Enqueue(() =>
            {
                TokenName.SetNew(slot, commandLineOptions.UserPin,
                    Helpers.StringToCp1251String(tokenParams.TokenLabel));

                logger.LogInformation(string.Format(Resources.TokenLabelChangeSuccess, tokenParams.TokenSerial));
            });

            return this;
        }

        public CommandHandlerBuilder WithGenerationActivationPassword()
        {
            
            return this;
        }

        public void Execute()
        {
            foreach (var command in commands)
            {
                command?.Invoke();
            }
        }

        private string GeneratePin(uint pinLength)
        {
            var tokenInfo = slot.GetTokenInfoExtended();
            return PinGenerator.Generate(slot, tokenInfo.TokenType, pinLength, commandLineOptions.UTF8InsteadOfcp1251);
        }
    }
}
