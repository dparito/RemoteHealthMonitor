using Renci.SshNet;
using System.Diagnostics;

Debug.WriteLine("SSH Debug app with ssh.net");

using (var client = new SshClient(host: "192.168.1.12", username: "allspark", password:"Allspark"))
{
    client.Connect();
    while (client.IsConnected)
    {
        Debug.WriteLine(client.ConnectionInfo.ServerVersion);
        var result = client.RunCommand("ls -l");
        Debug.WriteLine(result.Result);
    }

    client.Disconnect();
    client.Dispose();
}