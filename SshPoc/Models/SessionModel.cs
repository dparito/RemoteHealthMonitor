using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace SshPoc
{
    public class SessionModel
    {
        #region Member Variables

        private static string _sessionId;
        private string _username = String.Empty;
        private string _password = String.Empty;
        private string _ipAddress = String.Empty;

        #endregion // Memeber Variables

        #region Constructor

        /// <summary>
        /// Parameterized Constructor
        /// </summary>
        /// <param name="ip">IP address of the remote client</param>
        /// <param name="user">username of a profile on remote client</param>
        /// <param name="pass">password of the profile on remote client</param>
        public SessionModel(string ip, string user, string pass)
        {
            _sessionId = Guid.NewGuid().ToString();
            Username = user;
            Password = pass;
            IpAddress = ip;
            SshClient = new SshClient(host: IpAddress, username: Username, password: Password);
        }

        #endregion // Constructor

        #region Public Properties

        /// <summary>
        /// Username of a profile on remote client
        /// </summary>
        public string Username 
        { 
            get => _username; 
            set => _username = value; 
        }

        /// <summary>
        /// Password of the profile on remote client
        /// </summary>
        public string Password
        {
            get => _password;
            set => _password = value;
        }

        /// <summary>
        /// IP Address of the remote client
        /// </summary>
        public string IpAddress
        {
            get => _ipAddress;
            set => _ipAddress = value;
        }

        /// <summary>
        /// Object of SshClient class
        /// </summary>
        public SshClient SshClient { get; set; }

        #endregion // Public Properties

        #region Public Methods
        
        /// <summary>
        /// Method to establish SSH connection with a remote client
        /// </summary>
        public void ConnectSsh()
        {
            try
            {
                SshClient.Connect();

                // Verifying connection established
                if (SshClient.IsConnected)
                {
                    Debug.WriteLine(SshClient.ConnectionInfo.ServerVersion.ToString());
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        /// <summary>
        /// Method to disconnect SSH connection
        /// </summary>
        public void DisconnectSsh()
        {
            if (SshClient.IsConnected)
                SshClient.Disconnect();
            
            SshClient.Dispose();
        }

        /// <summary>
        /// Method to get SSH connection status
        /// </summary>
        /// <returns>True if SSH connection to remote client valid</returns>
        public bool GetConnectionStatus() => SshClient != null && SshClient.IsConnected;


        /// <summary>
        /// Method to run a terminal command on remote client over SSH
        /// </summary>
        /// <param name="cmd">user specified command</param>
        /// <returns>results of command execution as returned by the remote client</returns>
        public string RunCommand(string cmd) => SshClient.RunCommand(cmd).Result;
        

        #endregion // Public Methods

        #region Private Methods
        #endregion // Private Methods
    }
}
