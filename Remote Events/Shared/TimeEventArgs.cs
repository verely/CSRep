using System;

namespace RemoteEvents
{
    /// <summary>
    /// Describes the OnTime event
    /// </summary>
    [Serializable]
    public class TimeEventArgs : RemoteEventArgs
    {
        public TimeEventArgs(DateTime time)
        {
            Time = time;
        }
        public DateTime Time;
    }
}
