using System;
using GMTK2020.ActionTimeline;
using GMTK2020.Environment;
using UnityEngine;

namespace GMTK2020
{
    public class EnemyController : MonoBehaviour
    {
        private RoomDefinition _room;
        
        private void Start()
        {
            RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up * 100F, Vector3.down,
                float.PositiveInfinity, LayerMask.GetMask("Room Definition"));
            foreach (RaycastHit hit in hits)
            {
                RoomDefinition room = hit.transform.GetComponent<RoomDefinition>();
                if (!room || room.isOutsideRoom)
                    continue;

                _room = room;
                _room.RegisterEnemy(this);
                break;
            }
            
            if(!_room)
                throw new Exception("Yo, I'm lost");
            
            Debug.Log(_room.name);
        }

        public void PlayerDetected(TimelineActor player)
        {
            Vector3 ppos = player.transform.position;
            ppos.y = transform.position.y;
            transform.LookAt(ppos);
        }
    }
}
