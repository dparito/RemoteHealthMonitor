using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SshPoc
{
    public class GpuStressViewModel: INotifyPropertyChanged
    {
        private SessionModel _session;
        private string _cmdResponse;
        private bool _connStatus;
        private bool _currentErrorStatus;
        private bool _isLastErrorCleared;

        #region Constructors

        public GpuStressViewModel()
        {
            CmdResponse = string.Empty;
            ConnStatus = false;
            CurrentErrorStatus = false;
            IsLastErrorCleared = true;
            RunCmd = new RelayCommand(RunCommandButtonPress);
        }

        public GpuStressViewModel(SessionModel session)
        {
            Session = session;
            CmdResponse = string.Empty;
            ConnStatus = false;
            CurrentErrorStatus = false;
            IsLastErrorCleared = true;
            RunCmd = new RelayCommand(RunCommandButtonPress);
        }

        #endregion // Constructors

        #region Properties

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

        public ICommand RunCmd { get; private set; }

        public SessionModel Session
        {
            get => _session;
            set => _session = value;
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
                    RaisePropertyChanged(nameof(CmdResponse));
                }
            }
        }

        #endregion // Properties

        #region Private Methods

        private void RunCommandButtonPress()
        {
            if (Session != null && Session.GetConnectionStatus())
            {
                ConnStatus = true;
                CurrentErrorStatus = false;
                IsLastErrorCleared = true;
                CmdResponse = Session.RunCommand("ls -l");
            }
        }

        #endregion // Private Methods

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Implementation
    }

}
