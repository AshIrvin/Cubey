using UnityEngine;

public class Logger : MonoBehaviour
{
    public static Logger Instance;

    public bool EnableLogs = true;
    public bool EnableErrorLogs = true;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
#if !UNITY_EDITOR
        EnableLogs = false;
        EnableErrorLogs = false;
#endif
    }

    public void ShowDebugLog(object text, Object sender = null)
    {
        if (!EnableLogs) return;

        Debug.Log(text, sender);
    }

    public void ShowDebugError(object text, Object sender = null)
    {
        if (!EnableErrorLogs) return;

        Debug.LogError($"<color:red>{text}", sender);
    }
}
