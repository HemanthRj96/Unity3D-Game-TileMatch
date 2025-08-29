using System;
using UnityEngine;
using UnityEngine.Events;


public class ExecuteAfterDuration : MonoBehaviour
{
    public UnityEvent task;

    public void ExcuteAfter(float time)
    {
        Invoke("taskMethod", time);
    }

    private void taskMethod()
    {
        task?.Invoke();
    }
}
