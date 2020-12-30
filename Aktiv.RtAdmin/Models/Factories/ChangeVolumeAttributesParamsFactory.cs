using Aktiv.RtAdmin.Properties;
using Net.Pkcs11Interop.Common;
using Net.RutokenPkcs11Interop.HighLevelAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aktiv.RtAdmin
{
    public static class ChangeVolumeAttributesParamsFactory
    {
        private const int _changeParamsCount = 3;

        public static IEnumerable<ChangeVolumeAttributesParams> Create(
            VolumeAttributesStore volumeAttributesStore,
            IEnumerable<string> changeParams,
            IEnumerable<IVolumeInfoExtended> volumeInfos,
            RuntimeTokenParams runtimeTokenParams)
        {
            var changeParamsList = changeParams.ToList();
            var volumeInfosList = volumeInfos.ToList();

            if (changeParamsList.Count % _changeParamsCount != 0)
            {
                throw new ArgumentException(Resources.ChangeVolumeAttributesInvalidCommandParamsCount);
            }

            for (var i = 0; i < changeParamsList.Count; i += _changeParamsCount)
            {
                var volumeParams = changeParamsList.Skip(i).Take(_changeParamsCount).ToList();

                if (!(uint.TryParse(volumeParams[0], out var volumeId)))
                {
                    throw new ArgumentException(Resources.VolumeInfoInvalidVolumeId);
                }

                if (!volumeAttributesStore.TryGetAccessMode(volumeParams[1], out var accessMode))
                {
                    throw new ArgumentException(Resources.FormatDriveInvalidAccessMode);
                }

                if (!volumeAttributesStore.TryGetPermanentState(volumeParams[2], out var permanent))
                {
                    throw new ArgumentException(Resources.ChangeVolumeAttributesInvalidPermanentState);
                }

                var volumeInfo = volumeInfosList.SingleOrDefault(x => x.VolumeId == volumeId);
                if (volumeInfo == null)
                {
                    throw new ArgumentException(Resources.VolumeInfoInvalidVolumeId);
                }

                string ownerPin;
                switch (volumeInfo.VolumeOwner)
                {
                    case CKU.CKU_SO:
                        if (!runtimeTokenParams.OldAdminPin.EnteredByUser)
                        {
                            Console.WriteLine(Resources.DefaultAdminPinWillBeUsed);
                        }

                        ownerPin = runtimeTokenParams.OldAdminPin.Value;
                        break;
                    case CKU.CKU_USER:
                        if (!runtimeTokenParams.OldUserPin.EnteredByUser)
                        {
                            Console.WriteLine(Resources.DefaultUserPinWillBeUsed);
                        }

                        ownerPin = runtimeTokenParams.OldUserPin.Value;
                        break;
                    default:
                        if (runtimeTokenParams.LocalUserPins == null)
                        {
                            throw new InvalidOperationException(Resources.ChangeVolumeAttributesNeedSetLocalPin);
                        }
                        if (!(runtimeTokenParams.LocalUserPins.TryGetValue((uint) volumeInfo.VolumeOwner, out ownerPin)))
                        {
                            throw new InvalidOperationException(Resources.NewLocalPinInvalidOwnerId);
                        }
                        
                        break;
                }

                yield return new ChangeVolumeAttributesParams
                {
                    VolumeId = volumeId,
                    AccessMode = accessMode,
                    VolumeOwner = volumeInfo.VolumeOwner,
                    OwnerPin = ownerPin,
                    Permanent = permanent
                };
            }
        }
    }
}