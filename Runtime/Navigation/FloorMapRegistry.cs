using System;
using System.Collections.Generic;
using UnityEngine;

namespace IndoorNavigation.Navigation
{
    public sealed class FloorMapRegistry : MonoBehaviour
    {
        [Serializable]
        public sealed class FloorMapBinding
        {
            public string FloorId = "F1";
            public int ImmersalMapId;
            public Transform NavigationRoot;
            public GameObject FloorContentRoot;
            public bool IsDefault;
        }

        [SerializeField]
        private List<FloorMapBinding> maps = new List<FloorMapBinding>();

        public string ActiveFloorId { get; private set; } = string.Empty;
        public FloorMapBinding ActiveBinding { get; private set; }

        private void Awake()
        {
            if (maps.Count == 0)
            {
                return;
            }

            FloorMapBinding defaultMap = maps.Find(x => x.IsDefault) ?? maps[0];
            ActivateFloor(defaultMap.FloorId);
        }

        public bool ActivateFloor(string floorId)
        {
            FloorMapBinding target = maps.Find(x => string.Equals(x.FloorId, floorId, StringComparison.OrdinalIgnoreCase));
            if (target == null)
            {
                return false;
            }

            for (int i = 0; i < maps.Count; i++)
            {
                bool isActive = maps[i] == target;
                if (maps[i].FloorContentRoot != null)
                {
                    maps[i].FloorContentRoot.SetActive(isActive);
                }
            }

            ActiveFloorId = target.FloorId;
            ActiveBinding = target;
            return true;
        }

        public bool ActivateByMapId(int immersalMapId)
        {
            FloorMapBinding target = maps.Find(x => x.ImmersalMapId == immersalMapId);
            return target != null && ActivateFloor(target.FloorId);
        }

        public bool TryGetByMapId(int immersalMapId, out FloorMapBinding binding)
        {
            binding = maps.Find(x => x.ImmersalMapId == immersalMapId);
            return binding != null;
        }

        public bool TryGetByFloorId(string floorId, out FloorMapBinding binding)
        {
            binding = maps.Find(x => string.Equals(x.FloorId, floorId, StringComparison.OrdinalIgnoreCase));
            return binding != null;
        }
    }
}
