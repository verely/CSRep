using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteEvents
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    class Subscriber
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // configure the .Net remoting infrastructure
                string configFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                RemotingConfiguration.Configure(configFile);


                Thread thread = new Thread(new ThreadStart(ThreadProc));
                thread.Start();

                Console.WriteLine("Subscriber running.\nPress ENTER to quit...");
                Console.ReadLine();

                stopEvent.Set();
                thread.Join();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        // this procedure will continously try to attach itself to the server 
        // to add a subscription
        static ManualResetEvent stopEvent = new ManualResetEvent(false);
        static void ThreadProc()
        {
            string url = "tcp://localhost:8123/Publisher";
            IRemoteEventsPublisher publisher = (IRemoteEventsPublisher)RemotingServices.Connect(typeof(IRemoteEventsPublisher), url);

            while (!stopEvent.WaitOne(0, true))
            {
                try
                {
                    publisher.AddSubscription(typeof(IApplicationEventsSink), "tcp://localhost:8456/Subscriber");
                    return;
                }
                catch (Exception e)
                {
                    /*
                     This exception is most likely because the remote server 
                     is down. So, we just notify the user by printing this exception to the console and
                     try again.
                    */
                    Console.WriteLine("Will try again to add a subscription.");
                }
            }
        }
    }
}
