using System.Collections.Generic;
using System.Linq;
using Net.RutokenPkcs11Interop.Common;

namespace Aktiv.RtAdmin
{
    public class VolumeAttributesStore
    {
        private readonly Dictionary<string, FlashAccessMode> _accessModesMap =
            new Dictionary<string, FlashAccessMode>
            {
                {"ro", FlashAccessMode.Readonly},
                {"rw", FlashAccessMode.Readwrite},
                {"hi", FlashAccessMode.Hidden},
                {"cd", FlashAccessMode.Cdrom}
            };

        private readonly Dictionary<string, bool> _permanentStateMap =
            new Dictionary<string, bool>
            {
                {"p", true},
                {"t", false},
            };

        public bool TryGetAccessMode(string accessModeId, out FlashAccessMode accessMode) =>
            _accessModesMap.TryGetValue(accessModeId, out accessMode);

        public bool TryGetPermanentState(string permanentStateId, out bool permanentState) =>
            _permanentStateMap.TryGetValue(permanentStateId, out permanentState);

        public string GetAccessModeDescription(FlashAccessMode accessMode)
        {
            return _accessModesMap.ContainsValue(accessMode) ?
                _accessModesMap.Single(x => x.Value == accessMode).Key : "--";
        }

        public string GetPermanentStateDescription(bool state)
        {
            return _permanentStateMap.ContainsValue(state) ?
                _permanentStateMap.Single(x => x.Value == state).Key : "--";
        }
    }
}
