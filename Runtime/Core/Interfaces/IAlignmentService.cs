using IndoorNavigation.Core.Models;
using UnityEngine;

namespace IndoorNavigation.Core.Interfaces
{
    public interface IAlignmentService
    {
        Transform NavigationRoot { get; }
        bool IsSmoothing { get; }

        void Initialize(Transform navigationRoot, Transform arCameraTransform);
        void ApplyInstant(LocalizationPose localizationPose);
        void ApplySmooth(LocalizationPose localizationPose, float smoothDurationSeconds);
        void Tick(float deltaTime);
    }
}
