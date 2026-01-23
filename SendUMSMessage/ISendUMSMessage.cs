using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;

namespace SendUMSMessage_NS
{
    [ServiceContract]
    // direkt nem irom at, mas rendszer is hasznalhatja
    [SuppressMessage("SonarAnalyzer.CSharp", "S101")]
    public interface ISendUMSMessage
    {
        [OperationContract]
        void Hello(string srcName, string env);

        [OperationContract]
        void Send(string srcName, string env, string name, string message);

        [OperationContract]
        void SendCL(string srcName, string env, string name, string message);

        [OperationContract]
        void SendToForm(string srcName, string env, string name, string formTypeID, string publicMessage, string privateMessage);

        [OperationContract]
        void SendToForm2(string uiID, string srcName, string env, string name, string formTypeID, string publicMessage, string privateMessage);

        [OperationContract]
        void SendAll(string srcName, string env, string message);

        [OperationContract]
        void SendList(string srcName, string env, string[] nameList, string message);

        [OperationContract]
        void SetClient(string pass2UserID, string env, string macAddress);

        [OperationContract]
        void KeszreJelentes(string macAddress, int sound, string message);

        [OperationContract]
        void Vontat(string macAddress, int sound, string message);

        // itt lehet WCF-en keresztuk kezdemenyezni a hivast
        [OperationContract]
        void PASS2Message(string srcName, string env, string app, string message);

        // belso hasznalatra - amikor az egyik UMS szerver (PASS2Message-bol) cimzi a masikat, immar 1 db cimmel
        [OperationContract]
        void PASS2MessageCL(string srcName, string env, string pass2ID, string message);

        [OperationContract]
        void For00FormOpen(string targetUser, string env, bool modal, string formName, string extraJsonData);

        [OperationContract]
        void For00FormOpenCL(string targetUser, string env, bool modal, string formName, string extraJsonData);
    }
}
