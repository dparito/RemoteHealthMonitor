using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SshPoc
{
    public class ConnectWindowViewModel: INotifyPropertyChanged
    {
        #region Private Members

        private string _username;
        private string _password;
        private string _hostIpAddr;
        private string _connectButtonContent;
        private string _toggleButtonContent;
        private string _cmdResponse;
        private bool _connStatus;
        private bool _currentErrorStatus;
        private bool _isLastErrorCleared;
        private string _userCmdInput;
        private bool _isRecording;
        private ShellStream? _shellStream;
        private StreamReader _reader;
        private StreamWriter _writer;
        private StringBuilder _streamedResult;
        private string _minVal;
        private string _maxVal;
        private string _thresholdVal;

        #endregion // Private Members

        #region Constructor

        public ConnectWindowViewModel() 
        {
            _username = "allspark"; 
            _hostIpAddr = "10.160.8.138"; 
            _password = "Allspark";
            
            _connectButtonContent = "Connect";
            _toggleButtonContent = "Run";
            
            _userCmdInput = "sudo ./asapp -T";
            _cmdResponse = string.Empty;
            
            _connStatus = false;
            _currentErrorStatus = _isLastErrorCleared = _isRecording = _connStatus;
            
            RunStopToggleCmd = new RelayCommand(RunStopToggleState);
            ConnectCommand = new RelayCommand(ConnectButtonPress);
            ApplyCommand = new RelayCommand(ApplyButtonPress);
        }

        #endregion // Constructor

        #region Public Properties
        
        public SessionModel Session { get; private set; }

        public string Username 
        { 
            get => _username; 
            set
            {
                if (_username != value) 
                {
                    _username = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        public string Password 
        { 
            get => _password; 
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        public string HostIpAddress 
        { 
            get => _hostIpAddr; 
            set
            {
                if (_hostIpAddr != value)
                {
                    _hostIpAddr = value;
                    OnPropertyChanged(nameof(HostIpAddress));
                }
            }
        }

        public string ConnectButtonContent
        {
            get => _connectButtonContent;
            set
            {
                _connectButtonContent = value;
                OnPropertyChanged(nameof(ConnectButtonContent));
            }
        }

        public string ToggleButtonContent
        {
            get => _toggleButtonContent;
            set
            {
                _toggleButtonContent = value;
                OnPropertyChanged(nameof(ToggleButtonContent));
            }
        }
        
        public string UserCmdInput
        {
            get => _userCmdInput;
            set
            {
                if (value != _userCmdInput)
                {
                    _userCmdInput = value;
                    OnPropertyChanged(nameof(UserCmdInput));
                }
            }
        }

        public string CmdResponse
        {
            get => _cmdResponse;
            private set
            {
                if (value != _cmdResponse)
                {
                    _cmdResponse = value;
                    OnPropertyChanged(nameof(CmdResponse));
                }
            }
        }

        public bool ConnStatus
        {
            get => _connStatus;
            set
            {
                if (value != _connStatus)
                {
                    _connStatus = value;
                    OnPropertyChanged(nameof(ConnStatus));
                }
            }
        }

        public bool CurrentErrorStatus
        {
            get => _currentErrorStatus;
            set
            {
                if (value != _currentErrorStatus)
                {
                    _currentErrorStatus = value;
                    OnPropertyChanged(nameof(CurrentErrorStatus));
                }
            }
        }

        public bool IsLastErrorCleared
        {
            get => _isLastErrorCleared;
            set
            {
                if (value != _isLastErrorCleared)
                {
                    _isLastErrorCleared = value;
                    OnPropertyChanged(nameof(IsLastErrorCleared));
                }
            }
        }

        public StringBuilder StreamedResult
        {
            get => _streamedResult;
            set => _streamedResult = value;
        }

        public ICommand RunStopToggleCmd { get; private set; }

        public ICommand ConnectCommand { get; private set; }

        public ICommand ApplyCommand { get; private set; }
        
        public string MinVal
        {
            get => _minVal;
            set
            {
                _minVal = value;
                OnPropertyChanged(nameof(MinVal));
            }
        }
        
        public string MaxVal
        {
            get => _maxVal;
            set
            {
                _maxVal = value;
                OnPropertyChanged(nameof(MaxVal));
            }
        }

        public string ThresholdVal
        {
            get => _thresholdVal;
            set
            {
                _thresholdVal = value;
                OnPropertyChanged(nameof(ThresholdVal));
            }
        }

        #endregion // Public Properties

        #region Private Methods

        private void ApplyButtonPress()
        {
            Debug.WriteLine($"Min: {MinVal}" +
                $"\nMax: {MaxVal}" +
                $"\nThreshold: {ThresholdVal}");
        }

        private void ConnectButtonPress()
        {
            if (ConnectButtonContent == "Connect")
            {
                ConnectButtonContent = "Disconnect";

                if (Session == null)
                {
                    Session = new SessionModel(HostIpAddress, Username, Password);

                    if (!Session.GetConnectionStatus())
                        if (Session.IpAddress != null || Session.Username != null)
                        {
                            try
                            {
                                Session.ConnectSsh();
                                if (Session.GetConnectionStatus())
                                {
                                    var testStr = Session.RunCommand("ls -l");
                                    Debug.WriteLine(testStr);

                                    ConnStatus = true;

                                    if (_shellStream == null && _reader == null && _writer == null)
                                        CreateShellStream();
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                            // TODO: Close this window
                        }
                        else
                            Debug.WriteLine("Invalid Credentials");
                    else
                        Debug.WriteLine("Session already connected!");
                }
            }
            else
            {
                ConnectButtonContent = "Connect";
                Session.DisconnectSsh();
                ConnStatus = false;
            }
            
        }

        private async void RunStopToggleState()
        {
            if (ToggleButtonContent == "Run")
            {
                ToggleButtonContent = "Stop";

                if (Session != null && Session.GetConnectionStatus())
                {
                    if (!_shellStream.CanWrite)
                        CreateShellStream();

                    CurrentErrorStatus = true;
                    IsLastErrorCleared = true;

                    if (UserCmdInput != string.Empty)
                    {
                        _isRecording = true;
                        while (_isRecording)
                        {
                            StartRecording(UserCmdInput);

                            await Task.Delay(100);
                        }

                    }
                    else
                        CmdResponse = "User command input empty!";
                }
            }
            else
            {
                StopRecording();
                ToggleButtonContent = "Run";
            }

        }

        private void StartRecording(string cmd)
        {
            try
            {
                WriteStream(cmd);

                // Start a background thread that will read in the data from the Pyng terminal
                ThreadStart threadStart = ReceiveData;
                Thread thread = new Thread(threadStart) { IsBackground = true };
                thread.Start();
            }
            catch (Exception e)
            {
                // TODO
                Debug.WriteLine(e);
            }
            finally
            {
                _isRecording = false;
            }
        }

        private void StopRecording() 
        {
            _shellStream.Flush();
            _writer.Flush();
            _reader.DiscardBufferedData();

            _reader.BaseStream.Close();

            _isRecording = false;
        }

        private void WriteStream(string cmd)
        {
            _writer.WriteLine(cmd);
            while (_shellStream.Length == 0)
            {
                Thread.Sleep(500);
            }
        }

        private void ReceiveData()
        {
            while (true)
            {
                try
                {
                    if (_reader != null)
                    {
                        StreamedResult = new StringBuilder();

                        string line;
                        while ((line = _reader.ReadLine()) != null)
                        {
                            CmdResponse += "\n" + line;
                            StreamedResult.AppendLine(CmdResponse);
                            Debug.WriteLine(CmdResponse);
                        }

                        if (!string.IsNullOrEmpty(StreamedResult.ToString()))
                        {
                            // TODO - Parse data at this point
                            Debug.WriteLine(StreamedResult.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    // TODO
                    Debug.WriteLine(e);
                }

                Thread.Sleep(200);
            }
        }

        private void CreateShellStream()
        {
            _shellStream = Session.SshClient.CreateShellStream(terminalName: "Terminal",
                                    columns: 80, rows: 60, width: 800, height: 600, bufferSize: 65536);

            _reader = new StreamReader(_shellStream, Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);

            _writer = new StreamWriter(_shellStream) { AutoFlush = true };
        }

        #endregion // Private Methods

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Implementation
    }
}
