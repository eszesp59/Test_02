using GlobalFunctions_NS;
using MAVI.ARCH.UMS.DAL_NS;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Net.Mail;
using System.ServiceModel;
using System.Xml.Linq;
using UMSHost.SendUMSMessage_NS;

namespace UMSHost
{
    [HubName("UmsHub")]
    public class UmsHub : Hub<IUmsHub> //Hub
    {
        private bool disposed;
        private UmsHelper umsHelper=null;
        private readonly string ClientServiceName = "SendUMSMessage_NS.SendUMSMessage";


        public UmsHub()
        {
            //..FileLog0.LogTextFile("", null, null, null)
            //..FileLog0.LogTextFile("UmsHub.Constructor", string.Format("MachineName={0}",Environment.MachineName), null, null)

            // The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel
            // http://msdn.microsoft.com/en-us/library/bb408523.aspx
            // Validating X509 Certificates for SSL over HTTP in Exchange 2010
            // HACK: Az alábbi hack segítségével megbízunk a szerverünkben az X509 certificate bemutatása nélkül is.
            // Ez a probléma csak a szerveren jön elő, ahol az SSL certificate-ek egy Trusted Root már aláírta (és nem self-signed!)
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;


            // TLS1.2 beallitasa
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            umsHelper = new UmsHelper("UMSDB");
        }

        #region Dispose()
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            //..FileLog0.LogTextFile("UmsHub.Dispose", string.Format("disposing={0}", disposing), null, null)
            // Check to see if Dispose has already been called.
            if (!disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if ((disposing) &&  (umsHelper!=null))
                {
                        umsHelper.Dispose();
                        umsHelper = null;
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                // Note disposing has been done.
                disposed = true;

            }

            base.Dispose(disposing);
        }
        #endregion Dispose()



        private sealed class SendToForm2OtherMachineParam
        {
            public string uiID { get; set; }
            public string machineName { get; set; }
            public string srcName { get; set; }
            public string env { get; set; }
            public string name { get; set; }
            public string formTypeID { get; set; }
            public string publicMessage { get; set; }
            public string privateMessage { get; set; }
        }




        #region Altalanos fuggvenyek

        //Note: In JavaScript the reference to the server class and its members is in camel case.
        //The code sample references the C# ChatHub class in JavaScript as umsHub.
        public void Hello(string srcName, string env)
        {
            FileLog0.LogTextFile("UmsHub.Hello", $"ConnectionId={Context.ConnectionId} From={srcName} Env={env}", null, null);
            SendAll(srcName, env, "Hello");
        }



