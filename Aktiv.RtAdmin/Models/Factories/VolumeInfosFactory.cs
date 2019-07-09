using Aktiv.RtAdmin.Models;
using Net.Pkcs11Interop.Common;
using RutokenPkcs11Interop.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aktiv.RtAdmin
{
    public static class VolumeInfosFactory
    {
        private const int _volumeParamsCount = 4;

        private static readonly Dictionary<string, FlashAccessMode> _accessModesMap =
            new Dictionary<string, FlashAccessMode>
            {
                {"ro", FlashAccessMode.Readonly},
                {"rw", FlashAccessMode.Readwrite},
                {"hi", FlashAccessMode.Hidden},
                {"cd", FlashAccessMode.Cdrom}
            };

        private static readonly Dictionary<string, uint> _ownersMap =
            new Dictionary<string, uint>
            {
                {"a", (uint)CKU.CKU_SO},
                {"u", (uint)CKU.CKU_USER},
                {"l3", 0x3},
                {"l4", 0x4},
                {"l5", 0x5},
                {"l6", 0x6},
                {"l7", 0x7},
                {"l8", 0x8},
                {"l9", 0x9}
            };

        public static IEnumerable<VolumeInfo> Create(IEnumerable<string> formatParams)
        {
            var formatParamsList = formatParams.ToList();

            if (formatParamsList.Count % _volumeParamsCount != 0)
            {
                // TODO: брать из ресурсов
                throw new ArgumentException("Неверное число параметров");
            }

            for (var i = 0; i < formatParamsList.Count; i += _volumeParamsCount)
            {
                // TODO: error handling
                var volumeParams = formatParamsList.Skip(i).Take(_volumeParamsCount).ToList();
                uint.TryParse(volumeParams[0], out var volumeId);
                ulong.TryParse(volumeParams[1], out var volumeSize);
                _ownersMap.TryGetValue(volumeParams[2], out var owner);
                _accessModesMap.TryGetValue(volumeParams[3], out var accessMode);

                yield return new VolumeInfo
                {
                    Id = volumeId,
                    Size = volumeSize,
                    AccessMode = accessMode,
                    Owner = owner
                };
            }
        }
    }
}
