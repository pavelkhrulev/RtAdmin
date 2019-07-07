using System;
using System.Collections.Generic;
using System.Linq;
using Net.Pkcs11Interop.Common;
using RutokenPkcs11Interop.Common;
using RutokenPkcs11Interop.HighLevelAPI;

namespace Aktiv.RtAdmin
{
    public static class ChangeVolumeAttributesParamsFactory
    {
        private const int _changeParamsCount = 3;

        // TODO: вынести этот словарь в одно место из фабрик
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
                    throw new InvalidOperationException($"Раздел с id {volumeId} не найден");
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
                    case CKU.CKU_CONTEXT_SPECIFIC:
                        runtimeTokenParams.LocalUserPins.TryGetValue((uint)volumeInfo.VolumeOwner, out ownerPin);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
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
    }
}