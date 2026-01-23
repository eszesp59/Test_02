using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Diagnostics;

namespace UMSHost
{
    public class LoggingPipelineModule : HubPipelineModule
    {
        protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
        {
            Debug.WriteLine(string.Format("{0}===> Invoking {1}  on hub {2}",  Environment.NewLine, context.MethodDescriptor.Name, context.MethodDescriptor.Hub.Name));
            return base.OnBeforeIncoming(context);
        }
        protected override bool OnBeforeOutgoing(IHubOutgoingInvokerContext context)
        {
            Debug.WriteLine(string.Format("<=== Invoking {0} on client hub {1}", context.Invocation.Method, context.Invocation.Hub));
            return base.OnBeforeOutgoing(context);
        }
    }
}