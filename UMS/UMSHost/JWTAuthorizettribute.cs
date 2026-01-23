using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace UMSHost
{
    //[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class JWTAuthorizeAttribute : AuthorizeAttribute
    {

        protected override bool UserAuthorized(System.Security.Principal.IPrincipal user)
        {
            bool rc = false;

            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            rc = user.Identity.IsAuthenticated;



            return rc;
        }

        public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            bool rc = base.AuthorizeHubConnection(hubDescriptor, request);
            return rc;
        }

        public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        {
            //if (hubIncomingInvokerContext.Hub.Context.User==null)
            //{
            //    hubIncomingInvokerContext.Hub.Context.User = new WindowsPrincipal(new WindowsIdentity(hubIncomingInvokerContext.Args[0].ToString()));
            //}



            bool rc = base.AuthorizeHubMethodInvocation(hubIncomingInvokerContext, appliesToMethod);
            return rc;
        }

    }
}