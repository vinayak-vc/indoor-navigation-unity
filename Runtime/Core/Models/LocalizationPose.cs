using System;

using UnityEngine;

namespace IndoorNavigation.Core.Models {
    [Serializable]
    public struct LocalizationPose {
        public Vector3 Position;
        public Quaternion Rotation;
        public float Confidence;
        public int MapId;
        public DateTime TimestampUtc;

        public LocalizationPose(Vector3 position, Quaternion rotation, float confidence, int mapId) {
            Position = position;
            Rotation = rotation;
            Confidence = confidence;
            MapId = mapId;
            TimestampUtc = DateTime.UtcNow;
        }
    }
}