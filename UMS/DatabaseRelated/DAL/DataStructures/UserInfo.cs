using System;

namespace MAVI.ARCH.UMS.DAL_NS
{
    public struct UserInfo
    {
        public UserInfo(usp_getUserInfoByConnectionIdResult src)
        {
            m_SignalRConnectionID = src.SignalRConnectionID;

            m_ConnectionStarted = src.ConnectionStarted;
            m_EnvironmentName = src.EnvironmentName;
            m_MachineName = src.MachineName;
            m_LastMessageSender = src.LastMessageSender;
            m_LastMessageText = src.LastMessageText;
            m_LastMessageTime = src.LastMessageTime;
            m_Pass2UserID = src.Pass2UserID;
            m_MACAddress = src.MACAddress;
        }

        public UserInfo(usp_getUserInfoByPass2UserIdResult src)
        {
            m_SignalRConnectionID = src.SignalRConnectionID;

            m_ConnectionStarted = src.ConnectionStarted;
            m_EnvironmentName = src.EnvironmentName;
            m_MachineName = src.MachineName;
            m_LastMessageSender = src.LastMessageSender;
            m_LastMessageText = src.LastMessageText;
            m_LastMessageTime = src.LastMessageTime;
            m_Pass2UserID = src.Pass2UserID;
            m_MACAddress = src.MACAddress;
        }


        public UserInfo(usp_getUserInfoByMacAddressResult src)
        {
            m_SignalRConnectionID = src.SignalRConnectionID;

            m_ConnectionStarted = src.ConnectionStarted;
            m_EnvironmentName = src.EnvironmentName;
            m_MachineName = src.MachineName;
            m_LastMessageSender = src.LastMessageSender;
            m_LastMessageText = src.LastMessageText;
            m_LastMessageTime = src.LastMessageTime;
            m_Pass2UserID = src.Pass2UserID;
            m_MACAddress = src.MACAddress;
        }

        public UserInfo(usp_getPass2UserIdListByAppResult src)
        {
            m_SignalRConnectionID = src.SignalRConnectionID;

            m_ConnectionStarted = null;
            m_EnvironmentName = src.EnvironmentName;
            m_MachineName = src.MachineName;
            m_LastMessageSender = null;
            m_LastMessageText = null;
            m_LastMessageTime = null;
            m_Pass2UserID = src.Pass2UserID;
            m_MACAddress = src.MACAddress;
        }


        public override string ToString()
        {
            return string.Format(
                "UserInfo Data: -> Pass2:={0} MAC:{4} (ID:={1})\r\n" +
                "\tSender::={2}\r\n" +
                "\tMsg:={3}",
                m_Pass2UserID, m_SignalRConnectionID,m_LastMessageSender, m_LastMessageText, m_MACAddress);
        }

        #region -- Properties --
        public string Id { get { return m_SignalRConnectionID; } }
        public string Pass2Id { get { return m_Pass2UserID; } }

        public string MACAddRess { get { return m_MACAddress; } }

        public string LastSender { get { return m_LastMessageSender; } }
        public string LastMessage { get { return m_LastMessageText; } }

        public string EnvName { get { return m_EnvironmentName; } }
        public string MachineName { get { return m_MachineName; } }
        public DateTime? ConnectionStarted { get { return m_ConnectionStarted; } }

        public DateTime? LastMessageTime { get { return m_LastMessageTime; } }
        #endregion


        #region -- Private Variables --
        readonly string m_SignalRConnectionID;
        readonly string m_Pass2UserID;
        readonly string m_MACAddress;

        readonly string m_LastMessageSender;
        readonly string m_LastMessageText;
        readonly string m_EnvironmentName;
        readonly string m_MachineName;

        readonly DateTime? m_ConnectionStarted;
        readonly DateTime? m_LastMessageTime;
        #endregion
    }
}
