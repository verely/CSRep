using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace RemoteEvents
{
    /// <summary>
    /// This publisher just randomly dispatches some events.
    /// </summary>
    class Publisher
    {
        static ManualResetEvent stopEvent = new ManualResetEvent(false);

        /// <summary>
        /// The publisher's long running event dispatch procedure
        /// </summary>
        static void ThreadProc()
        {
            ApplicationEventsSink appEvents = new ApplicationEventsSink();

            int count = 0;
            while (!stopEvent.WaitOne(0, true))
            {
                Thread.Sleep(2000);

                if (stopEvent.WaitOne(0, true))
                    break;

                count++;
                if ((count % 5) == 0)
                {
                    // fire the OnClick event
                    ClickEventArgs onClick = new ClickEventArgs(count);
                    appEvents.OnClick(onClick);
                    Console.WriteLine(onClick.Value);
                }

                // fire the OnTime event
                TimeEventArgs onTime = new TimeEventArgs(DateTime.Now);
                appEvents.OnTime(onTime);
                Console.WriteLine(onTime.Time);

            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // config the .Net remoting infrastructure
                string configFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                System.Runtime.Remoting.RemotingConfiguration.Configure(configFile);

                // start the long running publishing process
                System.Threading.Thread thread = new Thread(new ThreadStart(ThreadProc));
                thread.Start();

                Console.WriteLine("Publisher running.\nPress ENTER to quit...");
                Console.ReadLine();

                stopEvent.Set();
                thread.Join();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
