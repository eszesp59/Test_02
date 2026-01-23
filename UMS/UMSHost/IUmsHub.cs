using System.Collections.Generic;

namespace UMSHost
{
    public interface IUmsHub
    {
        void Hello();
        void Send(string name, string message);
        void SendCL(string name, string message);
        void SendAll(string message);
        void SendList(List<string> nameList, string message);
        void SetClient(string pass2UserID, string env, string macAddress);
        void KeszreJelentes(string macAddress, int sound, string message);
        void Vontat(string macAddress, int sound, string message);
        void PASS2Message(string app, string message);
        void PASS2MessageCL(string app, string message);
        void For00FormOpen(string targetUser, string env, bool modal, string formName, string extraJsonData);
        void For00FormOpenCL(string targetUser, string env, bool modal, string formName, string extraJsonData);
    }
}
