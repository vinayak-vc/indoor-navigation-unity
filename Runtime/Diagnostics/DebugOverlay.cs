using IndoorNavigation.Controllers;
using IndoorNavigation.Core.Enums;
using UnityEngine;

namespace IndoorNavigation.Diagnostics
{
    public sealed class DebugOverlay : MonoBehaviour
    {
        [SerializeField]
        private AppController appController;

        [SerializeField]
        private NavigationController navigationController;

        [SerializeField]
        private bool showOverlay = true;

        private GUIStyle labelStyle;

        private void Awake()
        {
            labelStyle = new GUIStyle
            {
                fontSize = 24,
                normal = { textColor = Color.white }
            };
        }

        private void OnGUI()
        {
            if (!showOverlay || appController == null)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(20, 20, 760, 280), GUI.skin.box);
            GUILayout.Label($"Localization: {appController.Status}", labelStyle);
            GUILayout.Label($"Map ID: {appController.LastLocalizationPose.MapId}", labelStyle);
            GUILayout.Label($"Confidence: {appController.LastLocalizationPose.Confidence:0.00}", labelStyle);
            GUILayout.Label($"Navigation Active: {(navigationController != null && navigationController.IsNavigating)}", labelStyle);

            if (appController.Status == LocalizationStatus.Failed)
            {
                GUILayout.Label("Last localization attempt failed. Check map ID / VPS coverage.", labelStyle);
            }

            GUILayout.EndArea();
        }
    }
}
