using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Akinify_App {
	public class BaseVM : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;		
		public void OnPropertyChanged([CallerMemberName] string name = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public void SetProperty<T>(ref T fieldReference, T newValue, [CallerMemberName] string propertyName = null) {
			if (!object.Equals(fieldReference, newValue)) {
				fieldReference = newValue;
				OnPropertyChanged(propertyName);
			}
		}
	}
}
