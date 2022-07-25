using UnityEngine;

using IDebug;

public class BaseObject
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
