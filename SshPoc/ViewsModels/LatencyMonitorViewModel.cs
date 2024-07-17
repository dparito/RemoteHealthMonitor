using Renci.SshNet;
using System.ComponentModel;
using System.Data;
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

        // Configuration
        private string _allsparkUsername;
        private string _allsparkPassword;
        private string _allsparkHostIpAddr;
        private bool _isAllsparkConnectable = false;
        private string _connectAllsparkButtonContent;

        private string _jetsonUsername;
        private string _jetsonPassword;
        private string _jetsonHostIpAddr;
        private bool _isJetsonConnectable = false;
        private string _connectJetsonButtonContent;
        
        private string _baudRate;
        private string _comPort;
        private bool _isSerialConnectable = false;
        private bool _connSerialStatus;
        private string _connectSerialButtonContent;

        // GPU Burn Test
        private string _runStopGpuBurnTestContent;
        
        // ASAPP Test
        private string _runStopAsappTestContent;
        
        // Wi-Fi Flooding Test
        private string _runStopWiFiFloodingTestContent;

        // Latency Test
        private string _runStopLoopbackContent;
        private string _runStopInjectorContent;
        private string _runStopAnalyzerContent;

        #endregion // Private Members

        #region Constructors

        public LatencyMonitorViewModel()
        {
            // Configuration
            AllsparkUsername = "allspark";
            AllsparkHostIpAddress = string.Empty;
            AllsparkPassword = "Allspark";
            IsAllsparkConnectable = false;
            
            JetsonUsername = "allspark";
            JetsonHostIpAddress = string.Empty;
            JetsonPassword = "Allspark";
            IsJetsonConnectable = false;

            BaudRate = "115200";
            ComPort = string.Empty;
            IsSerialConnectable = false;
            ConnSerialStatus = false;
            
            ConnectSerialButtonContent = "Connect";
            ConnectAllsparkButtonContent = "Connect";
            ConnectJetsonButtonContent = "Connect";
            ConnectAllsparkCommand = new RelayCommand(ConnectAllsparkButtonPress);
            ConnectJetsonCommand = new RelayCommand(ConnectJetsonButtonPress);
            ConnectSerialCommand = new RelayCommand(ConnectSerialButtonPress);

            // GPU Burn Test
            GpuBurnSession = new SshSessionModel();
            RunStopGpuBurnTestContent = "Run GPU Burn Test";
            RunGpuBurnTestCommand = new RelayCommand(RunGpuBurnTestButtonPress);
            ClearLastErrorCommandGpu = new RelayCommand(ClearLastErrorButtonPressGpu);

            // ASAPP Test
            AsappSession = new SshSessionModel();
            RunStopAsappTestContent = "Run ASAPP Test";
            RunAsappTestCommand = new RelayCommand(RunAsappTestButtonPress);
            ClearLastErrorCommandAsapp = new RelayCommand(ClearLastErrorButtonPressAsapp);

            // Wi-Fi Flooding Test
            WifiPingSession = new SshSessionModel();
            RunStopWiFiFloodingTestContent = "Run Wi-Fi Flooding Test";
            RunWiFiFloodTestCommand = new RelayCommand(RunWiFiFloodTestButtonPress);
            ClearLastErrorCommandWiFi = new RelayCommand(ClearLastErrorButtonPressWiFi);

            // Latency Test
            LatencySessionOnAllspark = new SshSessionModel();
            LatencySessionOnJetson = new SshSessionModel();
            RunStopLoopbackContent = "Run Loopback";
            RunStopInjectorContent = "Run Counter Injector";
            RunStopAnalyzerContent = "Run Counter Analyzer";
            RunLoopbackCommand = new RelayCommand(RunLoopbackButtonPress);
            RunInjectorCommand = new RelayCommand(RunInjectorButtonPress);
            RunAnalyzerCommand = new RelayCommand(RunAnalyzerButtonPress);
            ClearLastErrorCommandLatency = new RelayCommand(ClearLastErrorButtonPressLatency);
        }

        #endregion // Constructors

        #region Configuration Properties
        
        public string AllsparkUsername
        {
            get => _allsparkUsername;
            set
            {
                if (_allsparkUsername != value)
                {
                    _allsparkUsername = value;
                    OnPropertyChanged(nameof(AllsparkUsername));

                    IsAllsparkConnectable = (AllsparkUsername != string.Empty && AllsparkPassword != string.Empty && AllsparkHostIpAddress != string.Empty);
                }
            }
        }

        public string AllsparkPassword
        {
            get => _allsparkPassword;
            set
            {
                if (_allsparkPassword != value)
                {
                    _allsparkPassword = value;
                    OnPropertyChanged(nameof(AllsparkPassword));

                    IsAllsparkConnectable = (AllsparkUsername != string.Empty && AllsparkPassword != string.Empty && AllsparkHostIpAddress != string.Empty);
                }
            }
        }

        public string AllsparkHostIpAddress
        {
            get => _allsparkHostIpAddr;
            set
            {
                if (_allsparkHostIpAddr != value)
                {
                    _allsparkHostIpAddr = value;
                    OnPropertyChanged(nameof(AllsparkHostIpAddress));

                    IsAllsparkConnectable = (AllsparkUsername != string.Empty && AllsparkPassword != string.Empty && AllsparkHostIpAddress != string.Empty);
                }
            }
        }

        public bool IsAllsparkConnectable
        {
            get => _isAllsparkConnectable;
            set
            {
                if (value !=  _isAllsparkConnectable) 
                {
                    _isAllsparkConnectable = value;
                    OnPropertyChanged(nameof(IsAllsparkConnectable));
                }
            }
        }

        public string ConnectAllsparkButtonContent
        {
            get => _connectAllsparkButtonContent;
            set
            {
                _connectAllsparkButtonContent = value;
                OnPropertyChanged(nameof(ConnectAllsparkButtonContent));
            }
        }
        
        public ICommand ConnectAllsparkCommand { get; private set; }


        public string JetsonUsername
        {
            get => _jetsonUsername;
            set
            {
                if (_jetsonUsername != value)
                {
                    _jetsonUsername = value;
                    OnPropertyChanged(nameof(JetsonUsername));

                    IsJetsonConnectable = (JetsonUsername != string.Empty && JetsonPassword != string.Empty && JetsonHostIpAddress != string.Empty);
                }
            }
        }

        public string JetsonPassword
        {
            get => _jetsonPassword;
            set
            {
                if (_jetsonPassword != value)
                {
                    _jetsonPassword = value;
                    OnPropertyChanged(nameof(JetsonPassword));

                    IsJetsonConnectable = (JetsonUsername != string.Empty && JetsonPassword != string.Empty && JetsonHostIpAddress != string.Empty);
                }
            }
        }

        public string JetsonHostIpAddress
        {
            get => _jetsonHostIpAddr;
            set
            {
                if (_jetsonHostIpAddr != value)
                {
                    _jetsonHostIpAddr = value;
                    OnPropertyChanged(nameof(JetsonHostIpAddress));

                    IsJetsonConnectable = (JetsonUsername != string.Empty && JetsonPassword != string.Empty && JetsonHostIpAddress != string.Empty);
                }
            }
        }

        public bool IsJetsonConnectable
        {
            get => _isJetsonConnectable;
            set
            {
                if (value != _isJetsonConnectable)
                {
                    _isJetsonConnectable = value;
                    OnPropertyChanged(nameof(IsJetsonConnectable));
                }
            }
        }

        public string ConnectJetsonButtonContent
        {
            get => _connectJetsonButtonContent;
            set
            {
                _connectJetsonButtonContent = value;
                OnPropertyChanged(nameof(ConnectJetsonButtonContent));
            }
        }
        
        public ICommand ConnectJetsonCommand { get; private set; }

        
        public SerialPort? SerialPort { get; private set; }

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

        public string ConnectSerialButtonContent
        {
            get => _connectSerialButtonContent;
            set
            {
                _connectSerialButtonContent = value;
                OnPropertyChanged(nameof(ConnectSerialButtonContent));
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
        
        public bool ConnSerialStatus
        {
            get => _connSerialStatus;
            set
            {
                _connSerialStatus = value;
                OnPropertyChanged(nameof(ConnSerialStatus));
            }
        }

        public ICommand ConnectSerialCommand { get; private set; }        

        #endregion // Configuration Properties


        #region GPU Burn Test Properties

        //public SshSessionModel? GpuBurnSession { get; private set; }
        public GpuBurnTestSessionModel? GpuBurnSession { get; private set; }

        public string RunStopGpuBurnTestContent
        {
            get => _runStopGpuBurnTestContent;
            set
            {
                _runStopGpuBurnTestContent = value;
                OnPropertyChanged(nameof(RunStopGpuBurnTestContent)); 
            }
        }

        public ICommand RunGpuBurnTestCommand { get; private set; }

        public ICommand ClearLastErrorCommandGpu { get; private set; }

        #endregion // GPU Burn Test Properties

        
        #region ASAPP Test Properties

        public SshSessionModel? AsappSession { get; private set; }

        public string RunStopAsappTestContent
        {
            get => _runStopAsappTestContent;
            set
            {
                _runStopAsappTestContent = value;
                OnPropertyChanged(nameof(RunStopAsappTestContent));
            }
        }

        public ICommand RunAsappTestCommand { get; private set; }

        public ICommand ClearLastErrorCommandAsapp { get; private set; }

        #endregion // ASAPP Test Properties

        
        #region Wi-Fi Flooding Test Properties

        public SshSessionModel? WifiPingSession { get; private set; }

        public string RunStopWiFiFloodingTestContent
        {
            get => _runStopWiFiFloodingTestContent;
            set
            {
                _runStopWiFiFloodingTestContent = value;
                OnPropertyChanged(nameof(RunStopWiFiFloodingTestContent));
            }
        }

        public ICommand RunWiFiFloodTestCommand { get; private set; }

        public ICommand ClearLastErrorCommandWiFi { get; private set; }

        #endregion  // Wi-Fi Flooding Test Properties

        
        #region Latency Test Properties

        public SshSessionModel? LatencySessionOnJetson { get; private set; }
        
        public SshSessionModel? LatencySessionOnAllspark { get; private set; }

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

        public ICommand ClearLastErrorCommandLatency { get; private set; }

        public ICommand RunLoopbackCommand { get; private set; }

        public ICommand RunInjectorCommand { get; private set; }

        public ICommand RunAnalyzerCommand { get; private set; }

        #endregion // Latency Test Properties


        #region Serial Communication

        /// <summary>
        /// Connects/disconnects with remote client over Serial
        /// </summary>
        private void ConnectSerialButtonPress()
        {
            if (ConnectSerialButtonContent == "Connect")
            {
                if (ComPort != string.Empty && BaudRate != string.Empty)
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
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Credentials");
                    ConnSerialStatus = false;
                }
            }
            else
            {
                //SerialPort.WriteLine("echo Allspark | sudo -S /sn/bin/ublaze_mgr_cli -C2");
                //Thread.Sleep(1000);
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


        #endregion // Serial Communication

        #region SSH Communication
        
        /// <summary>
        /// Connects/disconnects with a remote client over SSH
        /// </summary>
        private void ConnectAllsparkButtonPress()
        {
            // To establish a connection
            if (ConnectAllsparkButtonContent == "Connect")
            {
                //TODO: add user input validation
                
                ConnectGpuBurnSession();
                ConnectAsappSession();
                ConnectWiFiPingSession();
                ConnectLatencySessionOnAllspark();

                // Changes button label 
                ConnectAllsparkButtonContent = (GpuBurnSession.ConnStatus
                                                && AsappSession.ConnStatus
                                                && WifiPingSession.ConnStatus
                                                && LatencySessionOnAllspark.ConnStatus) ? "Disconnect" : "Connect";
            }
            // To disconnect from a remote client
            else
            {
                Debug.WriteLine($"Disconnected GPU Test Session");
                Debug.WriteLine($"Disconnected ASAPP Test Session");
                Debug.WriteLine($"Disconnected Wi-Fi Flood Session");
                Debug.WriteLine($"Disconnected Allspark Latency Session");

                GpuBurnSession?.DisconnectSsh();
                GpuBurnSession?.Dispose();

                AsappSession?.DisconnectSsh();
                AsappSession?.Dispose();

                WifiPingSession?.DisconnectSsh();
                WifiPingSession?.Dispose();

                LatencySessionOnAllspark?.DisconnectSsh();
                LatencySessionOnAllspark?.Dispose();

                ConnectAllsparkButtonContent = "Connect";
            }
        }

        /// <summary>
        /// Connects/disconnects with a remote client over SSH
        /// </summary>
        private void ConnectJetsonButtonPress()
        {
            // To establish a connection
            if (ConnectJetsonButtonContent == "Connect")
            {
                //TODO: add user input validation

                ConnectLatencySessionOnJetson();
                ConnectJetsonButtonContent = "Disconnect";
            }
            // To disconnect from a remote client
            else
            {
                Debug.WriteLine($"Disconnecting {LatencySessionOnJetson?.IpAddress}");

                LatencySessionOnJetson?.DisconnectSsh();
                LatencySessionOnJetson?.Dispose();
                
                ConnectJetsonButtonContent = "Connect";
                
                Debug.WriteLine($"Disconnected {LatencySessionOnJetson?.IpAddress}");
            }
        }

        private bool ConnectGpuBurnSession()
        {
            if (GpuBurnSession == null || GpuBurnSession.DisposedValue)
                GpuBurnSession = new SshSessionModel(AllsparkHostIpAddress, AllsparkUsername, AllsparkPassword, 5003);
            else
            {
                GpuBurnSession.Username = AllsparkUsername;
                GpuBurnSession.Password = AllsparkPassword;
                GpuBurnSession.IpAddress = AllsparkHostIpAddress;
                GpuBurnSession.PortNum = 5003;
            }

            try
            {
                Debug.WriteLine($"Connecting {GpuBurnSession.IpAddress} for {nameof(GpuBurnSession)}");

                GpuBurnSession.ConnectSsh(SshSessionModel.TestType.GpuBurn);

                // if connection established
                if (GpuBurnSession.GetConnectionStatus())
                {
                    //Debug.WriteLine(Session.StartRecording("ls -l"));
                    Debug.WriteLine($"Connected {GpuBurnSession.IpAddress} for {nameof(GpuBurnSession)}");

                    return true;
                }
                else
                    return false;
            }
            // Failed to connect 
            catch (Exception ex)
            {
                GpuBurnSession?.DisconnectSsh();
                GpuBurnSession?.Dispose();

                MessageBox.Show($"Failed to connect to {AllsparkUsername}@{AllsparkHostIpAddress}");
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        private bool ConnectAsappSession()
        {
            if (AsappSession == null || AsappSession.DisposedValue)
                AsappSession = new SshSessionModel(AllsparkHostIpAddress, AllsparkUsername, AllsparkPassword, 5003);
            else
            {
                AsappSession.Username = AllsparkUsername;
                AsappSession.Password = AllsparkPassword;
                AsappSession.IpAddress = AllsparkHostIpAddress;
                AsappSession.PortNum = 5003;
            }

            try
            {
                Debug.WriteLine($"Connecting {AsappSession.IpAddress} for {nameof(AsappSession)}");

                AsappSession.ConnectSsh(SshSessionModel.TestType.Asapp);

                // if connection established
                if (AsappSession.GetConnectionStatus())
                {
                    //Debug.WriteLine(Session.StartRecording("ls -l"));
                    Debug.WriteLine($"Connected {AsappSession.IpAddress} for {nameof(AsappSession)}");

                    return true;
                }
                else
                    return false;
            }
            // Failed to connect 
            catch (Exception ex)
            {
                AsappSession?.DisconnectSsh();
                AsappSession?.Dispose();

                MessageBox.Show($"Failed to connect to {AllsparkUsername}@{AllsparkHostIpAddress}");
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        private bool ConnectWiFiPingSession()
        {
            if (WifiPingSession == null || WifiPingSession.DisposedValue)
                WifiPingSession = new SshSessionModel(AllsparkHostIpAddress, AllsparkUsername, AllsparkPassword, 5003);
            else
            {
                WifiPingSession.Username = AllsparkUsername;
                WifiPingSession.Password = AllsparkPassword;
                WifiPingSession.IpAddress = AllsparkHostIpAddress;
                WifiPingSession.PortNum = 5003;
            }

            try
            {
                Debug.WriteLine($"Connecting {WifiPingSession.IpAddress} for {nameof(WifiPingSession)}");

                WifiPingSession.ConnectSsh(SshSessionModel.TestType.WiFiFlooding);

                // if connection established
                if (WifiPingSession.GetConnectionStatus())
                {
                    //Debug.WriteLine(Session.StartRecording("ls -l"));
                    Debug.WriteLine($"Connected {WifiPingSession.IpAddress} for {nameof(WifiPingSession)}");

                    return true;
                }
                else
                    return false;
            }
            // Failed to connect 
            catch (Exception ex)
            {
                WifiPingSession?.DisconnectSsh();
                WifiPingSession?.Dispose();

                MessageBox.Show($"Failed to connect to {AllsparkUsername}@{AllsparkHostIpAddress}");
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        private bool ConnectLatencySessionOnAllspark()
        {
            if (LatencySessionOnAllspark == null || LatencySessionOnAllspark.DisposedValue)
                LatencySessionOnAllspark = new SshSessionModel(AllsparkHostIpAddress, AllsparkUsername, AllsparkPassword, 5003);
            else
            {
                LatencySessionOnAllspark.Username = AllsparkUsername;
                LatencySessionOnAllspark.Password = AllsparkPassword;
                LatencySessionOnAllspark.IpAddress = AllsparkHostIpAddress;
                LatencySessionOnAllspark.PortNum = 5003;
            }

            try
            {
                Debug.WriteLine($"Connecting {LatencySessionOnAllspark.IpAddress} for {nameof(LatencySessionOnAllspark)}");

                LatencySessionOnAllspark.ConnectSsh(SshSessionModel.TestType.LatencyOnAllspark);

                // if connection established
                if (LatencySessionOnAllspark.GetConnectionStatus())
                {
                    //Debug.WriteLine(Session.StartRecording("ls -l"));
                    Debug.WriteLine($"Connected {LatencySessionOnAllspark.IpAddress} for {nameof(LatencySessionOnAllspark)}");

                    return true;
                }
                else
                    return false;
            }
            // Failed to connect 
            catch (Exception ex)
            {
                LatencySessionOnAllspark?.DisconnectSsh();
                LatencySessionOnAllspark?.Dispose();

                MessageBox.Show($"Failed to connect to {AllsparkUsername}@{AllsparkHostIpAddress}");
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        private bool ConnectLatencySessionOnJetson()
        {
            if (LatencySessionOnJetson == null || LatencySessionOnJetson.DisposedValue)
                LatencySessionOnJetson = new SshSessionModel(JetsonHostIpAddress, JetsonUsername, JetsonPassword, 22);
            else
            {
                LatencySessionOnJetson.Username = JetsonUsername;
                LatencySessionOnJetson.Password = JetsonPassword;
                LatencySessionOnJetson.IpAddress = JetsonHostIpAddress;
                LatencySessionOnJetson.PortNum = 22;
            }

            try
            {
                Debug.WriteLine($"Connecting {LatencySessionOnJetson.IpAddress} for {nameof(LatencySessionOnJetson)}");

                LatencySessionOnJetson.ConnectSsh(SshSessionModel.TestType.LatencyOnJetson);

                // if connection established
                if (LatencySessionOnJetson.GetConnectionStatus())
                {
                    //Debug.WriteLine(Session.StartRecording("ls -l"));
                    Debug.WriteLine($"Connected {LatencySessionOnJetson.IpAddress} for {nameof(LatencySessionOnJetson)}");

                    return true;
                }
                else
                    return false;
            }
            // Failed to connect 
            catch (Exception ex)
            {
                LatencySessionOnJetson?.DisconnectSsh();
                LatencySessionOnJetson?.Dispose();

                MessageBox.Show($"Failed to connect to {JetsonUsername}@{JetsonHostIpAddress}");
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
        
        private  void RunWiFiFloodTestButtonPress()
        {
            if (RunStopWiFiFloodingTestContent == "Run Wi-Fi Flooding Test")
            {
                RunStopWiFiFloodingTestContent = "Stop Wi-Fi Flooding Test";
                if (WifiPingSession != null && WifiPingSession.GetConnectionStatus())
                {
                    try
                    {
                        // reopen the shell stream if already closed
                        if (!WifiPingSession.ShellStream.CanWrite)
                            WifiPingSession.CreateShellStream(SshSessionModel.TestType.WiFiFlooding);

                        // changes status indicators
                        WifiPingSession.CurrentErrStatus = true;
                        WifiPingSession.IsLastErrorCleared = true;

                        WifiPingSession.IsRecording = true;
                        while (WifiPingSession.IsRecording)
                        {
                            //WifiPingSession.StartRecording($"echo {AllsparkPassword} | sudo ping -f -t 200 192.168.1.1", SshSessionModel.TestType.WiFiFlooding);
                            WifiPingSession.StartRecording($"ping -i 0.2 -s 65507 192.168.1.1", SshSessionModel.TestType.WiFiFlooding);
                            Thread.Sleep(100);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        Debug.WriteLine(e.Message);
                        WifiPingSession.IsRecording = false;
                    }
                }
                else
                    MessageBox.Show("Invalid Session");
            }
            else
            {
                WifiPingSession?.StopRecording();
                RunStopWiFiFloodingTestContent = "Run Wi-Fi Flooding Test";
            }
        }

        private  void RunAsappTestButtonPress()
        {
            if (RunStopAsappTestContent == "Run ASAPP Test")
            {
                RunStopAsappTestContent = "Stop ASAPP Test";
                if (AsappSession != null && AsappSession.GetConnectionStatus())
                {
                    try
                    {
                        // reopen the shell stream if already closed
                        if (!AsappSession.ShellStream.CanWrite)
                            AsappSession.CreateShellStream(SshSessionModel.TestType.Asapp);

                        // changes status indicators
                        AsappSession.CurrentErrStatus = true;
                        AsappSession.IsLastErrorCleared = true;

                        AsappSession.IsRecording = true;
                        while (AsappSession.IsRecording)
                        {
                            AsappSession.StartRecording($"echo {AllsparkPassword} | sudo -S /home/allspark/serviceApps/asapp -O -M -T", SshSessionModel.TestType.Asapp);
                            Thread.Sleep(100);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        Debug.WriteLine(e.Message);
                        AsappSession.IsRecording = false;
                    }
                }
                else
                    MessageBox.Show("Invalid Session");
            }
            else
            {
                AsappSession?.StopRecording();
                RunStopAsappTestContent = "Run ASAPP Test";
            }
        }

        private  void RunGpuBurnTestButtonPress()
        {
            if (RunStopGpuBurnTestContent == "Run GPU Burn Test")
            {
                RunStopGpuBurnTestContent = "Stop GPU Burn Test";
                if (GpuBurnSession != null && GpuBurnSession.GetConnectionStatus())
                {
                    try
                    {
                        // reopen the shell stream if already closed
                        if (!GpuBurnSession.ShellStream.CanWrite)
                            GpuBurnSession.CreateShellStream(SshSessionModel.TestType.GpuBurn);

                        // changes status indicators
                        GpuBurnSession.CurrentErrStatus = true;
                        GpuBurnSession.IsLastErrorCleared = true;

                        GpuBurnSession.IsRecording = true;
                        while (GpuBurnSession.IsRecording)
                        {
                            GpuBurnSession.StartRecording($"echo {AllsparkPassword} | sudo -S /home/allspark/serviceApps/matrixMul", SshSessionModel.TestType.GpuBurn);
                            Thread.Sleep(100);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        Debug.WriteLine(e.Message);
                        GpuBurnSession.IsRecording = false;
                    }
                }
                else
                    MessageBox.Show("Invalid Session");
            }
            else
            {
                GpuBurnSession?.StopRecording();
                RunStopGpuBurnTestContent = "Run GPU Burn Test";
            }
        }

        private void ClearLastErrorButtonPressGpu() => GpuBurnSession?.ClearLastErrorButtonPress();
        
        private void ClearLastErrorButtonPressAsapp() => AsappSession?.ClearLastErrorButtonPress();
        
        private void ClearLastErrorButtonPressWiFi() => WifiPingSession?.ClearLastErrorButtonPress();

        private void ClearLastErrorButtonPressLatency() => LatencySessionOnJetson?.ClearLastErrorButtonPress();
        
        private  void RunAnalyzerButtonPress()
        {
            if (RunStopAnalyzerContent == "Run Counter Analyzer")
            {
                RunStopAnalyzerContent = "Stop Counter Analyzer";
                if (LatencySessionOnJetson != null && LatencySessionOnJetson.GetConnectionStatus())
                {
                    try
                    {
                        // reopen the shell stream if already closed
                        if (!LatencySessionOnJetson.ShellStream.CanWrite)
                            LatencySessionOnJetson.CreateShellStream(SshSessionModel.TestType.LatencyOnJetson);

                        // changes status indicators
                        LatencySessionOnJetson.CurrentErrStatus = true;
                        LatencySessionOnJetson.IsLastErrorCleared = true;

                        LatencySessionOnJetson.IsRecording = true;
                        while (LatencySessionOnJetson.IsRecording)
                        {
                            LatencySessionOnJetson.StartRecording($"cd /home/allspark/Prime/Prime-master/LatencyTester.jl", SshSessionModel.TestType.LatencyOnJetson);
                            Thread.Sleep(1000);
                            LatencySessionOnJetson.StartRecording($"julia --project ./counters_analyzer.jl /dev/video0 -s 1", SshSessionModel.TestType.LatencyOnJetson);
                            Thread.Sleep(100);
                            Thread.CurrentThread.IsBackground = true;
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        Debug.WriteLine(e.Message);
                        LatencySessionOnJetson.IsRecording = false;
                    }
                }
                else
                    MessageBox.Show("Invalid Session");
            }
            else
            {
                LatencySessionOnJetson.StopRecording();
                RunStopAnalyzerContent = "Run Counter Analyzer";
            }
        }

        private  void RunInjectorButtonPress()
        {
            if (RunStopInjectorContent == "Run Counter Injector")
            {
                RunStopInjectorContent = "Stop Counter Injector";
                if (LatencySessionOnJetson != null && LatencySessionOnJetson.GetConnectionStatus())
                {
                    try
                    {
                        // changes status indicators
                        LatencySessionOnJetson.CurrentErrStatus = true;
                        LatencySessionOnJetson.IsLastErrorCleared = true;

                        LatencySessionOnJetson.IsRecording = true;
                        while (LatencySessionOnJetson.IsRecording)
                        {
                            LatencySessionOnJetson.StartRecording($"cd /home/allspark/Prime/Prime-master/LatencyTester.jl", SshSessionModel.TestType.LatencyOnJetson);
                            Thread.Sleep(1000);
                            LatencySessionOnJetson.StartRecording($"export DISPLAY=:1", SshSessionModel.TestType.LatencyOnJetson);
                            Thread.Sleep(1000);
                            LatencySessionOnJetson.StartRecording($"julia --project ./counter_injector.jl /dev/video1 -s 1 > /dev/null &", SshSessionModel.TestType.LatencyOnJetson);
                            Thread.Sleep(10000);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        Debug.WriteLine(e.Message);
                        LatencySessionOnJetson.IsRecording = false;
                    }
                }
                else
                    MessageBox.Show("Invalid Session");
            }
            else
            {
                RunStopInjectorContent = "Run Counter Injector";
                LatencySessionOnJetson.StopRecording();
            }
        }

        private  void RunLoopbackButtonPress()
        {
            if (RunStopLoopbackContent == "Run Loopback")
            {
                RunStopLoopbackContent = "Stop Loopback";
                if (LatencySessionOnAllspark != null && LatencySessionOnAllspark.GetConnectionStatus())
                {
                    try
                    {
                        // changes status indicators
                        LatencySessionOnAllspark.CurrentErrStatus = true;
                        LatencySessionOnAllspark.IsLastErrorCleared = true;

                        LatencySessionOnAllspark.IsRecording = true;
                        while (LatencySessionOnAllspark.IsRecording)
                        {
                            Thread.CurrentThread.IsBackground = true;
                            // Loopback on ALLSPARK
                            LatencySessionOnAllspark.StartRecording("echo Allspark | sudo -S /sn/bin/ublaze_mgr_cli -C1", SshSessionModel.TestType.LatencyOnAllspark);
                            //Thread.Sleep(2000);
                            LatencySessionOnAllspark.StartRecording("echo Allspark | sudo -S /sn/bin/vdma-out > /dev/null &", SshSessionModel.TestType.LatencyOnAllspark);
                            //Thread.Sleep(3000);
                            LatencySessionOnAllspark.StartRecording("echo Allspark | sudo -S /sn/bin/new-loopback/vdma-in-loopback", SshSessionModel.TestType.LatencyOnAllspark);
                            Thread.Sleep(1000);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        Debug.WriteLine(e.Message);
                        LatencySessionOnAllspark.IsRecording = false;
                    }
                }
                else
                    MessageBox.Show("Invalid Session");
            }
            else
            {
                if (LatencySessionOnAllspark != null && LatencySessionOnAllspark.GetConnectionStatus())
                {
                    try
                    {
                        LatencySessionOnAllspark.StartRecording("sudo pkill -9 vdma-out", SshSessionModel.TestType.LatencyOnAllspark);
                        LatencySessionOnAllspark.StartRecording("sudo pkill -9 vdma-in-loopback", SshSessionModel.TestType.LatencyOnAllspark);
                        LatencySessionOnAllspark.StartRecording("echo Allspark | sudo -S /sn/bin/ublaze_mgr_cli -C2", SshSessionModel.TestType.LatencyOnAllspark);
                        LatencySessionOnAllspark?.StopRecording();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        Debug.WriteLine(ex.Message);
                    }

                    finally
                    {
                        Thread.Sleep(1000);

                        RunStopLoopbackContent = "Run Loopback";
                    }
                }
                else
                    RunStopLoopbackContent = "Run Loopback";
            }
        }
        
        #endregion // SSH Communication

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
