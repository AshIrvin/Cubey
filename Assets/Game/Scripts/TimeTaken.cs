using System;
using UnityEngine;

public class TimeTaken : MonoBehaviour
{
    public enum TimeAction
    {
        Start,
        Stop
    }

    private static float timeDuration = 0;
    private static TimeAction timeAction;


    public static void TimeDuration(TimeAction timeAction, string methodName = "", Action callback = null)
    {
        switch (timeAction)
        {
            case TimeAction.Start:
                Logger.Instance.ShowDebugLog("Time started");
                timeDuration = Time.time;
                break;
            case TimeAction.Stop:
                float duration = Mathf.Abs(timeDuration - Time.time);
                Logger.Instance.ShowDebugLog(methodName + " time taken: " + Mathf.Round(duration * 100) / 100 + ", duration: " + duration);
                callback?.Invoke();
                break;
        }
    }
}
