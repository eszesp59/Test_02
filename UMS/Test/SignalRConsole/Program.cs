using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace SignalRConsole
{
    static class Program
    {
        [SuppressMessage("Sonar Vulnerability", "S2228")]
        [SuppressMessage("Sonar Security",      "S1313")]
        static void Main()
        {

            // sajat parameterek kuldese
            Dictionary<string, string> connParams = new Dictionary<string, string>
            {
                { "DomainName", "MBP991699520" },
                { "UserName", "HERNADIL" },
                { "IpAddr", "10.173.2.142" },

                { "par1", "ALMA1" },
                { "par2", "ALMA2" }
            };

            // az SQ arrol ismeri meg az utasitast, hogy ";" van a vegen
            // var hubConnection = new HubConnection("http://ceszesp.mavinformatika.hu/EP_SignalR",connParams)
            // var hubConnection = new HubConnection("http://tadwebifpdv01srv-2/EP_SignalR",connParams)
            // var hubConnection = new HubConnection("http://ceszesp.tad.mavinformatika.hu/WebApplication_server/",connParams)
            string ws = "http://localhost:6801";
            var hubConnection = new HubConnection(ws, connParams)
            {
                Credentials = CredentialCache.DefaultCredentials
            };

            var ums = hubConnection.CreateHubProxy("umsHub");
            ums.On<string, string>("ShowCommandMessage", (name, message) => 
            {
                Console.Write(name + ": ");
                Console.WriteLine(message);
            });

            ums.On<string, string>("broadcastMessage", (name, message) => 
            {
                Console.Write(name + ": ");
                Console.WriteLine(message);
            });

            hubConnection.Start().Wait();
            ums.Invoke("Notify", "Console app", hubConnection.ConnectionId);
            string msg;

            while ((msg = Console.ReadLine()) != null)
            {
                ums.Invoke("Send", "Console app", msg).Wait();
            }
        }
    }
}
