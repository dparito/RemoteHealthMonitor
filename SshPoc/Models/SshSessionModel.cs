using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Renci.SshNet;

namespace SshPoc
{
    public class SshSessionModel : IDisposable, INotifyPropertyChanged
    {
        #region Member Variables

        private static string _sessionId;
        private string _username = String.Empty;
        private string _password = String.Empty;
        private string _ipAddress = String.Empty;
        private int _portNum;
        private bool _disposedValue = false;
        private bool _connStatus = false;
        private bool _currentErrStatus = false;
        private bool _isLastErrorCleared = false;
        private bool _isRecording;
        private bool _keepReading;
        private ShellStream? _shellStream;
        private StreamReader? _sshReader;
        private StreamWriter? _sshWriter;
        private StringBuilder? _sshStreamedResult;
        private StreamWriter? _serviceLogFileWriter;
        private StreamWriter? _auditLogFileWriter;
        private ConfigParser _configParser;
        private float _asappTempMeasureCurrent;

        #endregion // Memeber Variables

        #region Constructor

        /// <summary>
        /// Parameterized Constructor
        /// </summary>
        /// <param name="ip">IP address of the remote client</param>
        /// <param name="user">username of a profile on remote client</param>
        /// <param name="pass">password of the profile on remote client</param>
        public SshSessionModel(string ip, string user, string pass, int port)
        {
            _sessionId = Guid.NewGuid().ToString();
            Username = user;
            Password = pass;
            IpAddress = ip;
            PortNum = port;
            SshClient = new SshClient(host: IpAddress, port:PortNum, username: Username, password: Password);
            
            ConnStatus = false;
            CurrentErrStatus = false;
            IsLastErrorCleared = false;

            IsRecording = _keepReading = false;
            
            _configParser = new ConfigParser();
        }

