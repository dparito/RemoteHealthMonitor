using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static SshPoc.SshSessionModel;
using static System.Net.Mime.MediaTypeNames;

namespace SshPoc
{
    public class LatencyTestSessionModel : ISshModel, INotifyPropertyChanged, IDisposable
    {
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
        private SshClient _sshClient;
        private string _commandToRun;
        private object _testLimit;

        public LatencyTestSessionModel()
        {
            _sessionId = Guid.NewGuid().ToString();
            Username = Password = IpAddress = string.Empty;
            ConnStatus = CurrentErrorStatus = IsLastErrorCleared = false;
            _configParser = new ConfigParser();
            //CommandToRun = _configParser.Limits.GpuBurn.CommandToRun;
            //TestLimit = _configParser.Limits.GpuBurn.TestLimit;
        }

        public LatencyTestSessionModel(string username, string password, string ipaddress, int port)
        {
            _sessionId = Guid.NewGuid().ToString();
            Username = username;
            Password = password;
            IpAddress = ipaddress;
            PortNum = port;
            _sshClient = new SshClient(host: IpAddress,
                                       port: PortNum,
                                       username: Username,
                                       password: Password);
            ConnStatus = CurrentErrorStatus = IsLastErrorCleared = false;
            _configParser = new ConfigParser();
            //CommandToRun = _configParser.Limits.GpuBurn.CommandToRun;
            //TestLimit = _configParser.Limits.GpuBurn.TestLimit;
        }

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
        public string IpAddress { get => _ipAddress; set => _ipAddress = value; }

        public int PortNum { get => _portNum; set => _portNum = value; }

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

        public bool CurrentErrorStatus
        {
            get => _currentErrStatus;
            set
            {
                if (_currentErrStatus != value)
                {
                    _currentErrStatus = value;
                    OnPropertyChanged(nameof(CurrentErrorStatus));

                    if (!value)
                        SystemSounds.Exclamation.Play();
                }
            }
        }

        public bool IsLastErrorCleared
        {
            get => CurrentErrorStatus && _isLastErrorCleared;
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

        public string CommandToRun { get => _commandToRun; set => _commandToRun = value; }
        
        public object TestLimit { get => _testLimit; set => _testLimit = value; }

        public void ConnectSsh()
        {
            try
            {
                _sshClient ??= new SshClient(IpAddress, port: 5003, Username, Password);

                _sshClient.Connect();
                ConnStatus = _sshClient.IsConnected;
                
                    // Verifying connection established
                if (ConnStatus)
                {
                    Debug.WriteLine(_sshClient.ConnectionInfo.ServerVersion.ToString());

                    // Instantiate shell stream, stream reader and writer for first connection
                    if (ShellStream == null && _sshReader == null && _sshWriter == null)
                    {
                        CreateShellStream();
                    }
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        public void DisconnectSsh()
        {
            if (!DisposedValue)
            {
                if (ConnStatus)
                {
                    if (IsRecording || _keepReading)
                        StopRecording();

                    _sshClient.Disconnect();
                    ConnStatus = false;
                }
                _sshClient?.Dispose();
            }
        }

        public void CreateShellStream()
        {
            ShellStream = _sshClient.CreateShellStream(terminalName: "Terminal",
                                    columns: 80, rows: 60, width: 800, height: 600, bufferSize: 65536);

            _sshReader = new StreamReader(ShellStream, Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);

            _sshWriter = new StreamWriter(ShellStream) { AutoFlush = true };

            var serviceLogFilename = "WifiFloodTest_service.log";
            _serviceLogFileWriter = new StreamWriter(serviceLogFilename, append: File.Exists(serviceLogFilename));
            
            var auditLogFilename = "WifiFloodTest_audit.log";
            _serviceLogFileWriter = new StreamWriter(auditLogFilename, append: File.Exists(auditLogFilename));
        }

        public void ReceiveDataAndParse()
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

                            if (line.Contains("ping"))
                            {
                                // TODO: parse latency
                                var result = float.Parse(line.Substring(line.Length - 8, 7));
                                if (result < (float)_configParser.Limits.Asapp.TestLimit)
                                {
                                    CurrentErrorStatus = true;
                                    IsLastErrorCleared &= CurrentErrorStatus;
                                }
                                else
                                {
                                    CurrentErrorStatus = false;
                                    IsLastErrorCleared &= CurrentErrorStatus;
                                    WriteToLogFile(line, isAuditLog: true);
                                }
                                    
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
                    Debug.WriteLine(e.Message);
                }

                Thread.Sleep(200);
            }
        }

        public void StartRecording(string cmd)
        {
            try
            {
                WriteStream(cmd);
                
                // Start a background thread that will read in the data from the Pyng terminal
                ThreadStart threadStartGpu = ReceiveDataAndParse;
                Thread threadGpu = new Thread(threadStartGpu) { IsBackground = true };
                threadGpu.Start();
                _keepReading = true;
            }
            catch (Exception ex)
            {
                //TODO
                Debug.WriteLine(ex.Message);
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
        /// Starts writing user command to remote SSH terminal
        /// </summary>
        /// <param name="cmd">user command as string</param>
        private void WriteStream(string cmd)
        {
            try
            {
                Thread.CurrentThread.IsBackground = true;
                _sshWriter?.WriteLine(cmd);
                while (ShellStream?.Length == 0)
                {
                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void WriteToLogFile(string entry, bool isAuditLog = false)
        {
            try
            {
                if (isAuditLog)
                    _auditLogFileWriter?.WriteLine(entry);
                else
                    _serviceLogFileWriter?.WriteLine(entry);
            }
            catch (Exception ex) 
            {
                Debug.WriteLine(ex.Message);
            }
        }


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

        #region IDisposable implementation

        public bool DisposedValue
        {
            get => _disposedValue;
            set => _disposedValue = value;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!DisposedValue)
            {
                if (disposing)
                {
                    _sshClient?.Dispose();
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                DisposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion // IDisposable implementation
    }
}
