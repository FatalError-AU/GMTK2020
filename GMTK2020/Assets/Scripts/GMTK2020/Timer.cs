using UltEvents;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float timer = 5F;
    public UltEvent ev;

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0F)
        {
            timer = float.MaxValue;
            ev.InvokeX();
            Destroy(this);
        }
    }
}
