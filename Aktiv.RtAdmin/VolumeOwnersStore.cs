using System;
using System.Collections.Generic;
using System.Linq;
using Net.Pkcs11Interop.Common;

namespace Aktiv.RtAdmin
{
    public class VolumeOwnersStore
    {
        private readonly Dictionary<string, uint> _ownersInfo = new Dictionary<string, uint>
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

        private const int _pin2Id = 0x1F;

        public bool TryGetOwnerId(string volumeOwner, out uint volumeOwnerId) => 
            _ownersInfo.TryGetValue(volumeOwner, out volumeOwnerId);

        public string GetVolumeOwnerById(uint id)
        {
            return _ownersInfo.ContainsValue(id) ? 
                _ownersInfo.Single(x => x.Value == id).Key : "--";
        }

        public uint GetPin2Id() => _pin2Id;
    }
}
