using System;
using System.Windows.Input;

namespace MongoBackupManager
{
    public class BaseCommand<T> : ICommand
    {
        readonly Action<T> callback;

        public BaseCommand(Action<T> callback)
        {
            this.callback = callback;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (callback != null) { callback((T)parameter); }
        }
    }
}