        public SshSessionModel()
        {
            _sessionId = Guid.NewGuid().ToString();
            
            ConnStatus = false;
            CurrentErrStatus = false;
            IsLastErrorCleared = false;

            IsRecording = _keepReading = false;

            _configParser = new ConfigParser();
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

        public int PortNum { get => _portNum; set => _portNum = value; }

        /// <summary>
        /// Object of SshClient class
        /// </summary>
        public SshClient SshClient { get; set; }
        
        public bool DisposedValue 
        { 
            get => _disposedValue; 
            set => _disposedValue = value; 
        }
        
        public bool ConnStatus 
        { 
            get => _connStatus; 
            set
            {
                if (_connStatus != value)
                {
                    _connStatus = value;
                    OnPropertyChanged(nameof(ConnStatus));
                }
            }
        }
        
        public bool CurrentErrStatus 
        { 
            get => _currentErrStatus; 
            set
            {
                if (_currentErrStatus != value) 
                {
                    _currentErrStatus = value;
                    OnPropertyChanged(nameof(CurrentErrStatus));

                    if (!value)
                        SystemSounds.Exclamation.Play();
                }
            }
        }

        public bool IsLastErrorCleared
        {
            get => CurrentErrStatus && _isLastErrorCleared;
            set
            {
                if (value != _isLastErrorCleared)
                {
                    _isLastErrorCleared = value;
                    OnPropertyChanged(nameof(IsLastErrorCleared));
                }
            }
        }

        public ShellStream? ShellStream { get => _shellStream; set => _shellStream = value; }
        
        public bool IsRecording { get => _isRecording; set => _isRecording = value; }

        public enum TestType
        {
            GpuBurn,
            Asapp,
            WiFiFlooding,
            LatencyOnAllspark,
            LatencyOnJetson
        }

        #endregion // Public Properties

        #region Public Methods

        /// <summary>
        /// Establishes SSH connection with a remote client
        /// </summary>
        public void ConnectSsh(TestType test)
        {
            try
            {
                if (SshClient == null)
                {
                    if (test == TestType.LatencyOnJetson)
                        SshClient = new SshClient(IpAddress, port:22, Username, Password);
                    else
                        SshClient = new SshClient(IpAddress, port: 5003, Username, Password);
                }
                
                SshClient.Connect();

                // Verifying connection established
                if (SshClient.IsConnected)
                {
                    ConnStatus = true;
                    Debug.WriteLine(SshClient.ConnectionInfo.ServerVersion.ToString());

                    // Instantiate shell stream, stream reader and writer for first connection
                    if (ShellStream == null && _sshReader == null && _sshWriter == null)
                    {
                        switch (test)
                        {
                            case TestType.GpuBurn:
                                CreateShellStream(TestType.GpuBurn);
                                break;
                            case TestType.Asapp:
                                CreateShellStream(TestType.Asapp);
                                break;
                            case TestType.WiFiFlooding:
                                CreateShellStream(TestType.WiFiFlooding);
                                break;
                            case TestType.LatencyOnAllspark:
                                CreateShellStream(TestType.LatencyOnAllspark);
                                break;
                            case TestType.LatencyOnJetson:
                                CreateShellStream(TestType.LatencyOnJetson);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        /// <summary>
        /// Disconnects SSH connection
        /// </summary>
        public void DisconnectSsh()
        {
            if (!DisposedValue)
            {
                if (SshClient.IsConnected)
                {
                    if (IsRecording || _keepReading)
                        StopRecording();

                    SshClient.Disconnect();
                    ConnStatus = false;
                }
                SshClient?.Dispose();
            }
        }

        /// <summary>
        /// Gets SSH connection status
        /// </summary>
        /// <returns>True if SSH connection to remote client valid</returns>
        public bool GetConnectionStatus()
        {
            if (SshClient == null)
                return false;
            else
                return SshClient.IsConnected;
        }

        /// <summary>
        /// Runs a terminal command on remote client over SSH
        /// </summary>
        /// <param name="cmd">user specified command</param>
        /// <returns>results of command execution as returned by the remote client</returns>
        public string RunCommand(string cmd)
        {
            string response;
            try
            {
                response = SshClient.RunCommand(cmd).Result;
            }
            catch
            {
                throw new Exception();
            }
            return response;
        }

        /// <summary>
        /// Creates and starts a thread to write user command to remote SSH terminal
        /// </summary>
        /// <param name="cmd">user command as string</param>
        public void StartRecording(string cmd, TestType test)
        {
            try
            {
                WriteStream(cmd);

                switch(test)
                {
                    case TestType.GpuBurn:
                        // Start a background thread that will read in the data from the Pyng terminal
                        ThreadStart threadStartGpu = ReceiveDataForGpuBurnTest;
                        Thread threadGpu = new Thread(threadStartGpu) { IsBackground = true };
                        threadGpu.Start();
                        break;

                    case TestType.Asapp:
                        // Start a background thread that will read in the data from the Pyng terminal
                        ThreadStart threadStartAsapp = ReceiveDataForAsappTest;
                        Thread thread = new Thread(threadStartAsapp) { IsBackground = true };
                        thread.Start();
                        break;

                    case TestType.WiFiFlooding:
                        // Start a background thread that will read in the data from the Pyng terminal
                        ThreadStart threadStartWiFi = ReceiveDataForWiFiFloodTest;
                        Thread threadWiFi = new Thread(threadStartWiFi) { IsBackground = true };
                        threadWiFi.Start();
                        break;

                    case TestType.LatencyOnAllspark:
                        // Start a background thread that will read in the data from the Pyng terminal
                        ThreadStart threadStartLatencyAllspark = ReceiveDataForLatencyTestAllspark;
                        Thread threadLatencyAllspark = new Thread(threadStartLatencyAllspark) { IsBackground = true };
                        threadLatencyAllspark.Start();
                        break;
                    case TestType.LatencyOnJetson:
                        // Start a background thread that will read in the data from the Pyng terminal
                        ThreadStart threadStartLatencyJetson = ReceiveDataForLatencyTestJetson;
                        Thread threadLatencyJetson = new Thread(threadStartLatencyJetson) { IsBackground = true };
                        threadLatencyJetson.Start();
                        break;
                }
                _keepReading = true;
            }
            catch (Exception e)
            {
                // TODO
                Debug.WriteLine(e);
            }
            finally
            {
                IsRecording = false;
            }
        }

        /// <summary>
        /// Closes thread that writes to remote SSH terminal
        /// </summary>
        public void StopRecording()
        {
            ShellStream?.Flush();
            _sshWriter?.Flush();
            _sshReader?.DiscardBufferedData();
            _sshStreamedResult?.Clear();
            _serviceLogFileWriter?.Flush();
            _serviceLogFileWriter?.Close();
            _serviceLogFileWriter?.Dispose();
            _auditLogFileWriter?.Flush();
            _auditLogFileWriter?.Close();
            _auditLogFileWriter?.Dispose();

            _sshReader?.BaseStream.Close();

            IsRecording = false;
            _keepReading = false;
        }

        
        /// <summary>
        /// Creates instances of shell stream, stream reader and writer
        /// </summary>
        public void CreateShellStream(TestType test)
        {
            ShellStream = SshClient.CreateShellStream(terminalName: "Terminal",
                                    columns: 80, rows: 60, width: 800, height: 600, bufferSize: 65536);

            _sshReader = new StreamReader(ShellStream, Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);

            _sshWriter = new StreamWriter(ShellStream) { AutoFlush = true };

            //var filename = @"C:\Temp\";
            var serviceLogFilename = "";
            var auditLogFilename = "";
            switch (test)
            {
                case TestType.GpuBurn:
                    serviceLogFilename = @"GpuBurnTest_service.log";
                    auditLogFilename = @"GpuBurnTest_audit.log";
                    break;

                case TestType.Asapp:
                    serviceLogFilename = @"AsappTest_service.log";
                    auditLogFilename = @"AsappTest_audit.log";
                    break;

                case TestType.WiFiFlooding:
                    serviceLogFilename = @"WiFiFloodingTest_service.log";
                    auditLogFilename = @"WiFiFloodingTest_audit.log";
                    break;

                case TestType.LatencyOnAllspark:
                    serviceLogFilename = @"LatencyTest_Allspark_service.log";
                    auditLogFilename = @"LatencyTest_Allspark_audit.log";
                    break;
                case TestType.LatencyOnJetson:
                    serviceLogFilename = @"LatencyTest_Jetson_service.log";
                    auditLogFilename = @"LatencyTest_Jetson_audit.log";
                    break;
            }
            
            _serviceLogFileWriter = new StreamWriter(serviceLogFilename, append: File.Exists(serviceLogFilename));
            _auditLogFileWriter = new StreamWriter(auditLogFilename, append: File.Exists(auditLogFilename));
        }

        public void ClearLastErrorButtonPress()
        {
            if (CurrentErrStatus)
                IsLastErrorCleared = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!DisposedValue)
            {
                if (disposing)
                {
                    SshClient?.Dispose();
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                DisposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SessionModel()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        #endregion // Public Methods

        #region Private Methods

        /// <summary>
        /// Starts writing user command to remote SSH terminal
        /// </summary>
        /// <param name="cmd">user command as string</param>
        private void WriteStream(string cmd)
        {
            Thread.CurrentThread.IsBackground = true;
            _sshWriter.WriteLine(cmd);
            while (ShellStream.Length == 0)
            {
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Contains thread to received and parse data from remote SSH terminal
        /// </summary>
        private void ReceiveDataForGpuBurnTest()
        {
            _sshStreamedResult = new StringBuilder();

            // keep receiving data until stream active, every 200 ms
            while (_keepReading)
            {
                try
                {
                    // if reader object valid 
                    if (_sshReader != null)
                    {
                        string line;

                        // while remote SSH terminal responds with a non-null string
                        while ((line = _sshReader.ReadLine()) != null)
                        {
                            _sshStreamedResult.AppendLine("\n" + line);
                            Debug.WriteLine(line);
                            WriteToLogFile(line);

                            if (line.Contains("Result = "))
                            {
                                var result = line.Substring(line.Length - 4, 4);
                                CurrentErrStatus = result == "PASS";
                                IsLastErrorCleared &= CurrentErrStatus;

                                if (!CurrentErrStatus)
                                    WriteToLogFile(line, isForAuditLog: true);
                            }
                        }

                        // Process data received from remote SSH terminal in this session
                        if (!string.IsNullOrEmpty(_sshStreamedResult.ToString()))
                        {
                            // TODO - Parse data at this point
                            Debug.WriteLine(_sshStreamedResult.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }

                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// Contains thread to received and parse data from remote SSH terminal
        /// </summary>
        private void ReceiveDataForAsappTest()
        {
            _sshStreamedResult = new StringBuilder();

            // keep receiving data until stream active, every 200 ms
            while (_keepReading)
            {
                try
                {
                    // if reader object valid 
                    if (_sshReader != null)
                    {
                        string line;

                        // while remote SSH terminal responds with a non-null string
                        while ((line = _sshReader.ReadLine()) != null)
                        {
                            _sshStreamedResult.AppendLine("\n" + line);
                            Debug.WriteLine(line);
                            WriteToLogFile(line, isForAuditLog: false);

                            if (line.Contains("Value is"))
                            {
                                _asappTempMeasureCurrent = float.Parse(line.Substring(line.Length-8, 7));
                                CurrentErrStatus = _asappTempMeasureCurrent > _configParser.TestLimits.Asapp.MaxTemp;
                                IsLastErrorCleared &= CurrentErrStatus;

                                if (!CurrentErrStatus)
                                    WriteToLogFile(line, isForAuditLog: true);
                            }
                        }

                        // Process data received from remote SSH terminal in this session
                        if (!string.IsNullOrEmpty(_sshStreamedResult.ToString()))
                        {
                            // TODO - Parse data at this point
                            Debug.WriteLine(_sshStreamedResult.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }

                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// Contains thread to received and parse data from remote SSH terminal
        /// </summary>
        private void ReceiveDataForWiFiFloodTest()
        {
            _sshStreamedResult = new StringBuilder();

            // keep receiving data until stream active, every 200 ms
            while (_keepReading)
            {
                try
                {
                    // if reader object valid 
                    if (_sshReader != null)
                    {
                        string line;

                        // while remote SSH terminal responds with a non-null string
                        while ((line = _sshReader.ReadLine()) != null)
                        {
                            _sshStreamedResult.AppendLine("\n" + line);
                            Debug.WriteLine(line);
                            WriteToLogFile(line, isForAuditLog: false);
                            
                            //65515 bytes from 192.168.1.1: icmp_seq = 11 ttl = 64 time = 23.6 ms
                            if (line.Contains("time"))
                            {
                                var result = float.Parse(line.Substring(line.IndexOf("time") + 5, 4));
                                CurrentErrStatus = result < 100;
                                IsLastErrorCleared &= CurrentErrStatus;

                                if (!CurrentErrStatus)
                                    WriteToLogFile(line, isForAuditLog: true);
                            }
                        }

                        // Process data received from remote SSH terminal in this session
                        if (!string.IsNullOrEmpty(_sshStreamedResult.ToString()))
                        {
                            // TODO - Parse data at this point
                            Debug.WriteLine(_sshStreamedResult.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }

                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// Contains thread to received and parse data from remote SSH terminal
        /// </summary>
        private void ReceiveDataForLatencyTestAllspark()
        {
            _sshStreamedResult = new StringBuilder();

            // keep receiving data until stream active, every 200 ms
            while (_keepReading)
            {
                try
                {
                    // if reader object valid 
                    if (_sshReader != null)
                    {
                        string line;

                        // while remote SSH terminal responds with a non-null string
                        while ((line = _sshReader.ReadLine()) != null)
                        {
                            _sshStreamedResult.AppendLine("\n" + line);
                            Debug.WriteLine(line);
                            WriteToLogFile(line, isForAuditLog: false);

                            if (line.Contains("Value"))
                            {
                                CurrentErrStatus = false; // (Temperature <= _threshold);
                                IsLastErrorCleared &= CurrentErrStatus;

                                if (!CurrentErrStatus)
                                    WriteToLogFile(line, isForAuditLog: true);
                            }
                        }

                        // Process data received from remote SSH terminal in this session
                        if (!string.IsNullOrEmpty(_sshStreamedResult.ToString()))
                        {
                            // TODO - Parse data at this point
                            Debug.WriteLine(_sshStreamedResult.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }

                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// Contains thread to received and parse data from remote SSH terminal
        /// </summary>
        private void ReceiveDataForLatencyTestJetson()
        {
            _sshStreamedResult = new StringBuilder();

            // keep receiving data until stream active, every 200 ms
            while (_keepReading)
            {
                try
                {
                    // if reader object valid 
                    if (_sshReader != null)
                    {
                        string line;

                        // while remote SSH terminal responds with a non-null string
                        while ((line = _sshReader.ReadLine()) != null)
                        {
                            _sshStreamedResult.AppendLine("\n" + line);
                            Debug.WriteLine(line);
                            WriteToLogFile(line, isForAuditLog: false);

                            if (line.Contains("Value"))
                            {
                                var latency = int.Parse(line.Substring(1));
                                CurrentErrStatus = latency > _configParser.TestLimits.Latency.MaxFrameLatency;
                                IsLastErrorCleared &= CurrentErrStatus;

                                if (!CurrentErrStatus)
                                    WriteToLogFile(line, isForAuditLog: true);
                            }
                        }

                        // Process data received from remote SSH terminal in this session
                        if (!string.IsNullOrEmpty(_sshStreamedResult.ToString()))
                        {
                            // TODO - Parse data at this point
                            Debug.WriteLine(_sshStreamedResult.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }

                Thread.Sleep(200);
            }
        }

        private void WriteToLogFile(string logEntry, bool isForAuditLog=false)
        {
            logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff} " + logEntry;
            try
            {
                if (isForAuditLog)
                    _serviceLogFileWriter?.WriteLine(logEntry);
                else
                    _auditLogFileWriter?.WriteLine(logEntry);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        
        #endregion // Private Methods

        #region INotifyPropertyChanged Implementation

        /// <summary>
        /// Event to trigger change of property value
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Event handler for PropertyChanged
        /// </summary>
        /// <param name="propertyName">Name of the changed property</param>
        protected virtual void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Implementation
    }
}
