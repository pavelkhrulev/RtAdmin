using System;

namespace Aktiv.RtAdmin
{
    public enum PinCodeOwner
    {
        Admin,
        User
    }

    public class PinCode
    {
        public PinCode(PinCodeOwner owner)
        {
            switch (owner)
            {
                case PinCodeOwner.Admin:
                    Value = DefaultValues.AdminPin;
                    break;
                case PinCodeOwner.User:
                    Value = DefaultValues.UserPin;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(owner), owner, "Incorrect PIN-code owner");
            }
        }

        public PinCode(string pinCode)
        {
            Value = pinCode;
            EnteredByUser = true;
        }

        public string Value { get; }

        public ulong Length => Value != null ? (ulong) Value?.Length : default;

        public bool EnteredByUser { get; }
    }
}
