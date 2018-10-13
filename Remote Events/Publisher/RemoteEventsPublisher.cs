using System;

namespace RemoteEvents
   
{
	/// <summary>
	/// Summary description for Class2.
	/// </summary>
    class RemoteEventsPublisher
        : MarshalByRefObject, IRemoteEventsPublisher
    {
     
        public void AddSubscription(Type type, string url)
        {            
            SubscriptionsManager.AddSubscription(type, url);
        }
        public void AddSubscription(Type type, string methodName, string url)
        {
            SubscriptionsManager.AddSubscription(type, methodName, url, true);
        }

       
        public void RemoveSubscription(Type type, string url)
        {
            SubscriptionsManager.RemoveSubscription(type, url);
        }
        public void RemoveSubscription(Type type, string methodName, string url)
        {
            SubscriptionsManager.RemoveSubscription(type, methodName, url, true);
        }


       


    }
}
