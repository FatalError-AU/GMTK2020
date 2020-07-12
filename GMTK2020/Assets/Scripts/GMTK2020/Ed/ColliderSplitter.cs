using UnityEditor;
using UnityEngine;

namespace GMTK2020.Ed
{
    public class ColliderSplitter
    {
        [MenuItem("Tools/Fatal Error/Split Colliders")]
        private static void Split()
        {
            Undo.RecordObjects(Selection.gameObjects, "Split Colliders");
            Undo.IncrementCurrentGroup();
            int gi = Undo.GetCurrentGroup();
            
            foreach (GameObject obj in Selection.gameObjects)
            {
                foreach (Collider col in obj.GetComponents<Collider>())
                {
                    UnityEditorInternal.ComponentUtility.CopyComponent(col);
                    GameObject sub = new GameObject(col.GetType().Name);
                    sub.transform.SetParent(obj.transform);
                    sub.transform.localPosition = Vector3.zero;
                    sub.transform.localRotation = Quaternion.identity;
                    sub.transform.localScale = Vector3.one;
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(sub);
                    
                    Object.DestroyImmediate(col);
                    Undo.RegisterCreatedObjectUndo(sub, "MN");
                }
            }
            
            Undo.CollapseUndoOperations(gi);
        }
    }
}
