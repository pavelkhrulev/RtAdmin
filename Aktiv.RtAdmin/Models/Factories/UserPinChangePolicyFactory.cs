using Aktiv.RtAdmin.Properties;
using System;

namespace Aktiv.RtAdmin
{
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

            throw new ArgumentOutOfRangeException(Resources.InvalidUserPinChangePolicy);
        }
    }
}