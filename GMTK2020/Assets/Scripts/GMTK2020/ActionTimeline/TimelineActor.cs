using System;
using System.Collections;
using GMTK2020.Environment;
using UnityEngine;
using UnityEngine.AI;

namespace GMTK2020.ActionTimeline
{
    public class TimelineActor : MonoBehaviour
    {
        private NavMeshAgent _agent;

        public bool IsDone { get; private set; } = true;
        
        private bool Moving => Vector3.Distance(transform.position, _agent.destination) > _agent.stoppingDistance + 1F; 
        
        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        public void Act(TimelineManager.TimelineAction action)
        {
            StartCoroutine(ActDo(action));
        }

        private IEnumerator ActDo(TimelineManager.TimelineAction action)
        {
            IsDone = false;

            MoveTo(action.chosenDoor.transform.position);
            yield return new WaitUntil(() => !Moving);
            
            action.target.EnterRoom(this);
            
            MoveTo(action.target.center);
            yield return new WaitUntil(() => !Moving);

            IsDone = true;
        }
        
        private void MoveTo(Vector3 position)
        {
            if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 2.0F, NavMesh.AllAreas))
                return;
            _agent.SetDestination(hit.position);
        }

        private void OnMouseDown()
        {
            TimelineManager.Instance.SelectPlayer(this);
        }
    }
}
