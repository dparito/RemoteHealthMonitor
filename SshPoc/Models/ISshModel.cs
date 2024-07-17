using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SshPoc
{
    interface ISshModel
    {
        string Username { get; set; }
        
        string Password { get; set; }
        
        string IpAddress { get; set; }

        int PortNum { get; set; }

        bool ConnStatus { get; set; }

        bool CurrentErrorStatus { get; set; }

        bool IsLastErrorCleared { get; set; }
        
        ShellStream? ShellStream { get; set; }

        bool IsRecording { get; set; }

        string CommandToRun { get; set; }

        object TestLimit { get; set; }

        void ConnectSsh();

        void DisconnectSsh();

        void CreateShellStream();

        void ReceiveDataAndParse();

        void StartRecording(string cmd);

        void StopRecording();
    }
}
