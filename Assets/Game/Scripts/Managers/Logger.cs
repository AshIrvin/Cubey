using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logger : MonoBehaviour
{
    public static Logger Instance;

    public bool EnableLogs = false;
    public bool EnableErrorLogs = false;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowDebugLog(object t, Object sender = null)
    {
        if (!EnableLogs) return;

        Debug.Log(t, sender);
    }

    public void ShowDebugError(object t, Object sender = null)
    {
        if (!EnableErrorLogs) return;

        Debug.LogError(t, sender);
    }
}
