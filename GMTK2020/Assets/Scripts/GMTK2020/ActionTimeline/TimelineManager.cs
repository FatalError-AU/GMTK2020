using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using InspectorGadgets.Attributes;
using UnityEngine;

namespace GMTK2020.ActionTimeline
{
    public class TimelineManager : MonoBehaviour
    {
        public static TimelineManager Instance { get; private set; }
        
        [LabelledCollection("Player A", "Player B", "Player C", "Player D")]
        public TimelineActor[] players = {};

        public ReadOnlyCollection<TimelineAction[]> Timeline => new ReadOnlyCollection<TimelineAction[]>(_timeline);
        private readonly List<TimelineAction[]> _timeline = new List<TimelineAction[]>();

        private int selectedActor = -1;
        
        private void Awake()
        {
            Instance = this;
        }

        //TODO temp
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
                selectedActor = 0;
            if (Input.GetKeyDown(KeyCode.W))
                selectedActor = 1;
            if (Input.GetKeyDown(KeyCode.E))
                selectedActor = 2;
            if (Input.GetKeyDown(KeyCode.R))
                selectedActor = 3;
            
            if (selectedActor != -1 && Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    players[selectedActor].MoveTo(hit.point);
                }
                else
                {
                    Debug.LogError("No point found");
                }
            }
        }

        private void OnValidate()
        {
            Array.Resize(ref players, 4);
        }

        public void AppendAction(int player, TimelineAction action)
        {
            int index = 0;
            
            if(_timeline.Count == 0)
                _timeline.Add(new TimelineAction[players.Length]);
            else if (_timeline.Last()[player] == null)
                index = _timeline.Count - 1;
            else
            {
                index = _timeline.Count;
                _timeline.Add(new TimelineAction[players.Length]);
            }

            _timeline[index][player] = action;
        }

        // Move
        // Breach
        
        public class TimelineAction
        {
            public MoveType moveType;
            
            //TODO room points
            public Vector3 point;
            
            public enum MoveType
            {
                Move,
                Breach
            }
        }
    }
}
