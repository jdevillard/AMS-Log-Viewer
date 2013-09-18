using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AzureMobileServiceLogViewer.Command
{
    public class DelegateCommandBW : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        public event EventHandler CanExecuteChanged;

        private BackgroundWorker bgWorker;

        public DelegateCommandBW(Action<object> execute)
            : this(execute, null)
        {
        }

        public DelegateCommandBW(Action<object> execute,
                       Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
            bgWorker = new BackgroundWorker();
            bgWorker.DoWork += bgWorker_DoWork;
        }

        void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //throw new NotImplementedException();
            _execute(e.Argument);
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }


            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            bgWorker.RunWorkerAsync(parameter);
            //_execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }


    }
}
