using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Akinify_App {
	public interface IDataTableContext<T> : INotifyPropertyChanged {
		ObservableCollection<T> Items { get; }
		T SelectedItem { get; set; }
	}
}
