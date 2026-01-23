using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace UMSHost
{
    public class QueryStringOAuthBearerProvider : OAuthBearerAuthenticationProvider
    {
        readonly string _name;

        public QueryStringOAuthBearerProvider(string name)
        {
            _name = name;
        }

        public override Task RequestToken(OAuthRequestTokenContext context)
        {
            //var value = context.Request.Query.Get(_name);
            //var value = context.Request.Headers[_name];

            string value = string.Empty;
            if (string.IsNullOrEmpty(value))
            {
                try
                {
                    value = context.Request.Query.Get(_name);
                }
                catch { }
            }
            if (string.IsNullOrEmpty(value))
            {
                try
                {
                    value = context.Request.Headers[_name];
                }
                catch { }
            }


            if (!string.IsNullOrEmpty(value))
            {
                context.Token = value;
            }

            return Task.FromResult<object>(null);
        }
    }
}