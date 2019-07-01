using Net.Pkcs11Interop.Common;
using RutokenPkcs11Interop.Common;
using RutokenPkcs11Interop.HighLevelAPI;
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

        private static readonly Dictionary<string, CKU> _ownersMap =
            new Dictionary<string, CKU>
            {
                {"u", CKU.CKU_USER},
                {"a", CKU.CKU_SO}
            };

        public static IEnumerable<VolumeFormatInfoExtended> Create(IEnumerable<string> formatParams)
        {
            var formatParamsList = formatParams.ToList();

            if (formatParamsList.Count % _volumeParamsCount != 0)
            {
                throw new ArgumentException("Неверное число параметров");
            }

            for (var i = 0; i < formatParamsList.Count; i += _volumeParamsCount)
            {
                // TODO: error handling
                var volumeParams = formatParamsList.Skip(i).Take(_volumeParamsCount).ToList();
                ulong.TryParse(volumeParams[1], out var volumeSize);
                _ownersMap.TryGetValue(volumeParams[2], out var owner);
                _accessModesMap.TryGetValue(volumeParams[3], out var accessMode);

                yield return new VolumeFormatInfoExtended(volumeSize, accessMode, owner, 0);
            }
        }
    }
}
