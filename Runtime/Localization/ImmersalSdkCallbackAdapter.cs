using UnityEngine;

namespace IndoorNavigation.Localization
{
    public sealed class ImmersalSdkCallbackAdapter : MonoBehaviour
    {
        [SerializeField]
        private ImmersalLocalizationBridge bridge;

        public void OnLocalizationSuccess(Transform localizedCameraInMapSpace)
        {
            if (bridge == null || localizedCameraInMapSpace == null)
            {
                return;
            }

            bridge.ReportLocalizationSuccess(localizedCameraInMapSpace.position, localizedCameraInMapSpace.rotation, 1f, 0);
        }

        public void OnLocalizationSuccessWithPose(Vector3 position, Quaternion rotation, float confidence, int mapId)
        {
            if (bridge == null)
            {
                return;
            }

            bridge.ReportLocalizationSuccess(position, rotation, confidence, mapId);
        }

        public void OnLocalizationFailed(string message)
        {
            bridge?.ReportLocalizationFailure(message);
        }
    }
}
