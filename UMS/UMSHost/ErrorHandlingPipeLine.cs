using GlobalFunctions_NS;
using Microsoft.AspNet.SignalR.Hubs;
using System.Diagnostics;

namespace UMSHost
{
    public class ErrorHandlingPipelineModule : HubPipelineModule
    {
        protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
        {
            Debug.WriteLine("===> Exception " + FileLog0.MakeExceptionMessages(exceptionContext.Error));
            base.OnIncomingError(exceptionContext, invokerContext);
        }
    }

}