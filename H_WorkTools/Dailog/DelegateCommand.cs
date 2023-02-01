using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace H_WorkTools.Dailog
{
    public class DelegateCommand : ICommand
    {
        public DelegateCommand(Action<object> action)
        {
            _action = action;
        }
        private Action<object> _action;
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke(parameter);
        }
    }
}
