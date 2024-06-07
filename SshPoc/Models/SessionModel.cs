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

        private string _sessionId;
        private string _username = String.Empty;
        private string _password = String.Empty;
        private string _ipAddress = String.Empty;

        #endregion // Memeber Variables

        #region Constructor

        public SessionModel(string ip, string user, string pass)
        {
            _sessionId = Guid.NewGuid().ToString();
            Username = user;
            Password = pass;
            IpAddress = ip;
        }

        #endregion // Constructor

        #region Public Properties

        public string Username 
        { 
            get => _username; 
            set => _username = value; 
        }

        public string Password
        {
            get => _password;
            set => _password = value;
        }

        public string IpAddress
        {
            get => _ipAddress;
            set => _ipAddress = value;
        }

        public SshClient SshClient { get; set; }

        #endregion // Public Properties

        #region Public Methods
        
        public void ConnectSsh()
        {
            SshClient = new SshClient(host:IpAddress, username:Username, password:Password);
            SshClient.Connect();

            if (SshClient.IsConnected)
            {
                Debug.WriteLine(SshClient.ConnectionInfo.ServerVersion.ToString());

                
            }
                
        }

        public void DisconnectSsh()
        {
            if (SshClient.IsConnected)
                SshClient.Disconnect();
            
            SshClient.Dispose();
        }

        public bool GetConnectionStatus() => SshClient == null ? false : SshClient.IsConnected;

        public string RunCommand(string cmd) => SshClient.RunCommand(cmd).Result;

        

        #endregion // Public Methods

        #region Private Methods

        

        #endregion // Private Methods
    }
}
