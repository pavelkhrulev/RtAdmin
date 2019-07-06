using System;
using Aktiv.RtAdmin.Properties;

namespace Aktiv.RtAdmin
{
    public enum UserPinChangePolicy
    {
        ByUser = 1,
        ByAdmin = 2,
        ByUserOrAdmin = 3
    }

    public static class UserPinChangePolicyFactory
    {
        public static UserPinChangePolicy Create(bool userCanChange, bool adminCanChange)
        {
            if (userCanChange && adminCanChange)
            {
                return UserPinChangePolicy.ByUserOrAdmin;
            }

            if (userCanChange)
            {
                return UserPinChangePolicy.ByUser;
            }

            if (adminCanChange)
            {
                return UserPinChangePolicy.ByAdmin;
            }

            throw new InvalidOperationException("Invalid UserPinChangePolicy");
        }
    }
}
