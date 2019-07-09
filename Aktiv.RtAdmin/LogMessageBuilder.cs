using Aktiv.RtAdmin.Properties;
using Net.Pkcs11Interop.Common;
using RutokenPkcs11Interop.HighLevelAPI;
using System;
using Aktiv.RtAdmin.Models;

namespace Aktiv.RtAdmin
{
    public class LogMessageBuilder
    {
        private readonly RuntimeTokenParams _runtimeTokenParams;
        private readonly VolumeOwnersStore _volumeOwnersStore;

        public LogMessageBuilder(RuntimeTokenParams runtimeTokenParams,
            VolumeOwnersStore volumeOwnersStore)
        {
            _runtimeTokenParams = runtimeTokenParams ?? 
                           throw new ArgumentNullException(nameof(runtimeTokenParams), Resources.TokenParamsNotSet);
            _volumeOwnersStore = volumeOwnersStore ??
                           throw new ArgumentNullException(nameof(volumeOwnersStore), Resources.TokenParamsNotSet);
        }

        public string WithTokenId(string message) => 
            $"0x{_runtimeTokenParams.TokenSerial} / {_runtimeTokenParams.TokenSerialDecimal} : {message}";

        public string WithTokenIdSuffix(string message) =>
            $"{message}. {Resources.TokenId}: 0x{_runtimeTokenParams.TokenSerial}";

        public string WithPKCS11Error(CKR code) => $"{Resources.PKCS11Error} 0x{code:X}";

        public string WithUnhandledError(string message) => $"{Resources.UnhandledError} {message}";

        public string WithFormatResult(string message) =>
            string.Format(WithTokenId(message),
                _runtimeTokenParams.NewAdminPin, _runtimeTokenParams.NewUserPin, _runtimeTokenParams.SmMode);

        public string WithPolicyDescription(UserPinChangePolicy policy)
        {
            string policyDescription;

            switch (policy)
            {
                case UserPinChangePolicy.ByUser:
                    policyDescription = Resources.UserPin;
                    break;

                case UserPinChangePolicy.ByAdmin:
                    policyDescription = Resources.AdminPin;
                    break;

                case UserPinChangePolicy.ByUserOrAdmin:
                    policyDescription = $"{Resources.AdminPin} {Resources.Or} {Resources.UserPin}";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(policy), policy, null);
            }

            return string.Format(Resources.UserPinChangePolicyError, policyDescription);
        }

        public string WithVolumeInfo(VolumeInfoExtended volumeInfo, 
            bool withPassedMark = true)
        {
            var passedMark = withPassedMark ? 
                $"{Resources.DriveFormatVolumeCreateSuccess} :" : string.Empty;

            return $"{WithTokenId(passedMark)} " +
                   $"{volumeInfo.VolumeId} " +
                   $"{volumeInfo.VolumeSize} " +
                   $"{_volumeOwnersStore.GetVolumeOwnerById((uint)volumeInfo.VolumeOwner)} " +
                   $"{ChangeVolumeAttributesParamsFactory.GetAccessModeDescription(volumeInfo.AccessMode)}";
        }

        public string WithVolumeInfo(VolumeInfo volumeInfo,
            bool withPassedMark = true)
        {
            var passedMark = withPassedMark ?
                $"{Resources.DriveFormatVolumeCreateSuccess} :" : string.Empty;

            return $"{WithTokenId(passedMark)} " +
                   $"{volumeInfo.Id} " +
                   $"{volumeInfo.Size} " +
                   $"{_volumeOwnersStore.GetVolumeOwnerById(volumeInfo.Owner)} " +
                   $"{ChangeVolumeAttributesParamsFactory.GetAccessModeDescription(volumeInfo.AccessMode)}";
        }

        public string WithVolumeInfo(ChangeVolumeAttributesParams volumeParams)
        {
            return $"{WithTokenId(Resources.VolumeAccessModeChangeSuccess)} : " +
                   $"{volumeParams.VolumeId} " +
                   $"{ChangeVolumeAttributesParamsFactory.GetAccessModeDescription(volumeParams.AccessMode)} " +
                   $"{ChangeVolumeAttributesParamsFactory.GetPermanentStateDescription(volumeParams.Permanent)}";
        }
    }
}
