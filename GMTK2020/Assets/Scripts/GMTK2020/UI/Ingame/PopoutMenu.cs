using System;
using GMTK2020.ActionTimeline;
using UnityEngine;

namespace GMTK2020.UI.Ingame
{
    public class PopoutMenu : MonoBehaviour
    {
        public Action<TimelineManager.TimelineAction.MoveType> callback;

        public void Move()
        {
            callback(TimelineManager.TimelineAction.MoveType.Move);
        }

        public void Breach()
        {
            callback(TimelineManager.TimelineAction.MoveType.Breach);
        }
    }
}
