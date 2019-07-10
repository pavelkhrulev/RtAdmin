using Net.Pkcs11Interop.Common;
using RutokenPkcs11Interop.Common;
using RutokenPkcs11Interop.HighLevelAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using Aktiv.RtAdmin.Properties;

namespace Aktiv.RtAdmin
{
    public static class ChangeVolumeAttributesParamsFactory
    {
        private const int _changeParamsCount = 3;

        // TODO: вынести это в store
        private static readonly Dictionary<string, FlashAccessMode> _accessModesMap =
            new Dictionary<string, FlashAccessMode>
            {
                {"ro", FlashAccessMode.Readonly},
                {"rw", FlashAccessMode.Readwrite},
                {"hi", FlashAccessMode.Hidden},
                {"cd", FlashAccessMode.Cdrom}
            };

        private static readonly Dictionary<string, bool> _permanentStateMap =
            new Dictionary<string, bool>
            {
                {"p", true},
                {"t", false},
            };

        public static IEnumerable<ChangeVolumeAttributesParams> Create(
            IEnumerable<string> changeParams,
            IEnumerable<VolumeInfoExtended> volumeInfos,
            RuntimeTokenParams runtimeTokenParams)
        {
            var changeParamsList = changeParams.ToList();
            var volumeInfosList = volumeInfos.ToList();

            if (changeParamsList.Count % _changeParamsCount != 0)
            {
                throw new ArgumentException("Неверное число параметров");
            }

            for (var i = 0; i < changeParamsList.Count; i += _changeParamsCount)
            {
                // TODO: error handling
                var volumeParams = changeParamsList.Skip(i).Take(_changeParamsCount).ToList();
                ulong.TryParse(volumeParams[0], out var volumeId);
                _accessModesMap.TryGetValue(volumeParams[1], out var accessMode);
                _permanentStateMap.TryGetValue(volumeParams[2], out var permanent);
                var volumeInfo = volumeInfosList.SingleOrDefault(x => x.VolumeId == volumeId);
                if (volumeInfo == null)
                {
                    throw new ArgumentException(Resources.VolumeInfoInvalidVolumeId);
                }

                string ownerPin;
                switch (volumeInfo.VolumeOwner)
                {
                    case CKU.CKU_SO:
                        ownerPin = runtimeTokenParams.NewAdminPin.EnteredByUser
                            ? runtimeTokenParams.NewAdminPin.Value
                            : runtimeTokenParams.OldAdminPin.Value;
                        break;
                    case CKU.CKU_USER:
                        ownerPin = runtimeTokenParams.NewUserPin.EnteredByUser
                            ? runtimeTokenParams.NewUserPin.Value
                            : runtimeTokenParams.OldUserPin.Value;
                        break;
                    default:
                        if (runtimeTokenParams.LocalUserPins == null)
                        {
                            throw new InvalidOperationException("Не установлен PIN-код локального пользователя");
                        }
                        if (!(runtimeTokenParams.LocalUserPins.TryGetValue((uint) volumeInfo.VolumeOwner, out ownerPin)))
                        {
                            throw new InvalidOperationException("Неверный владелец раздела");
                        }
                        
                        break;
                }

                if (string.IsNullOrEmpty(ownerPin))
                {
                    throw new InvalidOperationException("PIN-код не установлен");
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

        // TODO: вынести это в store
        public static string GetAccessModeDescription(FlashAccessMode accessMode)
        {
            return _accessModesMap.ContainsValue(accessMode) ?
                _accessModesMap.Single(x => x.Value == accessMode).Key : "--";
        }

        // TODO: вынести это в store
        public static string GetPermanentStateDescription(bool state)
        {
            return _permanentStateMap.ContainsValue(state) ?
                _permanentStateMap.Single(x => x.Value == state).Key : "--";
        }
    }
}