        public void Send(string srcName, string env, string name, string message)
        {
            try
            {
                FileLog0.LogTextFile("UmsHub.Send - begin", $"ConnectionId={Context.ConnectionId} From={srcName} Env={env} To={name} Msg={message}", null, null);

                // ha egynel tobb helyra kell kuldeni pl:tobb nyitott IUR ablak
                UserInfo[] uinfo = umsHelper.getAllUserInfoByPass2Id(name);

                foreach (UserInfo ui in uinfo)
                {
                    try
                    {
                        // csak a sajat kornyezetet vegye figyelembe
                        if ((ui.EnvName!=null) && ui.EnvName.Equals(env))
                        {
                            // ellenorzi, sajat gepen van-e a connection
                            if (Environment.MachineName.Equals(ui.MachineName))
                            {
                                ((Hub)this).Clients.Client(ui.Id).Send(srcName, env, message);
                                umsHelper.setMessage(ui.Id, srcName, message);
                                FileLog0.LogTextFile("UmsHub.Send", $"Sent to ConnectionId={ui.Id} == {ui.Pass2Id}", null, null);
                            }
                            else
                            {
                                sendOtherMachine(ui.MachineName, srcName, env, name, message);
                            }
                        }
                    }
                    catch (Exception ex1)
                    {
                        FileLog0.LogTextFile("UmsHub.Send_ex1", $"Sikertelen küldés: ConnectionId={ui.Id} == {ui.Pass2Id}", FileLog0.MakeExceptionMessages(ex1), null);
                    }
                }
                FileLog0.LogTextFile("UmsHub.Send - end", null, null, null);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                FileLog0.LogTextFile("UmsHub.Send_Catch", "*INFO*", ex.Message, null);
            }
            catch (Exception ex)
            {
                FileLog0.LogTextFileEx("UmsHub.Send_Catch",  FileLog0.MakeExceptionMessages(ex), null, null);
            }
        }
        private void sendOtherMachine(string machineName, string srcName, string env, string name, string message)
        {
            FileLog0.LogTextFile("UmsHub.sendOtherMachine - begin", $"machineName={machineName}  name={name} Msg={message}", null, null);
            var section = ConfigurationManager.GetSection("UMSservers") as NameValueCollection;
            var value = section[machineName];
            if (value != null)
            {
                Uri uri = new Uri(value);
                var address = new EndpointAddress(uri);
                // DV-ben : CustomBinding_IAAService
                // minden mas kornyezetben : CustomBinding_IAAService_Secure
                using (SendUMSMessageClient svc = new SendUMSMessageClient(ClientServiceName, address))
                {
                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                    svc.SendCL(srcName, env, name, message);
                }
                FileLog0.LogTextFile("UmsHub.sendOtherMachine - end", null, null, null);
            }
        }
        public void SendCL(string srcName, string env, string name, string message)
        {
            try
            {
                FileLog0.LogTextFile("UmsHub.SendCL - begin", $"ConnectionId={Context.ConnectionId} From={srcName} Env={env} To={name} Msg={message}", null, null);

                UserInfo uinfo = umsHelper.getUserInfoByPass2Id(name);

                // ellenorzi, sajat gepen van-e a connection
                if (Environment.MachineName.Equals(uinfo.MachineName))
                {
                    ((Hub)this).Clients.Client(uinfo.Id).Send(srcName, env, message);
                    umsHelper.setMessage(uinfo.Id, srcName, message);
                    FileLog0.LogTextFile("UmsHub.SendCL", $"Sent to ConnectionId={uinfo.Id} == {uinfo.Pass2Id}", null, null);
                }
                else
                {
                    FileLog0.LogTextFile("UmsHub.SendCL", $"Cél gép neve: {uinfo.MachineName} nem ez a gép", null, null);
                }
                FileLog0.LogTextFile("UmsHub.SendCL - end", null, null, null);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                FileLog0.LogTextFile("UmsHub.SendCL_Catch", "*INFO*", ex.Message, null);
            }
            catch (Exception ex)
            {
                FileLog0.LogTextFileEx("UmsHub.SendCL_Catch", FileLog0.MakeExceptionMessages(ex), null, null);
            }
        }


        public void SendAll(string srcName, string env, string message)
        {
            try
            {
                FileLog0.LogTextFile("UmsHub.SendAll - begin", $"ConnectionId={Context.ConnectionId} From={srcName} Env={env} Msg={message}", null, null);

                ((Hub)this).Clients.All.SendAll(srcName, env, message);
                FileLog0.LogTextFile("UmsHub.SendAll - end", null, null, null);
            }
            catch (Exception ex)
            {
                FileLog0.LogTextFileEx("UmsHub.SendAll_Catch", FileLog0.MakeExceptionMessages(ex), null, null);
            }
        }

