using Renci.SshNet;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SshPoc
{
    public class LatencyMonitorViewModel : INotifyPropertyChanged
    {
        #region Private Members

        private string _username;
        private string _password;
        private string _hostIpAddr;
        private string _connectSshButtonContent;
        private bool _connSshStatus;
        private bool _isSshConnectable;
        private bool _currentErrorStatus;
        private bool _isLastErrorCleared;
        private string _baudRate;
        private string _comPort;
        private bool _isSerialConnectable;
        private bool _connSerialStatus;
        private string _connectSerialButtonContent;
        private string _runStopLoopbackContent;
        private string _runStopInjectorContent;
        private string _runStopAnalyzerContent;
        private bool _isRecording;
        private bool _keepReading;
        private ShellStream? _shellStream;
        private StreamReader? _sshReader;
        private StreamWriter? _sshWriter;
        private StringBuilder? _sshStreamedResult;
        private FileStream? _fileStream;
        private StreamWriter? _fileWriter;
        private double _threshold;
        private double _temp;

        #endregion // Private Members

        #region Constructors

        public LatencyMonitorViewModel()
        {
            //Username = HostIpAddress = Password = string.Empty;
            Username = "allspark";
            HostIpAddress = string.Empty;
            Password = "Allspark";
            ConnectSshButtonContent = "Connect";
            IsSshConnectable = false;
            ConnSshStatus = false;

            BaudRate = "115200"; 
            ComPort = string.Empty;
            ConnectSerialButtonContent = "Connect";
            IsSerialConnectable = false;
            ConnSerialStatus = false;
            
            CurrentErrorStatus = IsLastErrorCleared = false;
            _isRecording = _keepReading = false;

            RunStopLoopbackContent = "Run Loopback";
            RunStopInjectorContent = "Run EMI Stress Test";
            RunStopAnalyzerContent = "Run Counter Analyzer";

            _threshold = 43.0;

            //_random = new Random();
            //_currentSecond = 0;
            //Temperature = 0.0;
            //Points = new PointCollection();
            //_seconds = [];
            //_temps = [];
            //_isPlottable = false;
            //MyModel = new MyModel()
            //{
            //    Points = Points,
            //    ColorName = "black",
            //};
            //_timer = new DispatcherTimer();
            //_timer.Tick += Timer_Tick;
            //_timer.Interval = TimeSpan.FromMilliseconds(500);
            //_timer.Start();

            ConnectSshCommand = new RelayCommand(ConnectSshButtonPress);
            ConnectSerialCommand = new RelayCommand(ConnectSerialButtonPress);
            RunLoopbackCmd = new RelayCommand(RunLoopbackButtonPress);
            RunInjectorCmd = new RelayCommand(RunInjectorButtonPress);
            RunAnalyzer = new RelayCommand(RunAnalyzerButtonPress);
            //PlotCommand = new RelayCommand(PlotButtonPress);
            ClearLastErrorCommand = new RelayCommand(ClearLastErrorButtonPress);
        }

        #endregion // Constructors

        #region Public Properties

        /// <summary>
        /// Object of SessionModel class
        /// </summary>
        public SessionModel? Session { get; private set; }

        public SerialPort? SerialPort { get; private set; }

        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(Username));

                    IsSshConnectable = (Username != string.Empty && Password != string.Empty && HostIpAddress != string.Empty);
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

                    IsSshConnectable = (Username != string.Empty && Password != string.Empty && HostIpAddress != string.Empty);
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

                    IsSshConnectable = (Username != string.Empty && Password != string.Empty && HostIpAddress != string.Empty);
                }
            }
        }

        public string ConnectSshButtonContent
        {
            get => _connectSshButtonContent;
            set
            {
                _connectSshButtonContent = value;
                OnPropertyChanged(nameof(ConnectSshButtonContent));
            }
        }

        public bool EitherConnStatus
        {
            get => ConnSshStatus || ConnSerialStatus;
        }

        public bool ConnSshStatus
        {
            get => _connSshStatus;
            set
            {
                if (value != _connSshStatus)
                {
                    _connSshStatus = value;
                    OnPropertyChanged(nameof(ConnSshStatus));

                    if (!ConnSshStatus)
                    {
                        CurrentErrorStatus = ConnSshStatus;
                        IsLastErrorCleared = ConnSshStatus;
                    }
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

                    if (value == false)
                        IsLastErrorCleared = false;
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

        public string BaudRate
        {
            get => _baudRate;
            set
            {
                _baudRate = value;
                OnPropertyChanged(nameof(BaudRate));

                IsSerialConnectable = (BaudRate != string.Empty && ComPort != string.Empty);
            }
        }

        public string ComPort
        {
            get => _comPort;
            set
            {
                _comPort = value;
                OnPropertyChanged(nameof(ComPort));

                IsSerialConnectable = (BaudRate != string.Empty && ComPort != string.Empty);
            }
        }

        public bool IsSshConnectable
        {
            get => _isSshConnectable;
            set
            {
                _isSshConnectable = value;
                OnPropertyChanged(nameof(IsSshConnectable));
            }
        }

        public bool IsSerialConnectable 
        { 
            get => _isSerialConnectable; 
            set
            {
                _isSerialConnectable = value;
                OnPropertyChanged(nameof(IsSerialConnectable));
            }
        }

        public string RunStopLoopbackContent 
        { 
            get => _runStopLoopbackContent; 
            set
            {
                _runStopLoopbackContent = value;
                OnPropertyChanged(nameof(RunStopLoopbackContent));
            }
        }
        
        public string RunStopInjectorContent 
        { 
            get => _runStopInjectorContent; 
            set
            {
                _runStopInjectorContent = value;
                OnPropertyChanged(nameof(RunStopInjectorContent));
            }
        }
        
        public string RunStopAnalyzerContent 
        { 
            get => _runStopAnalyzerContent; 
            set
            {
                _runStopAnalyzerContent = value;
                OnPropertyChanged(nameof(RunStopAnalyzerContent));
            }
        }

        public bool ConnSerialStatus 
        { 
            get => _connSerialStatus; 
            set
            {
                _connSerialStatus = value;
                OnPropertyChanged(nameof(ConnSerialStatus));
            }
        }

        public string ConnectSerialButtonContent 
        { 
            get => _connectSerialButtonContent; 
            set
            {
                _connectSerialButtonContent = value;
                OnPropertyChanged(nameof(ConnectSerialButtonContent));
            }
        }

        //public PointCollection Points 
        //{ 
        //    get => _points; 
        //    set
        //    {
        //        _points = value;
        //        OnPropertyChanged(nameof(Points));
        //    }
        //}

        public double Temperature 
        { 
            get => _temp; 
            set
            {
                _temp = value;
                OnPropertyChanged(nameof(Temperature));
            }
        }

        //public bool IsPlottable
        //{
        //    get => _isPlottable;
        //    set
        //    {
        //        _isPlottable = value;
        //        OnPropertyChanged(nameof(IsPlottable));
        //    }
        //}

        //public MyModel MyModel { get; set; }

        public ICommand ConnectSshCommand { get; private set; }

        public ICommand ConnectSerialCommand { get; private set; }

        public ICommand RunLoopbackCmd { get; private set; }
        
        public ICommand RunInjectorCmd { get; private set; }
        
        public ICommand RunAnalyzer { get; private set; }

        public ICommand PlotCommand { get; private set; }

        public ICommand ClearLastErrorCommand { get; private set; }

        #endregion // Public Properties

        #region Public Methods
        #endregion // Public Methods

        #region Private Methods

        #region Serial Communication

        /// <summary>
        /// Connects/disconnects with remote client over Serial
        /// </summary>
        private void ConnectSerialButtonPress()
        {
            if (ConnectSerialButtonContent == "Connect")
            {
                Debug.WriteLine($"Connecting to {ComPort}@{BaudRate}");

                _ = int.TryParse(BaudRate, out int baud);

                try
                {
                    SerialPort = new(ComPort, baud);

                    SerialPort.DataReceived += new SerialDataReceivedEventHandler(sPort_dataReceived);
                    //COMport.ErrorReceived += new SerialErrorReceivedEventHandler(sPort_ErrorReceived);

                    SerialPort.Parity = Parity.None;
                    SerialPort.DataBits = 8;
                    SerialPort.StopBits = StopBits.One;
                    SerialPort.RtsEnable = true;
                    SerialPort.Handshake = Handshake.None;

                    SerialPort.Open();

                    ConnSerialStatus = true;
                    ConnectSerialButtonContent = "Disconnect";
                    Debug.WriteLine($"Connected to {ComPort}@{BaudRate}");
                    
                    Thread.Sleep(1000);

                    SerialPort.WriteLine("echo Allspark | sudo -S /sn/bin/ublaze_mgr_cli -C1");
                }
                catch (Exception e) 
                {
                    ConnSerialStatus = false;
                    ConnectSerialButtonContent = "Connect";
                    Debug.WriteLine($"Connection failed to {ComPort}@{BaudRate}");
                    MessageBox.Show(e.Message); 
                };
            }
            else
            {
                SerialPort.WriteLine("echo Allspark | sudo -S /sn/bin/ublaze_mgr_cli -C2");
                Thread.Sleep(1000);
                SerialPort.Close();
                ConnectSerialButtonContent = "Connect";
                ConnSerialStatus = false;
                Debug.WriteLine($"Disconnected from {ComPort}@{BaudRate}");
            }
        }

        private void sPort_dataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Debug.WriteLine(SerialPort.ReadLine());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
            
            //MessageBox.Show(indata);
        }

        #endregion // Serial Communication

        #region SSH Communication

        /// <summary>
        /// Connects/disconnects with a remote client over SSH
        /// </summary>
        private void ConnectSshButtonPress()
        {
            // To establish a connection
            if (ConnectSshButtonContent == "Connect")
            {
                if (Session == null || Session.DisposedValue)
                {
                    Session = new SessionModel(HostIpAddress, Username, Password);

                    // if a valid SSH connection already exists
                    if (!Session.GetConnectionStatus())
                    {
                        // if remote client credentials are invalid
                        if (Session.IpAddress != null || Session.Username != null)
                        {
                            IsSshConnectable = true;
                            try
                            {
                                Debug.WriteLine($"Connecting {Session.IpAddress}");

                                Session.ConnectSsh();

                                // if connection established
                                if (Session.GetConnectionStatus())
                                {
                                    //Debug.WriteLine(Session.RunCommand("ls -l"));
                                    Debug.WriteLine($"Connected {Session.IpAddress}");
                                    
                                    // Changes button label 
                                    ConnectSshButtonContent = "Disconnect";
                                    ConnSshStatus = true;

                                    // Instantiate shell stream, stream reader and writer for first connection
                                    if (_shellStream == null && _sshReader == null && _sshWriter == null)
                                        CreateShellStream();
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Failed to connect to {Username}@{HostIpAddress}");
                                Debug.WriteLine(ex.Message);
                                //IsSshConnectable = false;
                                Session?.DisconnectSsh();
                                Session?.Dispose();
                                ConnectSshButtonContent = "Connect";
                                ConnSshStatus = false;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid Credentials");
                            Debug.WriteLine("Invalid Credentials");
                            //IsSshConnectable = false;
                        }                            
                    }
                    else
                    {
                        MessageBox.Show("Session already connected!");
                        Debug.WriteLine("Session already connected!");
                    }
                }
            }
            // To disconnect from a remote client
            else
            {
                Debug.WriteLine($"Disconnecting {Session.IpAddress}");

                if (_isRecording || _keepReading)
                    StopRecording();

                //Session?.DisconnectSsh();
                //Session?.Dispose();
                ConnectSshButtonContent = "Connect";
                ConnSshStatus = false;
                Debug.WriteLine($"Disconnected {Session.IpAddress}");
            }
        }

        private void RunAnalyzerButtonPress()
        {
            if (RunStopAnalyzerContent == "Run Counter Analyzer")
            {
                RunStopAnalyzerContent = "Stop Counter Analyzer";
            }
            else
            {
                RunStopAnalyzerContent = "Run Counter Analyzer";
            }
        }

        private async void RunInjectorButtonPress()
        {
            if (RunStopInjectorContent == "Run EMI Stress Test")
            {
                RunStopInjectorContent = "Stop EMI Stress Test";
                if (Session != null && Session.GetConnectionStatus())
                {
                    try
                    {
                        // reopen the shell stream if already closed
                        if (!_shellStream.CanWrite)
                            CreateShellStream();

                        // changes status indicators
                        CurrentErrorStatus = true;
                        IsLastErrorCleared = true;

                        _isRecording = true;
                        while (_isRecording)
                        {
                            //StartRecording("echo Allspark | sudo -S /home/allspark/loopback.sh");
                            StartRecording("echo Allspark | sudo -S /home/allspark/intertek.sh");

                            await Task.Delay(100);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        Debug.WriteLine(e.Message);
                    }
                }
                else
                    MessageBox.Show("Invalid Session");
            }
            else
            {
                RunStopInjectorContent = "EMI Stress Test";
                StopRecording();
            }
        }

        private void RunLoopbackButtonPress()
        {
            if (RunStopLoopbackContent == "Run EMI Stress Test")
            {
                RunStopLoopbackContent = "Stop EMI Stress Test";
                if (SerialPort != null && SerialPort.IsOpen)
                {
                    try
                    {
                        var thread = new Thread(RunThis);
                        //thread.Start("echo Allspark | sudo -S /home/allspark/beta_board_bringup_test/overlay_test/flyingpigs.sh");
                        thread.Start("echo Allspark | sudo -S /home/allspark/intertek.sh");
                    }
                    catch (Exception e) 
                    {
                        MessageBox.Show(e.Message); 
                        Debug.WriteLine(e.Message); 
                    }
                }
                else
                    MessageBox.Show("Invalid Session");
            }
            else
            {
                //SerialPort.WriteLine("sudo pkill vdma-out");
                //SerialPort.WriteLine("sudo pkill loopback");
                //SerialPort.WriteLine("echo Allspark | sudo -S /sn/bin/ublaze_mgr_cli -C2");
                Thread.Sleep(1000);

                //Session.RunCommand("echo Allspark | sudo -S killall -9 vdma-out");
                //Session.RunCommand("echo Allspark | sudo -S killall -9 loopback");
                RunStopLoopbackContent = "Run EMI Stress Test";
            }
        }

        //private void PlotButtonPress()
        //{

        //    for (int i = 0; i < _seconds.Count(); i++)
        //    {
        //        Points.Add(new Point(_seconds[i], _temps[i]));
        //    }
        //    //_timer = new DispatcherTimer();
        //    //_timer.Tick += Timer_Tick;
        //    //_timer.Interval = TimeSpan.FromMilliseconds(500);
        //    //_timer.Start();
        //    IsPlottable = true;
        //}

        private void ClearLastErrorButtonPress()
        {
            if (CurrentErrorStatus)
                IsLastErrorCleared = true;
        }

        private void RunThis(object? cmd)
        {
            try
            {
                SerialPort?.WriteLine((string)cmd);
            }
            catch
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Creates and starts a thread to write user command to remote SSH terminal
        /// </summary>
        /// <param name="cmd">user command as string</param>
        private void StartRecording(string cmd)
        {
            try
            {
                WriteStream(cmd);

                // Start a background thread that will read in the data from the Pyng terminal
                ThreadStart threadStart = ReceiveData;
                Thread thread = new Thread(threadStart) { IsBackground = true };
                thread.Start();
                _keepReading = true;
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

        /// <summary>
        /// Closes thread that writes to remote SSH terminal
        /// </summary>
        private void StopRecording()
        {
            _shellStream.Flush();
            _sshWriter.Flush();
            _sshReader.DiscardBufferedData();
            _sshStreamedResult.Clear();
            _fileWriter.Flush();
            _fileWriter.Close();

            _sshReader.BaseStream.Close();

            _isRecording = false;
            _keepReading = false;
        }

        /// <summary>
        /// Starts writing user command to remote SSH terminal
        /// </summary>
        /// <param name="cmd">user command as string</param>
        private void WriteStream(string cmd)
        {
            _sshWriter.WriteLine(cmd);
            while (_shellStream.Length == 0)
            {
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Contains thread to received and parse data from remote SSH terminal
        /// </summary>
        private void ReceiveData()
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
                            _fileWriter.WriteLine(line);

                            if (line.Contains("Value"))
                            {
                                Temperature = double.Parse(line.Substring(line.IndexOf("is ") + 3, 7));

                                Debug.Write($"{Temperature}");

                                CurrentErrorStatus = (Temperature <= _threshold);
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
        /// Creates instances of shell stream, stream reader and writer
        /// </summary>
        private void CreateShellStream()
        {
            _shellStream = Session?.SshClient.CreateShellStream(terminalName: "Terminal",
                                    columns: 80, rows: 60, width: 800, height: 600, bufferSize: 65536);

            _sshReader = new StreamReader(_shellStream, Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);

            _sshWriter = new StreamWriter(_shellStream) { AutoFlush = true };

            var filename = @"C:\Temp\RemoteHealth_Service.log";
            _fileWriter = new StreamWriter(filename, append:File.Exists(filename));
        }

        #endregion // SSH Communication

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
