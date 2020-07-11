using UnityEngine;

namespace GMTK2020
{
    public class Initialization : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            GameObject[] objects = Resources.LoadAll<GameObject>("Init");

            foreach (GameObject obj in objects)
                Instantiate(obj);
        }
    }
}
