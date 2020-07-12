using System;
using System.Collections.Generic;
using UnityEngine;

namespace GMTK2020.Environment
{
    public class RoomDefinition : MonoBehaviour
    {
        [NonSerialized] public Vector3 center;
        
        public RoomDoor[] doors;
        
        [HideInInspector]
        public Vector2[] points =
        {
            new Vector2(-1, -1), 
            new Vector2(1, -1), 
            new Vector2(1, 1), 
            new Vector2(-1, 1), 
        };

        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("Room Definition");
            
            Mesh m = new Mesh();

            List<Vector3> verts = new List<Vector3>();
            List<int> indicies = new List<int>();
            
            for (int i = 0; i < points.Length; i++)
                verts.Add(new Vector3(points[i].x, 0F, points[i].y));
            
            for (int i = 2; i < points.Length; i++)
            {
                indicies.Add(0);
                indicies.Add(i - 1);
                indicies.Add(i);
            }

            for (int i = points.Length - 2; i >= 0; i--)
            {
                indicies.Add(points.Length - 1);
                indicies.Add(i + 1);
                indicies.Add(i);
            }
            
            m.SetVertices(verts);
            m.SetIndices(indicies, MeshTopology.Triangles, 0);

            MeshCollider col = gameObject.AddComponent<MeshCollider>();
            col.sharedMesh = m;

            center = transform.localToWorldMatrix * col.bounds.center;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;

            Gizmos.matrix = transform.localToWorldMatrix;

            for (int i = 0; i < points.Length; i++)
            {
                int neg1 = i - 1;
                if (i == 0)
                    neg1 = points.Length - 1;
                
                Vector2 from = points[neg1]; 
                Vector2 to = points[i]; 
                
                Gizmos.DrawLine(new Vector3(from.x, 0, from.y), new Vector3(to.x, 0, to.y));   
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;

            foreach (RoomDoor door in doors)
            {
                Gizmos.DrawWireSphere(door.transform.position, .5F);
            }
        }
    }
}
