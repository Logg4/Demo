﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StockWatch.ViewModel
{
    public class RelayCommand<T> : ICommand
    {
        readonly Action<T> _execute;
        readonly Predicate<T> _canExecute;


        public RelayCommand(Action<T> execute)
        : this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }

}
