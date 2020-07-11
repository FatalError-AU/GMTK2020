using UnityEngine;
using UnityEngine.AI;

namespace GMTK2020.ActionTimeline
{
    public class TimelineActor : MonoBehaviour
    {
        private NavMeshAgent _agent;

        public bool Moving => (_agent.hasPath && _agent.remainingDistance <= _agent.stoppingDistance + 1.0F) ||
                              !_agent.hasPath || _agent.pathStatus == NavMeshPathStatus.PathComplete;
        
        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        public void MoveTo(Vector3 position)
        {
            if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1.0F, NavMesh.AllAreas))
                return;
            _agent.SetDestination(hit.position);
        }
    }
}
