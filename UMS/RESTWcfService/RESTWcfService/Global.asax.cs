using System;
using System.ServiceModel.Activation;
using System.Web.Routing;
using SwaggerWcf;

namespace RESTWcfService
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            RouteTable.Routes.Add(new ServiceRoute("api-docs", new WebServiceHostFactory(), typeof(SwaggerWcfEndpoint)));
        }
    }
}