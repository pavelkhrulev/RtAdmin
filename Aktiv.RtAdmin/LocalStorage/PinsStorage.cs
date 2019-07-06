using Aktiv.RtAdmin.Properties;
using System;

namespace Aktiv.RtAdmin
{
    public class PinsStorage : BaseLocalStorage
    {
        public PinsStorage() :
            base(Resources.PinCodesFileNotFound, Resources.PinCodesFileIsEmpty, Resources.PinCodesFilePinsHaveEnded)
        { }

        protected override void Validate()
        {
            if (Entities.Count % 2 != 0)
            {
                throw new InvalidOperationException(Resources.PinCodesFileIncorrectLinesCount);
            }
        }
    }
}
