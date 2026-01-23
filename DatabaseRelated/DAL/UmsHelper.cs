using System;
using System.Configuration;
using System.Linq;

namespace MAVI.ARCH.UMS.DAL_NS
{
    public class UmsHelper : IDisposable
    {
        #region -- Private Variables --
        private bool disposed;

        UmsSchemaDataContext m_dc = null;
        #endregion


        #region -- Construction --
        public UmsHelper() : this("UMSDB") {}

        public UmsHelper(string dbName) 
        {
            string m_nameUMSDB = dbName;

            m_dc = new UmsSchemaDataContext(GetConnectionString(m_nameUMSDB))
            {
                CommandTimeout = 600
            };
        }

        #endregion

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
                if ((disposing) &&  (m_dc!=null))
                {
                        m_dc.Dispose();
                        m_dc = null;
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



        #region --- Class General Interface Properties / Functions ---


        /// <summary>
        /// Regisztráljuk a klienst
        /// </summary>
        /// <param name="cnnID">SignalR-connection ID</param>
        /// <param name="pass2UserID">PASS2 user azonosító</param>
        /// <param name="env">Környezet</param>
        /// <param name="macAddress">Kliens gépének MAC címe</param>
        public void addUserInfo(string cnnID, string pass2UserID, string env, string macAddress)
        {
            m_dc.usp_addUserInfo(cnnID, pass2UserID, env, macAddress);
        }

        /// <summary>
        /// Töröljuk a klienst a kapcsolatok közül (userInfo)
        /// </summary>
        /// <param name="cnnID"></param>
        public void delUserInfo(string cnnID)
        {
            m_dc.usp_delUserInfo(cnnID);
        }

        public UserInfo getUserInfoByConnection(string cnnID) {
            var ret = m_dc.usp_getUserInfoByConnectionId(cnnID).ToList<usp_getUserInfoByConnectionIdResult>();
            if (ret.Count > 0)
            {
                // SQ:Indexing at 0 should be used instead of the "Enumerable" extension method "First"
                return new UserInfo(ret[0]);
            }

            throw new ArgumentOutOfRangeException("cnnID", string.Format("Nem található olyan kapcsolat, aminek {0} lenne az azonosítója", cnnID));
        }

        public UserInfo getUserInfoByPass2Id(string pass2Id)
        {
            var ret = m_dc.usp_getUserInfoByPass2UserId(pass2Id).ToList<usp_getUserInfoByPass2UserIdResult>();
            if (ret.Count > 0)
            {
                // SQ:Indexing at 0 should be used instead of the "Enumerable" extension method "First"
                return new UserInfo(ret[0]);
            }

            throw new ArgumentOutOfRangeException("pass2Id", string.Format("Nem található olyan kapcsolat, aminek {0} lenne az azonosítója", pass2Id));

        }

        public UserInfo getUserInfoByMacAddress(string macAddress)
        {
            var ret = m_dc.usp_getUserInfoByMacAddress(macAddress).ToList<usp_getUserInfoByMacAddressResult>();
            if (ret.Count > 0)
            {
                // SQ:Indexing at 0 should be used instead of the "Enumerable" extension method "First"
                return new UserInfo(ret[0]);
            }

            throw new ArgumentOutOfRangeException("macAddress", string.Format("Nem található olyan kapcsolat, aminek {0} lenne a MACADDRESS azonosítója", macAddress));
        }

        public void setPass2Info(string cnnID, string pass2UserID, string env, string machineName, string macAddress)
        {
            m_dc.usp_setPass2Info(cnnID, pass2UserID, env, machineName, macAddress);
        }

        public void setMessage(string cnnID, string sender, string message)
        {
            m_dc.usp_setMessage(cnnID, sender, message);
        }


        public UserInfo[] getAllUserInfoByPass2Id(string pass2Id)
        {
            UserInfo[] rc;
            var ret = m_dc.usp_getUserInfoByPass2UserId(pass2Id).ToList<usp_getUserInfoByPass2UserIdResult>();
            if (ret.Count > 0)
            {
                int i = 0;
                rc = new UserInfo[ret.Count];
                foreach(var r in ret)
                {
                    rc[i] = new UserInfo(r);
                    i++;
                }
                return rc;
            }

            throw new ArgumentOutOfRangeException("pass2Id", string.Format("Nem található olyan kapcsolat, aminek {0} lenne az azonosítója", pass2Id));
        }


        public UserInfo[] getAllUserInfoByMAC(string macAddress)
        {
            UserInfo[] rc;
            var ret = m_dc.usp_getUserInfoByMacAddress(macAddress).ToList<usp_getUserInfoByMacAddressResult>();
            if (ret.Count > 0)
            {
                int i = 0;
                rc = new UserInfo[ret.Count];
                foreach (var r in ret)
                {
                    rc[i] = new UserInfo(r);
                    i++;
                }
                return rc;
            }

            throw new ArgumentOutOfRangeException("macAddress", string.Format("Nem található olyan kapcsolat, aminek {0} lenne az azonosítója", macAddress));
        }


        
        public UserInfo[] usp_getPass2UserIdListByApp(string app)
        {
            UserInfo[] rc=null;
            char appCH = app[0];
            var ret = m_dc.usp_getPass2UserIdListByApp(appCH).ToList<usp_getPass2UserIdListByAppResult>();
            if (ret.Count > 0)
            {
                int i = 0;
                rc = new UserInfo[ret.Count];
                foreach (var r in ret)
                {
                    rc[i] = new UserInfo(r);
                    i++;
                }
            }
            return rc;
        }



        #endregion

        #region -- Private --



        /// <summary>
        /// Visszaadja a connection stringet és reklamál ha nincs!
        /// </summary>
        /// <param name="name">A Connection String neve a konfigban</param>
        private string GetConnectionString(string name)
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[name];
            if (settings == null)
            {
                throw new ConfigurationErrorsException(string.Format("UmsHelper: Nem található a [{0}] nevű connectionstring settings a config állományban.", name));
            }

            string connectionString = settings.ConnectionString;
            if (connectionString == null)
            {
                throw new ConfigurationErrorsException(string.Format("UmsHelper: Hibás connectionstring a config állományban ([{0}]).", name));
            }

            return connectionString;
        }
        #endregion
    }
}
