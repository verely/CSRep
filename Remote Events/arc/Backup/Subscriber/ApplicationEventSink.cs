using System;
using System.Security.Permissions;

namespace RemoteEvents
{
		
    /// <summary>
    ///  Subscriber's implementation of IApplicationEventsSink
    /// </summary>
    //[SecurityPermission(SecurityAction.InheritanceDemand)]
    class ApplicationEventSink : MarshalByRefObject, IApplicationEventsSink
    {
        //[SecurityPermission(SecurityAction.LinkDemand)]
        public void OnTime(TimeEventArgs args)
        {
            Console.WriteLine(args.Time);
        }

        //[SecurityPermission(SecurityAction.LinkDemand)]
        public void OnClick(ClickEventArgs args)
        {
            Console.WriteLine(args.Value);
        }
    }
}
