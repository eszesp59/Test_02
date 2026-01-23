using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace UMSHost
{
    public static class JwtConfig
    {
        private static string audience;
        private static string issuer;
        private static string key;

        static JwtConfig()
        {
            string jwtString = ConfigurationManager.AppSettings.Get("JWT");
            if (!string.IsNullOrEmpty(jwtString))
            {
                Dictionary<string, string> wqDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jwtString);
                audience = wqDict["Audience"];
                issuer = wqDict["Issuer"];
                key = wqDict["Key"];
            }
        }

        public static string Audience
        {
            get { return audience; }
        }

        public static string Issuer
        {
            get { return issuer; }
        }

        public static string Key
        {
            get { return key; }
        }

        public static string ToString()
        {
            return $"Audience={Audience}; Issuer={Issuer}; Key={Key};";
        }
    }
}
