﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GMTK2020.Environment;
using GMTK2020.FX;
using GMTK2020.UI;
using GMTK2020.UI.Ingame;
using InspectorGadgets.Attributes;
using UnityEngine;
using UnityEngine.AI;

namespace GMTK2020.ActionTimeline
{
    public class TimelineManager : MonoBehaviour
    {
        public static TimelineManager Instance { get; private set; }

        [LabelledCollection("Player A", "Player B", "Player C", "Player D")]
        public TimelineActor[] players = { };

        public GameObject popoutMenuTemplate;

        public ReadOnlyCollection<TimelineAction[]> Timeline => new ReadOnlyCollection<TimelineAction[]>(_timeline);
        private readonly List<TimelineAction[]> _timeline = new List<TimelineAction[]>();

        private Camera _cam;

        private GameObject _currentlyOpenMenu;
        private RoomDefinition _selectedRoom;

        public int SelectedActor { get; private set; } = -1;

        private bool _isBuildingPath = true;
        private int _step = 0;

        private void Awake()
        {
            Instance = this;
            _cam = Camera.main;
        }

        private void Update()
        {
            if (!_isBuildingPath)
            {
                SelectedActor = -1;
                if (_currentlyOpenMenu)
                    Destroy(_currentlyOpenMenu);

                ProcessTimeline();

                return;
            }

            if (_currentlyOpenMenu)
            {
                if (Input.GetMouseButtonDown(0) || Input.anyKeyDown)
                    _currentlyOpenMenu.GetComponent<UnfoldController>().Reverse();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _isBuildingPath = false;
                ProcessTimeline();
            }

            if (Input.GetKeyDown(KeyCode.Q))
                SelectedActor = 0;
            if (Input.GetKeyDown(KeyCode.W))
                SelectedActor = 1;
            if (Input.GetKeyDown(KeyCode.E))
                SelectedActor = 2;
            if (Input.GetKeyDown(KeyCode.R))
                SelectedActor = 3;

            if (SelectedActor == -1 || !Input.GetMouseButtonDown(0)) return;

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, float.PositiveInfinity, LayerMask.GetMask("Room Definition")))
            {
                _currentlyOpenMenu = Instantiate(popoutMenuTemplate);
                _currentlyOpenMenu.GetComponent<PopoutMenu>().callback = ButtonsCallback;

                RectTransform rect = _currentlyOpenMenu.GetComponent<RectTransform>();
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, null,
                    out Vector2 point);
                _currentlyOpenMenu.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = point;

                _selectedRoom = hit.transform.GetComponent<RoomDefinition>();
            }
        }

        public bool IsTurnDone(TimelineActor except = null)
        {
            return !players.Any(x => !x.IsDone && x != except);
        }
        
        private void ProcessTimeline()
        {
            if (!Input.GetKeyDown(KeyCode.Space))
                return;

            if (IsTurnDone())
                return;

            if (_timeline.Count <= _step)
                return;

            TimelineAction[] actions = _timeline[_step];

            if (actions == null) return;
            for (int i = 0; i < players.Length; i++)
            {
                if (actions[i] != null)
                    players[i].Act(actions[i]);
            }

            _step++;
        }

        public void SelectPlayer(TimelineActor actor)
        {
            SelectedActor = players.ToList().IndexOf(actor);
        }

        private void OnValidate()
        {
            Array.Resize(ref players, 4);
        }

        private void ButtonsCallback(TimelineAction.MoveType type)
        {
            AppendAction(SelectedActor, new TimelineAction(type, _selectedRoom));
        }

        public void AppendAction(int player, TimelineAction action)
        {
            int index = -1;

            if (_timeline.Count == 0)
            {
                _timeline.Add(new TimelineAction[players.Length]);
                index = 0;
            }
            else
            {
                for (int i = 0; i < _timeline.Count; i++)
                {
                    if (_timeline[i][player] != null) continue;
                    
                    index = i;
                    break;
                }
            }

            if(index < 0)
            {
                index = _timeline.Count;
                _timeline.Add(new TimelineAction[players.Length]);
            }

            Vector3 source =
                index == 0 ? players[player].transform.position : _timeline[index - 1][player].target.center;

            float minDistance = float.MaxValue;

            RoomDefinition previous = index == 0 ? RoomDefinition.OutsideRoom : _timeline[index - 1][player].target;

            if (!previous.doors.Any(x => action.target.doors.Any(y => y == x)))
            {
                PopupMessage.Show("Can only move between two connected rooms");
                return;
            }

            foreach (RoomDoor door in action.target.doors)
            {
                if (door.requireBreach && action.moveType != TimelineAction.MoveType.Breach)
                    continue;

                NavMeshPath path = new NavMeshPath();

                if (!NavMesh.CalculatePath(source, door.transform.position, NavMesh.AllAreas, path))
                    continue;

                Vector3[] corners = path.corners;
                float distance = .0F;
                for (int i = 0; i < corners.Length; i++)
                {
                    Vector3 prev = source;
                    if (i > 0)
                        prev = corners[i - 1];

                    distance += Vector3.Distance(prev, corners[i]);
                }

                distance += Vector3.Distance(corners.LastOrDefault(), door.transform.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    action.chosenDoor = door;
                }
            }

            if (!action.chosenDoor)
            {
                PopupMessage.Show("Unreachable room");
                return;
            }

            _timeline[index][player] = action;
        }


        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!UnityEditor.EditorApplication.isPlaying)
                return;
                
            Gizmos.color = Color.green;
            if (_timeline == null) return;

            Vector3[] prev = new Vector3[players.Length];
            for (int i = 0;
                i < players.Length;
                i++)
                prev[i] = players[i].transform.position;
            foreach (TimelineAction[] ta in _timeline)
            {
                for (int i = 0; i < ta.Length; i++)
                {
                    if (ta[i] == null)
                        continue;

                    NavMeshPath path = new NavMeshPath();
                    if (!NavMesh.CalculatePath(prev[i], ta[i].chosenDoor.transform.position, NavMesh.AllAreas, path))
                    {
                        Gizmos.DrawLine(prev[i], ta[i].chosenDoor.transform.position);
                        continue;
                    }

                    Vector3[] corners = path.corners;

                    for (int j = 1; j < corners.Length; j++)
                        Gizmos.DrawLine(corners[j - 1], corners[j]);

                    prev[i] = ta[i].target.center;
                    Gizmos.DrawLine(corners.Last(), prev[i]);
                }
            }
        }
        #endif

        public class TimelineAction
        {
            public MoveType moveType;
            public RoomDefinition target;

            public RoomDoor chosenDoor;

            public TimelineAction(MoveType type, RoomDefinition target)
            {
                moveType = type;
                this.target = target;
            }

            public enum MoveType
            {
                Move,
                Breach
            }
        }
    }
}