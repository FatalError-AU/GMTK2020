using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace GMTK2020.Environment
{
    public class RoomDoor : MonoBehaviour
    {
        public bool requireBreach;

        private NavMeshObstacle _obstacle;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(transform.position, .25F);
        }
    }
}
