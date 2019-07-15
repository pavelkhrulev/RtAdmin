using Aktiv.RtAdmin.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aktiv.RtAdmin
{
    public static class FormatVolumeInfosFactory
    {
        private const int _volumeParamsCount = 4;

        public static IEnumerable<VolumeInfo> Create(
            VolumeOwnersStore volumeOwnersStore,
            VolumeAttributesStore volumeAttributesStore,
            IEnumerable<string> formatParams)
        {
            var formatParamsList = formatParams.ToList();

            if (formatParamsList.Count % _volumeParamsCount != 0)
            {
                throw new ArgumentException(Resources.FormatDriveInvalidCommandParamsCount);
            }

            for (var i = 0; i < formatParamsList.Count; i += _volumeParamsCount)
            {
                var volumeParams = formatParamsList.Skip(i).Take(_volumeParamsCount).ToList();

                if (!(uint.TryParse(volumeParams[0], out var volumeId)))
                {
                    throw new ArgumentException(Resources.VolumeInfoInvalidVolumeId);
                }

                if (!(uint.TryParse(volumeParams[1], out var volumeSize)))
                {
                    throw new ArgumentException(Resources.FormatDriveInvalidVolumeSize);
                }

                if (!volumeOwnersStore.TryGetOwnerId(volumeParams[2], out var owner))
                {
                    throw new ArgumentException(Resources.NewLocalPinInvalidOwnerId);
                }

                if (!volumeAttributesStore.TryGetAccessMode(volumeParams[3], out var accessMode))
                {
                    throw new ArgumentException(Resources.FormatDriveInvalidAccessMode);
                }

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
