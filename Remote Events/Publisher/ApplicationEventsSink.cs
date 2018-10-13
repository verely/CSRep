using System;

namespace RemoteEvents
{
    /// <summary>
    /// This is the server side IApplicationEventSink implementation. It is a simple wrapper class around the 
    /// dispatch event mechanism.
    /// </summary>
    public class ApplicationEventsSink : IApplicationEventsSink
    {
        public ApplicationEventsSink()
        {
            SubscriptionsManager.SupportSubscription(typeof(IApplicationEventsSink));
        }

        public void OnTime(TimeEventArgs args)
        {
            EventSubscription[] Subscriptions = SubscriptionsManager.GetSubscriptions(typeof(IApplicationEventsSink), "OnTime");
            foreach(EventSubscription sub in Subscriptions)
                sub.DispatchEvent(args);
        }

        public void OnClick(ClickEventArgs args)
        {
            EventSubscription[] Subscriptions = SubscriptionsManager.GetSubscriptions(typeof(IApplicationEventsSink), "OnClick");
            foreach(EventSubscription sub in Subscriptions)
                sub.DispatchEvent(args);
        }
    }
}
