using GlobalFunctions_NS;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.Remoting.Contexts;

namespace SendUMSMessage_NS
{
    // direkt nem irom at, mas rendszer is hasznalhatja
    [SuppressMessage("SonarAnalyzer.CSharp", "S101")]
    public class SendUMSMessage : ISendUMSMessage, IDisposable
    {
        private bool disposed;
        private HubConnection hubConnection = null;


        #region -- Dispose --
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if ((disposing) && (hubConnection != null))
                {
                        hubConnection.Dispose();
                        hubConnection = null;
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                // Note disposing has been done.
                disposed = true;
            }
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
        #endregion -- Dispose --



        protected SendUMSMessage() : base()
        {
            // The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel
            // http://msdn.microsoft.com/en-us/library/bb408523.aspx
            // Validating X509 Certificates for SSL over HTTP in Exchange 2010
            // HACK: Az alábbi hack segítségével megbízunk a szerverünkben az X509 certificate bemutatása nélkül is.
            // Ez a probléma csak a szerveren jön elő, ahol az SSL certificate-ek egy Trusted Root már aláírta (és nem self-signed!)
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            // TLS1.2 beallitasa
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
        }



        public void Hello(string srcName, string env)
        {
            FileLog0.LogTextFile("--WCF Hello", $"srcName={srcName} env={env}", null, null);

            var umsHub = UMSInit();
            hubConnection.Start().Wait();

            if (umsHub != null)
            {
                umsHub.Invoke("Hello", srcName, env);
            }

            hubConnection.Stop();
        }

        public void Send(string srcName, string env, string name, string message)
        {
            FileLog0.LogTextFile("--WCF Send", $"srcName={srcName} env={env} name={name} Msg={message}", null, null);

            var umsHub = UMSInit();
            hubConnection.Start().Wait();

            if (umsHub != null)
            {
                umsHub.Invoke("Send", srcName, env, name, message);
            }

            hubConnection.Stop();
        }
        public void SendCL(string srcName, string env, string name, string message)
        {
            FileLog0.LogTextFile("--WCF SendCL", $"srcName={srcName} env={env} name={name} Msg={message}", null, null);

            var umsHub = UMSInit();
            hubConnection.Start().Wait();

            if (umsHub != null)
            {
                umsHub.Invoke("SendCL", srcName, env, name, message);
            }

            hubConnection.Stop();
        }

        public void SendToForm(string srcName, string env, string name, string formTypeID, string publicMessage, string privateMessage)
        {
            FileLog0.LogTextFile("--WCF SendToForm", $"srcName={srcName} env={env} name={name} formTypeID={formTypeID} publicMessage={publicMessage} privateMessage={privateMessage}", null, null);

            var umsHub = UMSInit();
            hubConnection.Start().Wait();

            if (umsHub != null)
            {
                umsHub.Invoke("SendToForm", srcName, env, name, formTypeID, publicMessage, privateMessage);
            }

            hubConnection.Stop();
        }

        public void SendToForm2(string uiID, string srcName, string env, string name, string formTypeID, string publicMessage, string privateMessage)
        {
            FileLog0.LogTextFile("--WCF SendToForm2",
                                $"uiID={uiID} srcName={srcName} env={env} name={name} formTypeID={formTypeID} publicMessage={publicMessage} privateMessage={privateMessage}", null, null);

            var umsHub = UMSInit();
            hubConnection.Start().Wait();

            if (umsHub != null)
            {
                umsHub.Invoke("SendToForm2", uiID, srcName, env, name, formTypeID, publicMessage, privateMessage);
            }

            hubConnection.Stop();
        }

        public void SendAll(string srcName, string env, string message)
        {
            FileLog0.LogTextFile("--WCF SendAll", $"srcName={srcName} env={env} message={message}", null, null);

            var umsHub = UMSInit();
            hubConnection.Start().Wait();

            if (umsHub != null)
            {
                umsHub.Invoke("SendAll", srcName, env, message);
            }

            hubConnection.Stop();
        }

        public void SendList(string srcName, string env, string[] nameList, string message)
        {
            var umsHub = UMSInit();
            hubConnection.Start().Wait();

            if (umsHub != null)
            {
                List<string> lst = new List<string>();
                foreach (string username in nameList)
                {
                    lst.Add(username);
                }
                umsHub.Invoke("SendList", srcName, env, lst, message);
            }

            hubConnection.Stop();
        }

        public void SetClient(string pass2UserID, string env, string macAddress)
        {
            var umsHub = UMSInit();
            hubConnection.Start().Wait();

            if (umsHub != null)
            {
                umsHub.Invoke("SetClient", pass2UserID, env, macAddress);
            }
        }

        public void KeszreJelentes(string macAddress, int sound, string message)
        {
            FileLog0.LogTextFile("--WCF KeszreJelentes", $"macAddress={macAddress} sound={sound} message={message}", null, null);

            var umsHub = UMSInit();
            hubConnection.Start().Wait();

            if (umsHub != null)
            {
                umsHub.Invoke("KeszreJelentes", macAddress, sound, message);
            }

            hubConnection.Stop();
        }


