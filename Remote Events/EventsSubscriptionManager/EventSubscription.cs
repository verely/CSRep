using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Serialization;

namespace RemoteEvents
{
    /// <summary>
    /// This class describes an event subscription. It requires the type of the events
    /// sink interface, e.g. IApplicationEventsSink, the event handler' method name, and the
    /// subscriber's url.
    /// 
    /// The SubscriptionManager serializes an event subscription as soon as one is received
    /// </summary>
    [Serializable]
    public class EventSubscription : ISerializable 
    {
        private object _proxy;   // transparent proxy
        private MethodInfo _mi;  // method info
        private string _url;     // the subscriber's url

        delegate void EventDelegate(RemoteEventArgs args);
        EventDelegate _eventDelegate;

        /// <summary>
        /// The initialization is delegated to a helper method
        /// </summary>
        /// <param name="type">the event sink's interface</param>
        /// <param name="methodName">the event handlers method name</param>
        /// <param name="url">the subscriber's url</param>
        public EventSubscription(Type type, string methodName, string url)
        {
            Initialize(type, methodName, url);
        }

        /// <summary>
        /// the initialization is called from the constructor.
        /// </summary>
        /// <param name="type">the event sink's interface</param>
        /// <param name="methodName">the event handlers method name</param>
        /// <param name="url">the subscriber's url</param>
        void Initialize(Type type, string methodName, string url)
        {
            // create the proxy right away
            _proxy = RemotingServices.Connect(type, url);

            // we just need to remember the event handle's method info
            _url = url;
            _mi = type.GetMethod(methodName);

            // the purpose of using a delegate is so that we can dispatch the event asynchronously
            // see DispatchEvent
            _eventDelegate = new EventDelegate(Invoke);
        }        
              
        /// <summary>
        /// the dispatch method that is responsible for the actual events dispatch
        /// </summary>
        /// <param name="args">an event argument that is a 'RemoteEventArgs' derived object</param>
        public void DispatchEvent(RemoteEventArgs args)
        {
            // a delegate is used here to dispatch the event on a thread pool thread
            _eventDelegate.BeginInvoke(args, null, null);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("type", _mi.DeclaringType);
            info.AddValue("methodName", _mi.Name);
            info.AddValue("url", _url);
        }

        protected EventSubscription(SerializationInfo info, StreamingContext context)
        {
            Type type = info.GetValue("type", typeof(Type)) as Type;
            string methodName = info.GetString("methodName");
            string url = info.GetString("url");
            Initialize(type, methodName, url);
        }

        /// <summary>
        /// This procedure is invoked on a threadpool thread. 
        /// </summary>
        /// <param name="args"></param>
        private void Invoke(RemoteEventArgs args)
        {
            try
            {
                // now really send this event
                _mi.Invoke(this._proxy, new object[] { args });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
