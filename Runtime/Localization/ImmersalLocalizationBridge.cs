using System;
using IndoorNavigation.Core.Models;
using UnityEngine;

namespace IndoorNavigation.Localization
{
    public sealed class ImmersalLocalizationBridge : MonoBehaviour
    {
        public event Action<int> LocalizationRequested;
        public event Action<LocalizationPose> PoseReceived;
        public event Action<string> LocalizationFailed;

        public bool IsRequestInFlight { get; private set; }

        public void RequestLocalization(int mapId)
        {
            IsRequestInFlight = true;
            LocalizationRequested?.Invoke(mapId);
        }

        public void ReportLocalizationSuccess(Vector3 localizedPosition, Quaternion localizedRotation, float confidence, int mapId)
        {
            IsRequestInFlight = false;
            PoseReceived?.Invoke(new LocalizationPose(localizedPosition, localizedRotation, confidence, mapId));
        }

        public void ReportLocalizationFailure(string reason)
        {
            IsRequestInFlight = false;
            LocalizationFailed?.Invoke(string.IsNullOrWhiteSpace(reason) ? "Immersal localization failed." : reason);
        }
    }
}