        public void SendList(string srcName, string env, List<string> nameList, string message)
        {
            try
            {
                FileLog0.LogTextFile("UmsHub.SendList - begin", $"ConnectionId={Context.ConnectionId} From={srcName} Env={env} Msg={message}", null, null);

                List<string> connIdList = new List<string>();
                foreach (string username in nameList)
                {
                    string Wuid = "";
                    string Pass2Id = "";
                    try
                    {
                        // elnyeljuk, ha nincs, lehet mar kilepet
                        UserInfo ui = umsHelper.getUserInfoByPass2Id(username);
                        // csak a sajat kornyezetet vegye figyelembe
                        if ((ui.EnvName != null) && ui.EnvName.Equals(env))
                        {
                            Wuid = ui.Id;
                            Pass2Id = ui.Pass2Id;
                            umsHelper.setMessage(ui.Id, srcName, message);
                            connIdList.Add(ui.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        // ha nem sikerul egyet elkuldeni, attol a tobbinek menni kell
                        FileLog0.LogTextFileEx("UmsHub.SendList_Catch", $"sikertelen umsHelper.setMessage({Wuid} == {Pass2Id}-nak, {srcName}-tól, {message})", FileLog0.MakeExceptionMessages(ex), null);
                    }
                }

                ((Hub)this).Clients.Clients(connIdList).SendList(srcName, env, message);
                FileLog0.LogTextFile("UmsHub.SendList - end", null, null, null);
            }
            catch (Exception ex)
            {
                FileLog0.LogTextFileEx("UmsHub.SendList_Catch", FileLog0.MakeExceptionMessages(ex), null, null);
            }
        }
        #endregion Altalanos fuggvenyek





        public void SendToForm2(string uiID, string srcName, string env, string name, string formTypeID, string publicMessage, string privateMessage)
        {
            try
            {
                FileLog0.LogTextFile("UmsHub.SendToForm2 - begin",
                                     $"ConnectionId={Context.ConnectionId} From={srcName} Env={env} To={name} (Name_ConnectionId={uiID}) "
                                     +$"FormID={formTypeID} publicMsg={publicMessage} privateMsg={privateMessage}",
                                     null, null
                                    );

                // UserInfo[] uinfo = umsHelper.getAllUserInfoByPass2Id(name)
                ((Hub)this).Clients.Client(uiID).Send(srcName, env, publicMessage);
                ((Hub)this).Clients.Client(uiID).SendToForm(srcName, env, formTypeID, privateMessage);
                umsHelper.setMessage(uiID, srcName, privateMessage);
                FileLog0.LogTextFile("UmsHub.SendToForm2 - end", null, null, null);
            }
            catch (Exception ex)
            {
                FileLog0.LogTextFileEx("UmsHub.SendToForm2_Catch", FileLog0.MakeExceptionMessages(ex), null, null);
            }
        }



        #region Funkcionalis fuggvenyek

        #region Irasbeli
        public void SendToForm(string srcName, string env, string name, string formTypeID, string publicMessage, string privateMessage)
        {
            try
            {
                FileLog0.LogTextFile("UmsHub.SendToForm - begin",
                                     $"ConnectionId={Context.ConnectionId} From={srcName} Env={env} To={name} FormID={formTypeID} publicMsg={publicMessage} privateMsg={privateMessage}", null, null);

                // ha egynel tobb helyra kell kuldeni pl:tobb nyitott IUR ablak
                UserInfo[] uinfo = umsHelper.getAllUserInfoByPass2Id(name);

                foreach (UserInfo ui in uinfo)
                {
                    try
                    {
                        // csak a sajat kornyezetet vegye figyelembe
                        if ((ui.EnvName != null) && ui.EnvName.Equals(env))
                        {
                            // ellenorzi, sajat gepen van-e a connection
                            if (Environment.MachineName.Equals(ui.MachineName))
                            {
                                ((Hub)this).Clients.Client(ui.Id).Send(srcName, env, publicMessage);
                                ((Hub)this).Clients.Client(ui.Id).SendToForm(srcName, env, formTypeID, privateMessage);
                                umsHelper.setMessage(ui.Id, srcName, privateMessage);
                                FileLog0.DebugTxt("UmsHub.SendToForm", $"Sent to={name} (ConnectionId={ui.Id})", null, null);
                            }
                            else
                            {
                                SendToForm2OtherMachineParam par = new SendToForm2OtherMachineParam();
                                par.uiID = ui.Id;
                                par.machineName = ui.MachineName;
                                par.srcName = srcName;
                                par.env = env;
                                par.name = name;
                                par.formTypeID = formTypeID;
                                par.publicMessage = publicMessage;
                                par.privateMessage = privateMessage;

                                FileLog0.DebugTxt("UmsHub.SendToForm", "sendToForm2OtherMachine - before", null, null);
                                sendToForm2OtherMachine(par);
                                FileLog0.DebugTxt("UmsHub.SendToForm", "sendToForm2OtherMachine - after", null, null);
                            }
                        }
                    }
                    catch (Exception ex1)
                    {
                        FileLog0.LogTextFile("UmsHub.SendToForm_ex1", $"Sikertelen küldés: ConnectionId={ui.Id} == {ui.Pass2Id}", FileLog0.MakeExceptionMessages(ex1), null);
                    }
                }
                FileLog0.LogTextFile("UmsHub.SendToForm - end", null, null, null);
            }
            catch (Exception ex)
            {
                FileLog0.LogTextFileEx("UmsHub.SendToForm_Catch", FileLog0.MakeExceptionMessages(ex), null, null);
            }
        }
        private void sendToForm2OtherMachine(SendToForm2OtherMachineParam par)
        {
            var section = ConfigurationManager.GetSection("UMSservers") as NameValueCollection;
            var value = section[par.machineName];
            Uri uri = new Uri(value);
            var address = new EndpointAddress(uri);
            using (SendUMSMessageClient svc = new SendUMSMessageClient(ClientServiceName, address))
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                svc.SendToForm2(par.uiID, par.srcName, par.env, par.name, par.formTypeID, par.publicMessage, par.privateMessage);
            }
        }
        #endregion Irasbeli


        #region Keszrejelentes
        public void KeszreJelentes(string macAddress, int sound, string message)
        {
            try
            {
                FileLog0.LogTextFile("UmsHub.KeszreJelentes - begin", $"ConnectionId={Context.ConnectionId} macAddress={macAddress} (sound={sound}) message={message}", null, null);

                UserInfo uinfo = umsHelper.getUserInfoByMacAddress(macAddress);
                FileLog0.DebugTxt("UmsHub.KeszreJelentes", $"MachineName={uinfo.MachineName}", null, null);

                // ellenorzi, sajat gepen van-e a connection
                if (Environment.MachineName.Equals(uinfo.MachineName))
                {
                    Clients.Client(uinfo.Id).KeszreJelentes(macAddress, sound, message);
                    umsHelper.setMessage(uinfo.Id, macAddress, message);
                    FileLog0.DebugTxt("UmsHub.KeszreJelentes", $"Sent to={macAddress} Pass2UserID={uinfo.Pass2Id} ConnectionId={uinfo.Id}  message={message}", null, null);
                }
                else
                {
                    FileLog0.DebugTxt("UmsHub.KeszreJelentes", "keszreJelentes2OtherMachine - before", null, null);
                    keszreJelentes2OtherMachine(uinfo.MachineName, macAddress, sound, message);
                    FileLog0.DebugTxt("UmsHub.KeszreJelentes", "keszreJelentes2OtherMachine - after", null, null);
                }
                FileLog0.LogTextFile("UmsHub.KeszreJelentes - end", null, null, null);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                FileLog0.LogTextFile("UmsHub.KeszreJelentes_Catch", "*INFO*", ex.Message, null);
            }
            catch (Exception ex)
            {
                FileLog0.LogTextFileEx("UmsHub.KeszreJelentes_Catch", FileLog0.MakeExceptionMessages(ex), null, null);
            }
        }
        private void keszreJelentes2OtherMachine(string machineName, string macAddress, int sound, string message)
        {
            var section = ConfigurationManager.GetSection("UMSservers") as NameValueCollection;
            var value = section[machineName];
            if (value != null)
            {
                Uri uri = new Uri(value);
                var address = new EndpointAddress(uri);
                using (SendUMSMessageClient svc = new SendUMSMessageClient(ClientServiceName, address))
                {
                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                    svc.KeszreJelentes(macAddress, sound, message);
                }
            }
        }
        #endregion Keszrejelentes


        #region Vontat
        public void Vontat(string macAddress, int sound, string message)
        {
            try
            {
                FileLog0.LogTextFile("UmsHub.Vontat - begin", $"ConnectionId={Context.ConnectionId} macAddress={macAddress} sound={sound} message={message}", null, null);

                UserInfo uinfo = umsHelper.getUserInfoByMacAddress(macAddress);
                FileLog0.DebugTxt("UmsHub.Vontat", $"MachineName={uinfo.MachineName}", null, null);

                // ellenorzi, sajat gepen van-e a connection
                if (Environment.MachineName.Equals(uinfo.MachineName))
                {
                    Clients.Client(uinfo.Id).Vontat(macAddress, sound, message);
                    umsHelper.setMessage(uinfo.Id, macAddress, message);
                    FileLog0.DebugTxt("UmsHub.Vontat", $"Sent to={macAddress} Pass2UserID={uinfo.Pass2Id} ConnectionId={uinfo.Id}  message={message}", null, null);
                }
                else
                {
                    FileLog0.DebugTxt("UmsHub.Vontat", "vontat2OtherMachine - before", null, null);
                    vontat2OtherMachine(uinfo.MachineName, macAddress, sound, message);
                    FileLog0.DebugTxt("UmsHub.Vontat", "vontat2OtherMachine - after", null, null);
                }
                FileLog0.LogTextFile("UmsHub.Vontat - end", null, null, null);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                FileLog0.LogTextFile("UmsHub.Vontat_Catch", "*INFO*", ex.Message, null);
            }
            catch (Exception ex)
            {
                FileLog0.LogTextFileEx("UmsHub.Vontat_Catch", FileLog0.MakeExceptionMessages(ex), null, null);
            }
        }
        private void vontat2OtherMachine(string machineName, string macAddress, int sound, string message)
        {
            var section = ConfigurationManager.GetSection("UMSservers") as NameValueCollection;
            var value = section[machineName];
            if (value != null)
            {
                Uri uri = new Uri(value);
                var address = new EndpointAddress(uri);
                using (SendUMSMessageClient svc = new SendUMSMessageClient(ClientServiceName, address))
                {
                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                    svc.Vontat(macAddress, sound, message);
                }
            }
        }
        #endregion Vontat

        #region PASS2message
        public void PASS2Message(string srcName, string env, string app, string message)
        {
            try
            {
                FileLog0.LogTextFile("UmsHub.PASS2Message - begin", $"ConnectionId={Context.ConnectionId} srcName={srcName} env={env} group={app} message={message}", null, null);

                app = app.ToUpper();

                if (app.Equals("F") || app.Equals("I"))
                {
                    UserInfo[] uinfo = umsHelper.usp_getPass2UserIdListByApp(app);
                    if (uinfo != null)
                    {
                        foreach (UserInfo ui in uinfo)
                        {
                            doPass2MessageCore(ui, srcName, env, message);
                        }
                    }
                }
                else
                {
                    FileLog0.DebugTxt("UmsHub.PASS2Message", "SendAll-begin", null, null);
                    PASS2Message(srcName, env, "F", message);
                    PASS2Message(srcName, env, "I", message);
                    FileLog0.DebugTxt("UmsHub.PASS2Message", "SendAll-end", null, null);
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                FileLog0.LogTextFile("UmsHub.PASS2Message_Catch", "*INFO*", ex.Message, null);
            }
            catch (Exception ex)
            {
                FileLog0.LogTextFileEx("UmsHub.PASS2Message_Catch", FileLog0.MakeExceptionMessages(ex), null, null);
            }
        }


        private void doPass2MessageCore(UserInfo ui, string srcName, string env, string message)
        {
            try
            {
                if (ui.EnvName == null)
                {
                    FileLog0.LogTextFile("UmsHub.doPass2MessageCore", "ui.EnvName==null", null, null);
                }
                // csak a sajat kornyezetet vegye figyelembe
                if ((ui.EnvName != null) && ui.EnvName.Equals(env))
                {                    // ellenorzi, sajat gepen van-e a connection
                    if (Environment.MachineName.Equals(ui.MachineName))
                    {
                        //Clients.Client(uinfo.Id).Send(srcName, env, message)
                        ((Hub)this).Clients.Client(ui.Id).PASS2MessageCL(srcName, env, message);
                        umsHelper.setMessage(ui.Id, srcName, message);
                        FileLog0.LogTextFile("UmsHub.doPass2MessageCore", $"Sent to ConnectionId={ui.Id} == {ui.Pass2Id}", null, null);
                    }
                    else
                    {
                        FileLog0.LogTextFile("UmsHub.doPass2MessageCore", "sendOtherMachine - before", null, null);
                        Passe2MessageOtherMachine(ui.MachineName, srcName, env, ui.Pass2Id, message);
                        FileLog0.LogTextFile("UmsHub.doPass2MessageCore", "sendOtherMachine - after", null, null);
                    }
                }
            }
            catch (Exception ex1)
            {
                FileLog0.LogTextFile("UmsHub.PASS2Message_ex1", $"Sikertelen küldés: ConnectionId={ui.Id} == {ui.Pass2Id}", FileLog0.MakeExceptionMessages(ex1), null);
            }
        }

        private void Passe2MessageOtherMachine(string machineName, string srcName, string env, string pass2Id, string message)
        {
            var section = ConfigurationManager.GetSection("UMSservers") as NameValueCollection;
            var value = section[machineName];
            if (value != null)
            {
                Uri uri = new Uri(value);
                var address = new EndpointAddress(uri);
                using (SendUMSMessageClient svc = new SendUMSMessageClient(ClientServiceName, address))
                {
                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                    svc.PASS2MessageCL(srcName, env, pass2Id, message);
                }
            }
            else
            {
                FileLog0.LogTextFileEx("UmsHub.Passe2MessageOtherMachine", $"Ismeretlen gépnév={machineName}", null, null);
            }
        }
        public void PASS2MessageCL(string srcName, string env, string pass2ID, string message)
        {
            try
            {
                FileLog0.LogTextFile("UmsHub.PASS2MessageCL - begin", $"ConnectionId={Context.ConnectionId} From={srcName} Env={env} pass2ID={pass2ID} Msg={message}", null, null);

                // ha egynel tobb helyre kell kuldeni pl:tobb nyitott IUR ablak
                UserInfo[] uinfo = umsHelper.getAllUserInfoByPass2Id(pass2ID);

                foreach (UserInfo ui in uinfo)
                {
                    try
                    {
                        // ellenorzi, sajat gepen van-e a connection
                        if (Environment.MachineName.Equals(ui.MachineName))
                        {
                            ((Hub)this).Clients.Client(ui.Id).PASS2MessageCL(srcName, env, message);
                            umsHelper.setMessage(ui.Id, srcName, message);
                            FileLog0.LogTextFile("UmsHub.PASS2MessageCL", $"Sent to ConnectionId={ui.Id} == {ui.Pass2Id}", null, null);
                        }
                        else
                        {
                            FileLog0.LogTextFile("UmsHub.PASS2MessageCL", $"!!! --- EZ LEHETETLEN --- !!! ({ui.Pass2Id})", null, null);
                        }
                    }
                    catch (Exception ex1)
                    {
                        FileLog0.LogTextFile("UmsHub.PASS2MessageCL_ex1", $"Sikertelen küldés: ConnectionId={ui.Id} == {ui.Pass2Id}", FileLog0.MakeExceptionMessages(ex1), null);
                    }
                }
                FileLog0.LogTextFile("UmsHub.PASS2MessageCL - end", null, null, null);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                FileLog0.LogTextFile("UmsHub.PASS2MessageCL_Catch", "*INFO*", ex.Message, null);
            }
            catch (Exception ex)
            {
                FileLog0.LogTextFileEx("UmsHub.PASS2MessageCL_Catch", FileLog0.MakeExceptionMessages(ex), null, null);
            }
        }
        #endregion PASS2message



        #region For00FormOpen
        // egyelore nincs szukseg a gyorsan egymas utan jovo hivasok kiszuresere
        //// Szálbiztos tároló a kliensek utolsó üzenetének idejéről
        //private static readonly ConcurrentDictionary<string, DateTime> _lastMessageTimes = new ConcurrentDictionary<string, DateTime>();


        public void For00FormOpen(string targetUser, string env, bool modal, string formName, string extraJsonData)
        {
            // egyelore nincs szukseg a gyorsan egymas utan jovo hivasok kiszuresere
            //try
            //{
            //    var connectionId = Context.ConnectionId;
            //    var now = DateTime.UtcNow;

            //    // Ellenőrizzük, mikor volt az utolsó hívás
            //    if (_lastMessageTimes.TryGetValue(connectionId, out var lastTime))
            //    {
            //        // Ha kevesebb mint 100ms telt el, az gyanús (bombázás)
            //        if ((now - lastTime).TotalMilliseconds < 100)
            //        {
            //            FileLog0.LogTextFile("UmsHub.For00FormOpen", $"Bombázás gyanú: {connectionId}", null, null);
            //            // Csendben eldobjuk a kérést (hogy ne terheljük a szervert válasszal)
            //            return;
            //        }
            //    }

            //    // Időbélyeg frissítése
            //    _lastMessageTimes[connectionId] = now;
            //}
            //catch (Exception ex)
            //{
            //    FileLog0.LogTextFileEx("UmsHub.For00FormOpen_UpdateLastMessageTime_Catch", FileLog0.MakeExceptionMessages(ex), null, null);
            //}

            try
            {
                FileLog0.LogTextFile("UmsHub.For00FormOpen - begin", $"ConnectionId={Context.ConnectionId} targetUser={targetUser} env={env} modal={modal} formName={formName} extraJsonData={extraJsonData}", null, null);

                formName = formName.ToUpper();

                UserInfo[] uinfo = umsHelper.getAllUserInfoByPass2Id(targetUser);

                foreach (UserInfo ui in uinfo)
                {
                    string current_ui_id = ui.Id;
                    if ((ui.MachineName != null) && !ui.MACAddRess.Equals("IUR"))
                    {
                        try
                        {
                            // csak a sajat kornyezetet vegye figyelembe
                            if ((ui.EnvName != null) && ui.EnvName.Equals(env))
                            {
                                // ellenorzi, sajat gepen van-e a connection
                                if (Environment.MachineName.Equals(ui.MachineName))
                                {
                                    FileLog0.LogTextFile("UmsHub.For00FormOpen", $"Ezen a gepne kell lennie: {Environment.MachineName}", null, null);
                                    dynamic clientProxy = Clients.Client(current_ui_id);
                                    if (clientProxy == null)
                                    {
                                        FileLog0.LogTextFile("UmsHub.For00FormOpen", $"clientProxy==null", null, null);
                                        continue;
                                    }
                                    clientProxy.For00FormOpen(targetUser, env, modal, formName, extraJsonData);
                                    umsHelper.setMessage(current_ui_id, targetUser, extraJsonData);
                                    FileLog0.LogTextFile("UmsHub.For00FormOpen", $"Sent to ConnectionId={current_ui_id} == {ui.Pass2Id},  formName={formName}", null, null);
                                }
                                else
                                {
                                    For00FormOpen2OtherMachine(ui.MachineName, targetUser, env, modal, formName, extraJsonData);
                                }
                            }
                        }
                        catch (Exception ex1)
                        {
                            FileLog0.LogTextFile("UmsHub.For00FormOpen_ex1", $"Sikertelen küldés: ConnectionId={current_ui_id} == {ui.Pass2Id}", FileLog0.MakeExceptionMessages(ex1), null);
                        }
                    }
                    else
                    {
                        FileLog0.LogTextFile("UmsHub.For00FormOpen", $"MachineName is null, or MACAddress=IUR.", null, null);
                    }
                }
                FileLog0.LogTextFile("UmsHub.For00FormOpen - end", null, null, null);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                FileLog0.LogTextFile("UmsHub.PASS2Message_Catch", "*INFO*", ex.Message, null);
            }
            catch (Exception ex)
            {
                FileLog0.LogTextFileEx("UmsHub.PASS2Message_Catch", FileLog0.MakeExceptionMessages(ex), null, null);
            }
            finally
            {
                FileLog0.LogTextFile("UmsHub.For00FormOpen - end", "", null, null);
            }
        }
        private void For00FormOpen2OtherMachine(string machineName, string targetUser, string env, bool modal, string formName, string extraJsonData)
        {
            try
            {
                FileLog0.LogTextFile("UmsHub.For00FormOpen2OtherMachine - begin", "", null, null);
                var section = ConfigurationManager.GetSection("UMSservers") as NameValueCollection;
                var value = section[machineName];
                if (value != null)
                {
                    FileLog0.LogTextFile("UmsHub.For00FormOpen2OtherMachine", $"Ide fog menni:{value}", null, null);

                    Uri uri = new Uri(value);
                    var address = new EndpointAddress(uri);
                    using (SendUMSMessageClient svc = new SendUMSMessageClient(ClientServiceName, address))
                    {
                        FileLog0.LogTextFile("UmsHub.For00FormOpen2OtherMachine", $"Masik gep hivasa(WCF) - before:{address}", null, null);
                        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                        svc.For00FormOpenCL(targetUser, env, modal, formName, extraJsonData);
                        FileLog0.LogTextFile("UmsHub.For00FormOpen2OtherMachine", $"Masik gep hivasa(WCF) - after", null, null);
                    }
                }
                else
                {
                    FileLog0.LogTextFileEx("UmsHub.For00FormOpen2OtherMachine", $"Ismeretlen gépnév={machineName}", null, null);
                }
            }
            finally
            {
                FileLog0.LogTextFile("UmsHub.For00FormOpen2OtherMachine - end", "", null, null);
            }
        }



        public void For00FormOpenCL(string targetUser, string env, bool modal, string formName, string extraJsonData)
        {
            try
            {
                FileLog0.LogTextFile("UmsHub.For00FormOpenCL - begin", $"ConnectionId={Context.ConnectionId} targetUser={targetUser} env={env} modal={modal} formName={formName} extraJsonData={extraJsonData}", null, null);

                formName = formName.ToUpper();

                UserInfo[] uinfo = umsHelper.getAllUserInfoByPass2Id(targetUser);
                foreach (UserInfo ui in uinfo)
                {
                    string current_ui_id = ui.Id;
                    if ((ui.MachineName != null) && !ui.MACAddRess.Equals("IUR"))
                    {
                        try
                        {
                            // ellenorzi, sajat gepen van-e a connection
                            if (Environment.MachineName.Equals(ui.MachineName))
                            {
                                FileLog0.LogTextFile("UmsHub.For00FormOpenCL", $"Ezen a gepne kell lennie: {Environment.MachineName}", null, null);
                                dynamic clientProxy = Clients.Client(current_ui_id);
                                if (clientProxy == null)
                                {
                                    FileLog0.LogTextFile("UmsHub.For00FormOpenCL", $"clientProxy==null", null, null);
                                }
                                clientProxy.For00FormOpen(targetUser, env, modal, formName, extraJsonData);
                                umsHelper.setMessage(current_ui_id, targetUser, extraJsonData);
                                FileLog0.LogTextFile("UmsHub.For00FormOpenCL", $"Sent to ConnectionId={current_ui_id} == {ui.Pass2Id},  formName={formName}", null, null);
                            }
                            else
                            {
                                FileLog0.LogTextFile("UmsHub.For00FormOpenCL", $"Cél gép neve: {ui.MachineName} nem ez a gép", null, null);
                            }
                        }
                        catch (Exception ex1)
                        {
                            FileLog0.LogTextFile("UmsHub.For00FormOpenCL_ex1", $"Sikertelen küldés: ConnectionId={current_ui_id} == {ui.Pass2Id}", FileLog0.MakeExceptionMessages(ex1), null);
                        }
                    }
                    else
                    {
                        FileLog0.LogTextFile("UmsHub.For00FormOpenCL", $"MachineName is null, or MACAddress=IUR.", null, null);
                    }
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                FileLog0.LogTextFile("UmsHub.For00FormOpenCL_Catch", "*INFO*", ex.Message, null);
            }
            catch (Exception ex)
            {
                FileLog0.LogTextFileEx("UmsHub.For00FormOpenCL_Catch", FileLog0.MakeExceptionMessages(ex), null, null);
            }
            finally
            {
                FileLog0.LogTextFile("UmsHub.For00FormOpenCL - end", "", null, null);
            }
        }

        #endregion For00FormOpen




        #endregion Funkcionalis fuggvenyek




        public void SetClient(string pass2UserID, string env, string macAddress)
        {
            try
            {
                // sajat atkuldott parameterek kiszedese
                FileLog0.LogTextFile("UmsHub.SetClient - begin",
                                     $"ConnectionId={Context.ConnectionId} pass2UserID={pass2UserID} Env={env} MachineName={Environment.MachineName} MacAddress={macAddress}",
                                      null, null);

                umsHelper.setPass2Info(Context.ConnectionId, pass2UserID.ToUpperInvariant(), env, Environment.MachineName, macAddress);
                FileLog0.LogTextFile("UmsHub.SetClient - end", null, null, null);
            }
            catch (Exception ex)
            {
                FileLog0.LogTextFileEx("UmsHub.SetClient_Catch", FileLog0.MakeExceptionMessages(ex), null, null);
            }
        }




        ///
        /// register online user
        ///
        ///
        public override System.Threading.Tasks.Task OnConnected()
        {

            try
            {
                // lassuk mit lehet kinyerni
                string connId = Context.ConnectionId;
                string userName = Context.User.Identity.Name;
                string authenticationType = Context.User.Identity.AuthenticationType;
                bool isAuthenticated = Context.User.Identity.IsAuthenticated;
                string tostr = Context.User.Identity.ToString();
                // ----


                ((Hub)this).Clients.Caller.UserId = Context.User.Identity.Name;
                ((Hub)this).Clients.Caller.initialized();

                //..string pass2UserID = getPass2UserID()
                // parameters in Context
                //..FileLog0.LogTextFile("UmsHub.OnConnected", string.Format("ConnectionId={0} Name={1} pass2UserID={2}", Context.ConnectionId, Context.User.Identity.Name, pass2UserID), null, null)


                // tobb szalon, tobbszor is mehet ua ConnectionId-val
                umsHelper.addUserInfo(Context.ConnectionId, null, null, null);
            }
            catch (Exception ex)
            {
                FileLog0.LogTextFileEx("UmsHub.OnConnected_Catch", $"sikertelen umsHelper.addUserInfo({Context.ConnectionId}, null, null)", FileLog0.MakeExceptionMessages(ex), null);
            }

            return base.OnConnected();
        }

        ///
        /// unregister disconected user
        ///
        ///
        // latszolag nem jon be, de kell varni kb 1 percet!!
        // a HUbConnection.Stop()-ra azonnal bejon  : stopCalled==true   ertekkel
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            //..string pass2UserID = getPass2UserID()

            //string ws1 = Context.User.Identity.Name;      // nincs erteke
            //string ws2 = Clients.Caller.UserId;           // nincs erteke


            //..FileLog0.LogTextFile("UmsHub.OnDisconnected", string.Format("ConnectionId={0} pass2UserID={1}", Context.ConnectionId, pass2UserID), null, null)
            try
            {
                umsHelper.delUserInfo(Context.ConnectionId);
            }
            catch (Exception ex)
            {
                FileLog0.LogTextFileEx("UmsHub.OnDisconnected_Catch", $"sikertelen umsHelper.delUserInfo({Context.ConnectionId}, null, null)", FileLog0.MakeExceptionMessages(ex), null);
            }

            return base.OnDisconnected(stopCalled);
        }


        public override System.Threading.Tasks.Task OnReconnected()
        {
            // Ekkor (Onreconnected) a ConnactionID  MEGMARADT!!!
            // minden marad a regiben, ezert csak logolom
            string pass2UserID=Context.QueryString["Pass2UserID"];
            string macAddress=Context.QueryString["MacAddress"];
            FileLog0.LogTextFileNL("UmsHub.OnReconnected", $"ConnectionId={Context.ConnectionId} Name={Context.User.Identity.Name} pass2UserID={pass2UserID} macAddress={macAddress}", null, null);
            return base.OnReconnected();
        }
    }
}