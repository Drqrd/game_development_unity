using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IDebug;

public class BaseObjectMono : MonoBehaviour
{
    protected TimeTracker debugTimeTracker;
    protected void TryLogStart(string toLog)
    {
        if (debugTimeTracker != null) debugTimeTracker.LogTimeStart(toLog);
        else Debug.LogWarning("Tried tracking time when timeTracking not enabled: " + this);
    }

    protected void TryLogElapsed(string toLog)
    {
        if (debugTimeTracker != null) debugTimeTracker.LogTimeElapsed(toLog);
        else Debug.LogWarning("Tried tracking time when timeTracking not enabled: " + this);
    }

    protected void TryLogEnd()
    {
        if (debugTimeTracker != null) debugTimeTracker.LogTimeEnd();
        else Debug.LogWarning("Tried tracking time when timeTracking not enabled: " + this);
    }
}
