using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace GMTK2020.Environment
{
    public class RoomDoor : MonoBehaviour
    {
        public static readonly HashSet<RoomDoor> AllDoors = new HashSet<RoomDoor>();
        
        public bool DisableArea
        {
            set
            {
                if (_obstacle)
                    _obstacle.enabled = value;
            }
        }

        public bool requireBreach;

        private NavMeshObstacle _obstacle;

        private void Awake()
        {
            AllDoors.Add(this);
            
            NavMeshModifierVolume vol = GetComponent<NavMeshModifierVolume>();
            if (!vol)
            {
                Debug.LogError($"Door {name} is missing a navmesh modifier volume");
                return;
            }

            _obstacle = gameObject.AddComponent<NavMeshObstacle>();
            _obstacle.size = vol.size;
            _obstacle.center = vol.center;
            _obstacle.carving = true;
            _obstacle.carvingTimeToStationary = 0F;

            DisableArea = false;
        }

        private void OnDestroy()
        {
            AllDoors.Remove(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(transform.position, .25F);
        }
    }
}
