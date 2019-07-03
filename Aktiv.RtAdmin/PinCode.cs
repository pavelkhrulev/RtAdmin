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
            Owner = owner;

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

        public PinCode(PinCodeOwner owner, string pinCode)
        {
            Owner = owner;
            Value = pinCode;
            EnteredByUser = true;
        }

        public PinCodeOwner Owner { get; }

        public string Value { get; }

        public ulong Length => Value != null ? (ulong) Value?.Length : default;

        public bool EnteredByUser { get; }

        public override string ToString() => Value;
    }
}
