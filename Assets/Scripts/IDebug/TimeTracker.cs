using UnityEngine;

namespace IDebug
{
    public class TimeTracker
    {
        private float timeStart, timeElapsed;
        private bool totalTime;
        private int precision;
        private string startTitle;
        public TimeTracker(bool totalTime = true, int precision = 3)
        {
            this.totalTime = totalTime;
            this.precision = precision;
        }
        public void LogTimeStart(string title, GameObject trackedObject = null)
        {
            #if UNITY_EDITOR

            startTitle = title;
            timeStart = Time.realtimeSinceStartup;
            string totalTimeStr = (totalTime) ? $"Start Time: {System.Math.Round(timeStart, precision)}" : "";
            Debug.Log("-----------------------------------------");
            Debug.Log($"--- TIME START --- {totalTimeStr} - {startTitle}", trackedObject);

            #endif
        }

        public void LogTimeElapsed(string title = "", GameObject trackedObject = null)
        {
            #if UNITY_EDITOR

            timeElapsed = Time.realtimeSinceStartup;
            string totalTimeStr = (totalTime) ? $"Total Time: {System.Math.Round(Time.realtimeSinceStartup, precision)}" : "";
            string timeElapsedStr = $"{System.Math.Round(timeElapsed - timeStart, precision)}";
            Debug.Log($"--- Time Elapsed: {timeElapsedStr} - {totalTimeStr} - {title}", trackedObject);

            #endif
        }
        public void LogTimeEnd(string title = null, GameObject trackedObject = null)
        {
            #if UNITY_EDITOR

            string titleStr = (title == null) ? startTitle : title;
            string totalTimeStr = (totalTime) ? $"End Time: {System.Math.Round(Time.realtimeSinceStartup, precision)}" : "";
            string totalTimeElapsedStr = $"Total Time Elapsed: {System.Math.Round(Time.realtimeSinceStartup - timeStart, precision)}";
            Debug.Log($"--- TIME END --- {totalTimeStr} - {totalTimeElapsedStr} - {titleStr}", trackedObject);
            Debug.Log("-----------------------------------------");

            #endif
        }
    }
}
