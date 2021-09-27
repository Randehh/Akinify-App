using System;
using System.Windows.Input;

namespace Akinify_App {
	public class SimpleCommand : ICommand {
		private Action<object> m_OnExecute;
		private Func<bool> m_CanExecute;

		public SimpleCommand(Action<object> onExecute, Func<bool> canExecute = null) {
			m_OnExecute = onExecute;
			m_CanExecute = canExecute != null ? canExecute : () => { return true; };
		}

		public event EventHandler CanExecuteChanged {
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public bool CanExecute(object parameter) {
			return m_CanExecute();
		}

		public void Execute(object parameter) {
			m_OnExecute(parameter);
		}
	}
}
