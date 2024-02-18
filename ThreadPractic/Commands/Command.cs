using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Whatsapp.Commands
{
    public class Command : ICommand
    {
        private ICommand? myStatusCommand;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public Command(Action<object> action, Predicate<object> predicat = null)
        {
            this.action = action;
            this.predicat = predicat;
            if (this.predicat == null)
                this.predicat = (obj) => true;
        }

        public Command(ICommand? myStatusCommand)
        {
            this.myStatusCommand = myStatusCommand;
        }

        public Action<object>? action { get; set; }
        public Predicate<object>? predicat { get; set; }

        public bool CanExecute(object? parameter)
        {
            return predicat?.Invoke(parameter!) ?? false;
        }

        public void Execute(object? parameter)
        {
            action?.Invoke(parameter!);
        }
    }
}
