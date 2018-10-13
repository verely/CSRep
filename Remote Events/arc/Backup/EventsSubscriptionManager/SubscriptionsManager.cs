using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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
        #region ISerializable
        protected EventSubscription(SerializationInfo info, StreamingContext context)
        {
            Type type = info.GetValue("type", typeof(Type)) as Type;
            string methodName = info.GetString("methodName");
            string url = info.GetString("url");
            Initialize(type, methodName, url);
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context) 
        {
            info.AddValue("type", this.mi.DeclaringType);
            info.AddValue("methodName", this.mi.Name);
            info.AddValue("url", this.url);
        }
        #endregion

        object proxy;   // transparent proxy
        MethodInfo mi;  // method info
        string url;     // the subscriber's url

        delegate void EventDelegate(RemoteEventArgs args);
        EventDelegate eventDelegate;

        /// <summary>
        /// the initialization is called from the constructor.
        /// </summary>
        /// <param name="type">the event sink's interface</param>
        /// <param name="methodName">the event handlers method name</param>
        /// <param name="url">the subscriber's url</param>
        void Initialize(Type type, string methodName, string url)
        {
            // create the proxy right away
            this.proxy = RemotingServices.Connect(type, url);

            // we just need to remember the event handle's method info
            this.mi = type.GetMethod(methodName);
            this.url = url;

            // the purpose of using a delegate is so that we can dispatch the event asynchronously
            // see DispatchEvent
            this.eventDelegate = new EventDelegate(this.Invoke);
        }


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
        /// the dispatch method that is responsible for the actual events dispatch
        /// </summary>
        /// <param name="args">an event argument that is a 'RemoteEventArgs' derived object</param>
        public void DispatchEvent(RemoteEventArgs args)
        {
            // a delegate is used here to dispatch the event on a thread pool thread
            this.eventDelegate.BeginInvoke(args, null, null);
        }


        /// <summary>
        /// This procedure is invoked on a threadpool thread. 
        /// </summary>
        /// <param name="args"></param>
        void Invoke(RemoteEventArgs args)
        {
            try 
            {
                // now really send this event
                this.mi.Invoke(this.proxy, new object[] { args } );
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }


    /// <summary>
    /// This class manages all the subscriptions. One of its important functions is also to persist 
    /// all subscriptions into a disk file. 
    /// </summary>
    public sealed class SubscriptionsManager 
    {
        /// <summary>
        /// This is a table of subscriptions lists. Each entry in this table consists of a Hashtable
        /// of EventSubscription objects.
        /// </summary>
        static Hashtable subscriptionTable = Hashtable.Synchronized( new Hashtable() );

        const string subscriptionsFileName = "Subscriptions.bin";

        /// <summary>
        /// writes the subscription table to disk
        /// </summary>
        static void Serialize()
        {
            FileStream file = new FileStream(subscriptionsFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            using(file)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(file, subscriptionTable);
            }
        }

        /// <summary>
        /// reads the subscription tablefrom disk
        /// </summary>
        static void Deserialize()
        {
            string pathName = Path.GetFullPath(subscriptionsFileName);
            if(!File.Exists(pathName))
                return;

            FileStream file = new FileStream(pathName, FileMode.Open, FileAccess.Read, FileShare.None);
            using(file)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                subscriptionTable = formatter.Deserialize(file) as Hashtable;
            }
        }


        /// <summary>
        /// This is called one at start up.
        /// </summary>
        static SubscriptionsManager()
        {
            // reads any persisted subscriptions from disk
            Deserialize();
        }

        /// <summary>
        /// validates the type to be 'RemoteEventArgs'
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static bool IsRemoteEventArgs(Type type)
        {
            return (type == null) ? false :
                (type != null && type == typeof(RemoteEventArgs)) ? true : IsRemoteEventArgs(type.BaseType);
        }

        /// <summary>
        /// creates the subscription lists for the entire event handler sink
        /// </summary>
        /// <param name="type">the type of the event handler sink</param>
        public static void SupportSubscription(Type type) 
        {
            foreach(MethodInfo mi in type.GetMethods())
            {
                ParameterInfo[] pi = mi.GetParameters();
                if(pi.Length > 0 && IsRemoteEventArgs(pi[0].ParameterType))
                    SupportSubscription(type, mi.Name, false);
            }

            Serialize();
        }

        /// <summary>
        /// Creates a specific subscription list in the subscription table. The subscription list is initially empty.
        /// </summary>
        /// <param name="type">the type of the event handler sink</param>
        /// <param name="methodName"> the method name of the event handler</param>
        /// <param name="persist"></param>
        public static void SupportSubscription(Type type, string methodName, bool persist)
        {
            string key = string.Format("{0}.{1}", type.FullName, methodName);
            if(subscriptionTable[key] == null)
            {
                subscriptionTable[key] = Hashtable.Synchronized( new Hashtable() );
                if(persist)
                    Serialize();
            }
        }
        
        /// <summary>
        /// Adds the event subscription for all methods that 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="url"></param>
        public static void AddSubscription(Type type, string url)
        {
            foreach(MethodInfo mi in type.GetMethods())
            {
                ParameterInfo[] pi = mi.GetParameters();
                if(pi.Length > 0 && IsRemoteEventArgs(pi[0].ParameterType))
                    AddSubscription(type, mi.Name, url, false);
            }
            
            Serialize();
        }
        
        /// <summary>
        /// Adds a specific subscription to the SubscriptionManager
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="url"></param>
        /// <param name="persist"></param>
        public static void AddSubscription(Type type, string methodName, string url, bool persist)
        {
            string key = string.Format("{0}.{1}", type.FullName, methodName);
            
            // first check if this subscription exist.
            // the url will tell so. Do not add a subscription twice
            Hashtable subscriptionList = subscriptionTable[key] as Hashtable;

            key = string.Format("{0}.{1}", url, key);
            if(subscriptionList[key] == null)
                subscriptionList[key] = new EventSubscription(type, methodName, url);
          
            if(persist)
                Serialize();
        }

        /// <summary>
        /// Removes a subscription for an entire event sink interface
        /// </summary>
        /// <param name="type">the event sink type</param>
        /// <param name="url">the subscriber's event sink</param>
        public static void RemoveSubscription(Type type, string url)
        {
            // just iterate over the entire type and then remove the subscription one bey one
            foreach(MethodInfo mi in type.GetMethods())
            {
                ParameterInfo[] pi = mi.GetParameters();
                if(pi.Length > 0 && IsRemoteEventArgs(pi[0].ParameterType))
                    RemoveSubscription(type, mi.Name, url, false);
            }

            // serialize after subscription removal
            Serialize();
        }

        /// <summary>
        /// Removes a specific subscription for a specific event handle on a specific event sink
        /// </summary>
        /// <param name="type">the event sink type</param>
        /// <param name="methodName">the method name of the event handler</param>
        /// <param name="url">the subscriber's url</param>
        /// <param name="persist"></param>
        public static void RemoveSubscription(Type type, string methodName, string url, bool persist)
        {
            // first get the subscription list from the tabel
            // then remove it if there is a subscription for this url
            string key = string.Format("{0}.{1}", type.FullName, methodName);
            Hashtable subscriptionList = subscriptionTable[key] as Hashtable;

            key = string.Format("{0}.{1}", url, key);
            if(subscriptionList[key] != null)
            {
                subscriptionList.Remove(key);

                if(persist)
                    Serialize();
            }
        }

        /// <summary>
        /// Gets the list of event subscriptions for a specific event handler of a specific 
        /// event handler sink
        /// </summary>
        /// <param name="type">the type of the event handler sink</param>
        /// <param name="methodName">the method name of the event handler</param>
        /// <returns></returns>
        public static EventSubscription[] GetSubscriptions(Type type, string methodName)
        {
            string key = string.Format("{0}.{1}", type.FullName, methodName);
            Hashtable subscriptionList = subscriptionTable[key] as Hashtable;
            if(subscriptionList == null)
                return new EventSubscription[0];

            ArrayList list = new ArrayList();
            foreach(DictionaryEntry item in subscriptionList)
                list.Add(item.Value);

            return (EventSubscription[])list.ToArray(typeof(EventSubscription));
        }
    }
}
