using System.Windows.Input;

namespace SshPoc
{
    internal class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly bool _canExecute = true;

        public RelayCommand(Action execute, bool canExecute = true)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute;

        public void Execute(object? parameter)
        {
            _execute();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
