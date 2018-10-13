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
}
