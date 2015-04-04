using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OCRApp.ViewModel
{
    public class BaseCommand : ICommand
    {
        Func<object, bool> _CanExecute;
        Action<object> _Execute;
        public BaseCommand(Action<object> execute)
        {   
            _Execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _Execute(parameter);
        }

        public void RaiseCanExecutechanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, null);
            }
        }

    }
}
