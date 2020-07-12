using System.Collections.Generic;
using System.Linq;
using GMTK2020.Environment;
using InspectorGadgets.Editor;
using UnityEditor;
using UnityEngine;

namespace GMTK2020.Ed
{
    [CustomEditor(typeof(RoomDefinition))]
    public class RoomDefEditor : Editor<RoomDefinition>
    {
        private void OnSceneGUI()
        {
            Handles.matrix = Target.transform.localToWorldMatrix;

            for (int i = 0; i < Target.points.Length; i++)
            {
                Handles.color = Color.cyan;
                ref Vector2 point = ref Target.points[i];
                Vector3 value = new Vector3(point.x, 0F, point.y);
                value = Handles.FreeMoveHandle(value, Quaternion.identity, .15F, Vector3.zero, Handles.DotHandleCap);
                point = new Vector2(value.x, value.z);

                if (Target.points.Length > 3)
                {
                    Handles.color = Color.red;
                    if(Handles.Button(value + Camera.current.transform.up * .3F, Quaternion.identity, .05F, .1F, Handles.DotHandleCap))
                    {
                        List<Vector2> l = Target.points.ToList();
                        l.RemoveAt(i);
                        Target.points = l.ToArray();
                    }
                }
                
                Handles.color = Color.black;
            }

            for (int i = 0; i < Target.points.Length; i++)
            {
                int neg1 = i - 1;
                if (i == 0)
                    neg1 = Target.points.Length - 1;

                Vector2 one = Target.points[neg1];
                Vector2 two = Target.points[i];

                Vector2 m = (two + one) / 2F;

                Handles.color = Color.green;
                if (Handles.Button(new Vector3(m.x, 0F, m.y), Quaternion.identity, .05F, .1F, Handles.DotHandleCap))
                {
                    List<Vector2> l = Target.points.ToList();
                    l.Insert(i, m);
                    Target.points = l.ToArray();
                }
            }
        }
    }
}