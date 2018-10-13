using System;

namespace RemoteEvents
{
    /// <summary>
    /// interface to the remote publisher
    /// </summary>
    public interface IRemoteEventsPublisher
    {
        void AddSubscription(Type type, string url);
        void AddSubscription(Type type, string methodName, string url);

        void RemoveSubscription(Type type, string url);
        void RemoveSubscription(Type type, string methodName, string url);
    }


    /// <summary>
    /// The events sink interface known to both publisher and subscriber
    /// </summary>
    public interface IApplicationEventsSink 
    {
        void OnTime(TimeEventArgs args);
        void OnClick(ClickEventArgs args);
    }

    /// <summary>
    /// The remnote event arguments object
    /// </summary>
    [Serializable]
    public class RemoteEventArgs : EventArgs 
    {
        public bool Enqueue = true;
    }

    /// <summary>
    /// Describes the OnClick event
    /// </summary>
    [Serializable]
    public class ClickEventArgs : RemoteEventArgs
    {
        public ClickEventArgs(int n)
        {
            Value = string.Format("Click: {0}", n);
        }
        public string Value = "Click";
    }

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
