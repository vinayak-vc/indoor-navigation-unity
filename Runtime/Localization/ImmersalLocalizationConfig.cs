using UnityEngine;

namespace IndoorNavigation.Localization
{
    [CreateAssetMenu(fileName = "ImmersalLocalizationConfig", menuName = "Indoor Navigation/Immersal Localization Config")]
    public sealed class ImmersalLocalizationConfig : ScriptableObject
    {
        [Min(0.1f)]
        public float LocalizationRetryIntervalSeconds = 2f;

        [Min(0.1f)]
        public float RelocalizationCooldownSeconds = 4f;

        [Range(0f, 1f)]
        public float MinimumAcceptedConfidence = 0.4f;
    }
}