        public void Vontat(string macAddress, int sound, string message)
        {
            FileLog0.LogTextFile("--WCF Vontat", $"macAddress={macAddress} sound={sound} message={message}", null, null);

            var umsHub = UMSInit();
            hubConnection.Start().Wait();

            if (umsHub != null)
            {
                umsHub.Invoke("Vontat", macAddress, sound, message);
            }

            hubConnection.Stop();
        }

        public void PASS2Message(string srcName, string env, string app, string message)
        {
            FileLog0.LogTextFile("--WCF PASS2Message", $"srcName={srcName} env={env} app={app} message={message}", null, null);

            var umsHub = UMSInit();
            hubConnection.Start().Wait();

            if (umsHub != null)
            {
                umsHub.Invoke("PASS2Message", srcName, env, app, message);
            }

            hubConnection.Stop();
        }

        public void PASS2MessageCL(string srcName, string env, string pass2ID, string message)
        {
            FileLog0.LogTextFile("--WCF PASS2MessageCL", $"srcName={srcName} env={env} pass2ID={pass2ID} message={message}", null, null);

            var umsHub = UMSInit();
            hubConnection.Start().Wait();

            if (umsHub != null)
            {
                umsHub.Invoke("PASS2MessageCL", srcName, env, pass2ID, message);
            }

            hubConnection.Stop();
        }


        // egyelore nincs szukseg a gyorsan egymas utan jovo hivasok kiszuresere
        //// Szálbiztos tároló a kliensek utolsó üzenetének idejéről
        //// itt ugyan arra a target-re NE menjen túl gyakran üzenet
        //private static readonly ConcurrentDictionary<string, DateTime> _lastMessageTimes = new ConcurrentDictionary<string, DateTime>();


        public void For00FormOpen(string targetUser, string env, bool modal, string formName, string extraJsonData)
        {
            // egyelore nincs szukseg a gyorsan egymas utan jovo hivasok kiszuresere
            //try
            //{
            //    var now = DateTime.UtcNow;

            //    // Ellenőrizzük, mikor volt az utolsó hívás
            //    if (_lastMessageTimes.TryGetValue(targetUser, out var lastTime))
            //    {
            //        // Ha kevesebb mint 100ms telt el, az gyanús (bombázás)
            //        if ((now - lastTime).TotalMilliseconds < 100)
            //        {
            //            FileLog0.LogTextFile("UmsHub.For00FormOpen", $"Bombázás gyanú: {targetUser}", null, null);
            //            // Csendben eldobjuk a kérést (hogy ne terheljük a szervert válasszal)
            //            return;
            //        }
            //    }

            //    // Időbélyeg frissítése
            //    _lastMessageTimes[targetUser] = now;
            //}
            //catch (Exception ex)
            //{

            //    throw;
            //}


            try
            {
                FileLog0.LogTextFile("--WCF For00FormOpen - begin", $"targetUser={targetUser} env={env} modal={modal} formName={formName} extraJsonData={extraJsonData}", null, null);

                var umsHub = UMSInit();
                hubConnection.Start().Wait();

                if (umsHub != null)
                {
                    FileLog0.LogTextFile("--WCF For00FormOpen", "call local HUB - before", null, null);
                    umsHub.Invoke("For00FormOpen", targetUser, env, modal, formName, extraJsonData);
                    FileLog0.LogTextFile("--WCF For00FormOpen", "call local HUB - after", null, null);
                }

                hubConnection.Stop();
            }
            finally
            {
                FileLog0.LogTextFile("--WCF For00FormOpen - end", "", null, null);
            }
        }


        public void For00FormOpenCL(string targetUser, string env, bool modal, string formName, string extraJsonData)
        {
            try
            {
                FileLog0.LogTextFile("--WCF For00FormOpenCL - begin", $"targetUser={targetUser} env={env} modal={modal} formName={formName} extraJsonData={extraJsonData}", null, null);

                var umsHub = UMSInit();
                hubConnection.Start().Wait();

                if (umsHub != null)
                {
                    FileLog0.LogTextFile("--WCF For00FormOpenCL", "call local HUB - before", null, null);
                    umsHub.Invoke("For00FormOpenCL", targetUser, env, modal, formName, extraJsonData);
                    FileLog0.LogTextFile("--WCF For00FormOpenCL", "call local HUB - after", null, null);
                }

                hubConnection.Stop();
            }
            finally
            {
                FileLog0.LogTextFile("--WCF For00FormOpenCL - end", "", null, null);
            }
        }



        private IHubProxy UMSInit()
        {
            IHubProxy rc = null;
            try
            {
                // get connection string
                NameValueCollection appSettings = ConfigurationManager.AppSettings;
                string conStr = appSettings["UmsHost"];
                // sajat parameterek kuldese
                Dictionary<string, string> connParams = new Dictionary<string, string>();
                connParams.Add("Pass2UserID", "");
                connParams.Add("EnvironmentName", "");
                connParams.Add("MacAddress", "");
                hubConnection = new HubConnection(conStr, connParams);
                hubConnection.Credentials = CredentialCache.DefaultCredentials;
                //  Start() elott kell a proxy-kat letrehozni
                rc = hubConnection.CreateHubProxy("UmsHub");
            }
            catch (Exception ex)
            {
                FileLog0.LogTextFileEx("UMSInit", FileLog0.MakeExceptionMessages(ex), null, null);
            }
            return rc;
        }
    }
}
