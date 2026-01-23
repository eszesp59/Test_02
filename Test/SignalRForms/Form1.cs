using Microsoft.AspNet.SignalR.Client;
using SignalRForms.SendUMSMessage_NS;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Management;
using System.Media;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SignalRForms
{
    public partial class Form1 : Form
    {
        private Queue<string> EventQueue = new Queue<string>(32);
        private const string SEPARATOR_STR = "---";
        private bool Loaded = false;


        private IHubProxy umsHub = null;
        private HubConnection hubConnection = null;
        private bool UmsConnected = false;

        private int sound;
        private int errNUM = 0;

        public Form1()
        {
            InitializeComponent();

            // TLS1.2 beallitasa
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            // fogadjon el minden certificate-t
            ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;
        }

        private void ClearList_btt_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void logException(Exception ex)
        {
            addQueue("===========-------------------");
            //addQueue("UMSConnect_catch", MakeExceptionMessages(ex));
            string nl = Environment.NewLine;
            string[] ex_lines = MakeExceptionMessages(ex).Split('|');
            foreach (string line in ex_lines)
            {
                addQueue(line);
            }
            addQueue("===========-------------------");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            MacAddress_txt.Text = GetMACAddress("-");
            radioButton1.Checked = true;

            Fill_ServerNames();

            // UmsConnected = UMSConnect(); itt lenne,  de csak a combo valasztasban johet
            Loaded = true;
        }

        private void Fill_ServerNames()
        {
            ServerNameItemList names = new ServerNameItemList();
            NameValueCollection settingCollection = (NameValueCollection)ConfigurationManager.GetSection("SERVER_NAMES");
            foreach(string key in settingCollection.AllKeys)
            {
                names.Add(new ServerNameItem(key, settingCollection[key]));
            }
            serverNameItemListBindingSource.DataSource = names;
            ServerNames.DataSource = serverNameItemListBindingSource;
            ServerNames.Text = "";
        }

        private void ServerNames_SelectedValueChanged(object sender, EventArgs e)
        {
            if ((Loaded) && (ServerNames.SelectedIndex>-1))
            {
                ServerLabel.Text = MakeHostName();
                SvcName.Text = MakeSvcString();
                errNUM = 0;
                UmsConnected = UMSConnect();
            }
        }

        private string MakeHostName()
        {
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            string ws = appSettings["UmsHost"];
            ws = ws.Replace("__SERVER_NAME__", ServerNames.SelectedValue.ToString());
            if (ws.Contains("localhost"))
            {
                ws += "Host";
            }
            return ws;
        }

        private string MakeSvcString()
        {
            string ws = MakeHostName();
            ws += "/SendUMSMessage.svc";
            if (ws.Contains("localhost"))
            {
                ws = "http://localhost/SendUMSMessage/SendUMSMessage.svc";
            }
            return ws;
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        static public string GetCurrentMethod()
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }


        private string ActualConnectionIDStr()
        {
            string rc = "No Connection";
            if (hubConnection != null)
            {
                rc = $"ConnectionID={hubConnection.ConnectionId} State={hubConnection.State}";
            }
            return rc;
        }



        private bool UMSConnect()
        {
            UmsConnected = false;
            if (errNUM < 3)
            {
                try
                {
                    string conStr = ServerLabel.Text;
                    addQueue($"conStr={conStr}");

                    // sajat parameterek kuldese
                    Dictionary<string, string> connParams = new Dictionary<string, string>();
                    connParams.Add("Pass2UserID", "");
                    connParams.Add("EnvironmentName", "");
                    connParams.Add("MacAddress", "");
                    //..connParams.Add("Bearer", token)

                    hubConnection = new HubConnection(conStr, connParams);

                    hubConnection.Credentials = CredentialCache.DefaultCredentials;
                    // hubConnection.Headers.Add("Authorization: Bearer", token)


                    #region log config
                    ////hubConnection.TraceWriter      =             System.Console.Out;
                    hubConnection.Closed += () => addQueue("hubConnection.Closed");
                    hubConnection.ConnectionSlow += () => addQueue("hubConnection.ConnectionSlow");
                    hubConnection.Error += (error) => addQueue("hubConnection.Error", error.GetType().ToString(), error.Message);
                    hubConnection.Reconnected += (UMS_Reconnected);
                    hubConnection.Reconnecting += () => addQueue("hubConnection.Reconnecting");
                    hubConnection.StateChanged += (change) => addQueue("hubConnection.StateChanged", change.OldState.ToString(), change.NewState.ToString());
                    #endregion log config



                    //  Start() elott kell a proxy-kat letrehozni
                    umsHub = hubConnection.CreateHubProxy("UmsHub");
                    hubConnection.Start().Wait();


                    #region UmsHub
                    umsHub.On<string, string, string>("Hello", (srcName, env, message) => { HelloAction(srcName, env, message); });
                    umsHub.On<string, string, string>("Send", (srcName, env, message) => { SendAction(srcName, env, message); });
                    umsHub.On<string, string, string, string>("SendToForm", (srcName, env, formTypeID, privateMessage) => { SendToFormAction(srcName, env, formTypeID, privateMessage); });
                    umsHub.On<string, string, string>("SendAll", (srcName, env, message) => { SendAllAction(srcName, env, message); });
                    umsHub.On<string, string, string>("SendList", (srcName, env, message) => { SendListAction(srcName, env, message); });
                    umsHub.On<string, string, string>("SetClient", (pass2UserID, env, macAddress) => { SetClientAction(pass2UserID, env, macAddress); });
                    umsHub.On<string, int, string>("KeszreJelentes", (macAddress, sound, message) => { KeszreJelentesAction(macAddress, sound, message); });
                    umsHub.On<string, int, string>("Vontat", (macAddress, sound, message) => { VontatAction(macAddress, sound, message); });


                    umsHub.On<string, string, string>("PASS2MessageCL", (srcName, env, message) => { PASS2MessageCLAction(srcName, env, message); });
                    umsHub.On<string, string, bool, string, string>("For00FormOpen", (targetUser, env, modal, formName, extraJsonData) => { For00FormOpenAction(targetUser, env, modal, formName, extraJsonData); });

                    #endregion UmsHub
                }
                catch (Exception ex)
                {
                    errNUM++;
                    logException(ex);
                }
            }

            return UmsConnected;
        }



        protected void UMS_Reconnected()
        {
            // Ekkor (Onreconnected) a ConnactionID  MEGMARADT!!!
            // minden marad a regiben, ezert csak logolom
            addQueue("hubConnection.Reconnected");
            //if (umsHub != null)
            //{
            //    umsHub.Invoke("SetClient", OwnPass2ID_txt.Text, Environment_txt.Text, GetMACAddress("-"));
            //}
        }



        #region call hub functions
        private void Hello_btt_Click(object sender, EventArgs e)
        {
            try
            {
                if (umsHub != null)
                {
                    addListbox(GetCurrentMethod(),ActualConnectionIDStr());
                    umsHub.Invoke("Hello", OwnPass2ID_txt.Text, Environment_txt.Text);
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void Send_btt_Click(object sender, EventArgs e)
        {
            if (umsHub != null)
            {
                addListbox(GetCurrentMethod(), ActualConnectionIDStr());
                umsHub.Invoke("Send", OwnPass2ID_txt.Text, Environment_txt.Text, Addresss_txt.Text, Message_txt.Text);
            }
        }

        private void SendToForm_btt_Click(object sender, EventArgs e)
        {
            if (umsHub != null)
            {
                addListbox(GetCurrentMethod(), ActualConnectionIDStr());
                umsHub.Invoke("SendToForm", OwnPass2ID_txt.Text, Environment_txt.Text, Addresss_txt.Text, FormTypeID_txt.Text, Message_txt.Text, Private_txt.Text);
            }
        }

        private void For00FormOpen_btt_Click(object sender, EventArgs e)
        {
            if (umsHub != null)
            {
                addListbox(GetCurrentMethod(), ActualConnectionIDStr());
                umsHub.Invoke("For00FormOpen", Addresss_txt.Text, Environment_txt.Text, Modal_chk.Checked, FormName_txt.Text, Message_txt.Text);
            }
        }


        private void SendAll_btt_Click(object sender, EventArgs e)
        {
            if (umsHub != null)
            {
                addListbox(GetCurrentMethod(), ActualConnectionIDStr());
                umsHub.Invoke("SendAll", OwnPass2ID_txt.Text, Environment_txt.Text, Message_txt.Text);
            }
        }

        private void SendList_btt_Click(object sender, EventArgs e)
        {
            if (umsHub != null)
            {
                addListbox(GetCurrentMethod(), ActualConnectionIDStr());
                List<string> lst = new List<string>();
                string[] users = Regex.Split(Names_txt.Text, ",", RegexOptions.None ,TimeSpan.FromMilliseconds(100));
                foreach (string username in users)
                {
                    lst.Add(username);
                }

                umsHub.Invoke("SendList", OwnPass2ID_txt.Text, Environment_txt.Text, lst, Message_txt.Text);
            }
        }

        private void SetClient_btt_Click(object sender, EventArgs e)
        {
            if (umsHub != null)
            {
                addListbox(GetCurrentMethod(), ActualConnectionIDStr());
                umsHub.Invoke("SetClient", OwnPass2ID_txt.Text, Environment_txt.Text, GetMACAddress("-"));
            }
        }

        private void Pass2Message_Btt_Click(object sender, EventArgs e)
        {
            if (umsHub != null)
            {
                addListbox(GetCurrentMethod(), ActualConnectionIDStr());
                string group = "";
                if (IUR_Rbtt.Checked) group = "I";
                else if (FOR00_Rbtt.Checked) group = "F";
                else if (Mindenki_Rbtt.Checked) group = "A";
                if (!string.IsNullOrEmpty(group))
                {
                    umsHub.Invoke("PASS2Message", OwnPass2ID_txt.Text, Environment_txt.Text, group, Message_txt.Text);
                }
                else
                {
                    addQueue("PASS2Message: nem volt érvényes választás!");
                }
            }
        }
        #endregion call hub functions



        #region Actions - HUB functions
        public void HelloAction(string srcName, string env, string message)
        {
            addListbox(srcName, env, message);
        }

        public void SendAction(string srcName, string env, string message)
        {
            addListbox($"SendAction source={srcName} env={env} message={message}");
        }

        public void SendToFormAction(string srcName, string env, string formTypeID, string privateMessage)
        {
            addListbox($"SendToFormAction source={srcName} env={env} formTypeID={formTypeID} privateMessage={privateMessage}");
        }

        public void SendAllAction(string srcName, string env, string message)
        {
            addListbox($"SendAllAction source={srcName} env={env} message={message}");
        }

        public void SendListAction(string srcName, string env, string message)
        {
            addListbox($"SendListAction source={srcName} env={env} message={message}");
        }

        public void SetClientAction(string pass2UserID, string env, string macAddress)
        {
            addListbox($"SetClientAction pass2UserID={pass2UserID} env={env} macAddress={macAddress}");
        }

        public void KeszreJelentesAction(string macAddress, int sound, string messages)
        {
            addListbox($"SetClientAction macAddress={macAddress} sound={sound} messages={messages}");
        }

        public void VontatAction(string macAddress, int sound, string messages)
        {
            addListbox($"VontatAction macAddress={macAddress} sound={sound} messages={messages}");
        }

        public void PASS2MessageCLAction(string srcname, string env, string message)
        {
            addListbox($"PASS2MessageAction source={srcname} env={env} messages={message}");
        }

        public void For00FormOpenAction(string targetUser, string env, bool modal, string formName, string extraJsonData)
        {
            addListbox($"For00FormOpenAction targetUser={targetUser} env={env} formName={formName} messages={extraJsonData}");
        }
        #endregion Actions - HUB functions



        #region Add Queue
        private void addQueue(string txt)
        {
            EventQueue.Enqueue($"{txt}");
        }

        private void addQueue(string txt0, string txt1)
        {
            EventQueue.Enqueue($"{txt0} - {txt1}");
        }

        private void addQueue(string txt0, string txt1, string txt2)
        {
            EventQueue.Enqueue($"{txt0} - {txt1} => {txt2}");
        }

        #endregion Add Queue


        #region display message
        delegate void AddListbox();
        private void addListbox()
        {
            if (listBox1.InvokeRequired)
            {
                AddListbox d = (addListbox);
                listBox1.Invoke(d, new object[] { SEPARATOR_STR });
            }
            else
            {
                listBox1.Items.Add(SEPARATOR_STR);
            }
        }

        private void addListbox(string txt)
        {
            this.Invoke((MethodInvoker)delegate
            {
                listBox1.Items.Add(txt);
            });
        }

        private void addListbox(string txt, string txt0)
        {
            this.Invoke((MethodInvoker)delegate
            {
                listBox1.Items.Add($"{txt}, {txt0}");
            });
        }

        private void addListbox(string txt, string txt0, string txt1)
        {
            this.Invoke((MethodInvoker)delegate
            {
                listBox1.Items.Add($"{txt}, {txt0}, {txt1}");
            });
        }

        private void addListbox(string txt, string txt0, string txt1, string txt2)
        {
            this.Invoke((MethodInvoker)delegate
            {
                listBox1.Items.Add($"{txt}, {txt0}, {txt1}, {txt2}");
            });
        }

        private void addListbox(string txt, string txt0, string txt1, string txt2, string txt3)
        {
            this.Invoke((MethodInvoker)delegate
            {
                listBox1.Items.Add($"{txt}, {txt0}, {txt1}, {txt2}, {txt3}");
            });
        }
        #endregion display message



        #region timer
        [SuppressMessage("Move this Dispose call into this class' own Dispose method.", "S2952")]
        private void timer1_Tick__dispose()
        {
            try
            {
                hubConnection.Dispose();
                hubConnection = null;

                UmsConnected = UMSConnect();

                if (umsHub != null)
                {
                    umsHub.Invoke("SetClient", OwnPass2ID_txt.Text, Environment_txt.Text, GetMACAddress("-"));
                }
            }
            catch (Exception ex)
            {
                listBox1.Items.Add(ex.Message);
                if (ex.InnerException != null)
                {
                    listBox1.Items.Add(ex.InnerException.Message);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                timer1.Enabled = false;
                while (EventQueue.Count > 0)
                {
                    string ws = EventQueue.Dequeue();
                    listBox1.Items.Add(ws);
                    textBox1.Text += ws;
                }


                if ((hubConnection != null) && (hubConnection.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Disconnected))
                {
                    timer1_Tick__dispose();
                }
            }
            finally
            {
                timer1.Enabled = true;
            }
        }
        #endregion timer


        private SendUMSMessageClient CreateClient()
        {
            SendUMSMessageClient rc = null;
            if (SvcName.Text.Substring(0,5).Equals("https"))
            {
                Uri uri = new Uri(SvcName.Text);
                var address = new EndpointAddress(uri);
                rc = new SendUMSMessageClient("Secure", address);
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            }
            else
            {
                rc = new SendUMSMessageClient("", SvcName.Text);
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            }

            return rc;
        }

        private void WCF_Hello_btt_Click(object sender, EventArgs e)
        {
            addListbox(GetCurrentMethod());
            using (SendUMSMessageClient svc = CreateClient())
            {
                svc.Hello(OwnPass2ID_txt.Text, Environment_txt.Text);
            }
        }

        private void WCF_Send_btt_Click(object sender, EventArgs e)
        {
            addListbox(GetCurrentMethod());
            using (SendUMSMessageClient svc = CreateClient())
            {
                svc.Send(OwnPass2ID_txt.Text, Environment_txt.Text, Addresss_txt.Text, WCFMessage(Message_txt.Text));
            }
        }

        private void WCF_SendToForm_btt_Click(object sender, EventArgs e)
        {
            addListbox(GetCurrentMethod());
            using (SendUMSMessageClient svc = CreateClient())
            {
                svc.SendToForm(OwnPass2ID_txt.Text, Environment_txt.Text, Addresss_txt.Text, FormTypeID_txt.Text, WCFMessage(Message_txt.Text), Private_txt.Text);
            }
        }

        private void WCF_For00FormOpen_btt_Click(object sender, EventArgs e)
        {
            addListbox(GetCurrentMethod());
            using (SendUMSMessageClient svc = CreateClient())
            {
                svc.For00FormOpen(Addresss_txt.Text, Environment_txt.Text, Modal_chk.Checked, FormName_txt.Text, WCFMessage(Message_txt.Text));
            }
        }

        private void WCF_SendAll_btt_Click(object sender, EventArgs e)
        {
            addListbox(GetCurrentMethod());
            using (SendUMSMessageClient svc = CreateClient())
            {
                svc.SendAll(OwnPass2ID_txt.Text, Environment_txt.Text, WCFMessage(Message_txt.Text));
            }
        }

        private void WCF_SendList_btt_Click(object sender, EventArgs e)
        {
            addListbox(GetCurrentMethod());
            using (SendUMSMessageClient svc = CreateClient())
            {
                string[] users = Regex.Split(Names_txt.Text, ",", RegexOptions.None, TimeSpan.FromMilliseconds(100));
                svc.SendList(OwnPass2ID_txt.Text, Environment_txt.Text, users, WCFMessage(Message_txt.Text));
            }
        }


        private string WCFMessage(string msg)
        {
            return string.Format("{0}--WCF", msg);
        }



        private void button1_Click(object sender, EventArgs e)
        {
            sendOtherMachine("PESZESP", "DEV", "444", "ALMA - msg");
        }
        private void sendOtherMachine(string srcName, string env, string name, string message)
        {
            using (SendUMSMessageClient svc = CreateClient())
            {
                svc.Send(srcName, env, name, message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            addQueue($"MAC address={GetMACAddress("-")}");
        }



        // =============  c:\Work\IFP\PTB-Main-Dev\Src\ARCH\Forms\CommonLib\MachineInfo.cs -bol idemasolva !!!!!!!
        /// <summary>
        /// Returns MAC Address from first Network Card in Computer
        /// </summary>
        /// <returns>[string] MAC Address</returns>
        public static string GetMACAddress(string sepChar)
        {
            string MACAddress = String.Empty;
            using (ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
            {
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementBaseObject mo in moc)
                {
                    // only return MAC Address from first card
                    if ((string.IsNullOrEmpty(MACAddress)) && ((bool)mo["IPEnabled"]))
                    {
                        MACAddress = mo["MacAddress"].ToString();
                    }
                    mo.Dispose();
                }
                //..if (!string.IsNullOrEmpty(sepChar))
                if ((!string.IsNullOrEmpty(MACAddress)) && (!string.IsNullOrEmpty(sepChar)))
                {
                    MACAddress = MACAddress.Replace(":", sepChar);
                }
            }
            return MACAddress;
        }


        public  string MakeExceptionMessages(Exception ex)
        {
            StringBuilder rc = new StringBuilder();
            int i = 0;
            while (ex != null)
            {
                i++;
                if (i != 1)
                {
                    rc.Append("|");
                }
                rc.Append($"|   {i:00} {ex.GetType().Name} {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                ex = ex.InnerException;
            }
            return rc.ToString();
        }



        private void KeszreJelentes_Btt_Click(object sender, EventArgs e)
        {
            addListbox(GetCurrentMethod());
            using (SendUMSMessageClient svc = CreateClient())
            {
                svc.KeszreJelentes(MacAddress_txt.Text, sound, WCFMessage(Message_txt.Text));
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = (sender as RadioButton);
            if (rb.Checked)
            {
                sound = Convert.ToInt32(rb.Text.Substring(0, 1));
                switch (sound)
                {
                    case 0:   // none
                    {
                        // nem kell semmit sem csinalni - csend van
                    }
                    break;
                    case 1:   // beep
                    {
                        SystemSounds.Beep.Play();
                    }
                    break;
                    case 2:
                    {
                        SoundPlayer snd = new SoundPlayer(Properties.Resources.Windows_Background);
                        snd.Play();
                    }
                    break;
                    case 3:
                    {
                        SoundPlayer snd = new SoundPlayer(Properties.Resources.Windows_Message_Nudge);
                        snd.Play();
                    }
                    break;
                    case 4:
                    {
                        SoundPlayer snd = new SoundPlayer(Properties.Resources.Windows_Notify_Messaging);
                        snd.Play();
                    }
                    break;
                    case 5:
                    {
                        SoundPlayer snd = new SoundPlayer(Properties.Resources.Windows_Notify_System_Generic);
                        snd.Play();
                    }
                    break;
                    default: // none
                    {
                        // nem kell semmit sem csinalni - csend van
                    }
                    break;
                }
            }
        }

        private void Vontat_Btt_Click(object sender, EventArgs e)
        {
            addListbox(GetCurrentMethod());
            using (SendUMSMessageClient svc = CreateClient())
            {
                //svc.Vontat(MacAddress_txt.Text, sound, WCFMessage(Message_txt.Text)) // nem kell a szoveg, mert az uzenet szamma lesz konvertalva
                svc.Vontat(MacAddress_txt.Text, sound, Message_txt.Text);
            }
        }




        private void ServerLabelEdit_CheckedChanged(object sender, EventArgs e)
        {
            ServerLabel.ReadOnly = !ServerLabelEdit.Checked;
        }

        private void SvcNameEdit_CheckedChanged(object sender, EventArgs e)
        {
            SvcName.ReadOnly = !SvcNameEdit.Checked;
        }

        private void Connect_btt_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add("-------------------------------------------");

            if (hubConnection != null)
            {
                hubConnection.Dispose();
                hubConnection = null;
            }

            errNUM = 0;
            UmsConnected = UMSConnect();

            if ((umsHub != null) &&  (UmsConnected))
            {
                umsHub.Invoke("SetClient", OwnPass2ID_txt.Text, Environment_txt.Text, GetMACAddress("-"));
                addQueue(ActualConnectionIDStr());
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string ws = OwnPass2ID_txt.Text;
            OwnPass2ID_txt.Text = Addresss_txt.Text;
            Addresss_txt.Text = ws;
        }

        private void WCF_Pass2Message_Btt_Click(object sender, EventArgs e)
        {
            string group = "";
            if (IUR_Rbtt.Checked) group = "I";
            else if (FOR00_Rbtt.Checked) group = "F";
            else if (Mindenki_Rbtt.Checked) group = "A";
            using (SendUMSMessageClient svc = CreateClient())
            {
                svc.PASS2Message(OwnPass2ID_txt.Text, Environment_txt.Text, group, WCFMessage(Message_txt.Text));
            }
        }

    }

    #region ServerNamesList
    public class ServerNameItem
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public ServerNameItem(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
    public class ServerNameItemList : List<ServerNameItem> { }
    #endregion  ServerNamesList


}
