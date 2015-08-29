using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FlickrClient.Helpers
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _executeMethod = null;
        private readonly Predicate<object> _canExecuteMethod = null;
        private bool _isAutomaticRequeryDisabled = false;

        public DelegateCommand(Action<Object> executeMethod, Predicate<object> canExecuteMethod, bool isAutomaticRequeryDisabled)
        {
            if (executeMethod == null)
            {
                throw new ArgumentNullException("executeMethod is null. Please set executeMethod.");
            }
            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
            _isAutomaticRequeryDisabled = isAutomaticRequeryDisabled;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteMethod == null ? true : _canExecuteMethod(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (_executeMethod != null)
                _executeMethod(parameter);
        }

        //In WinRT, there is no CommandManager to raise events globally. you must update/raise CanExecuteChanged manually 
        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

    }

}
