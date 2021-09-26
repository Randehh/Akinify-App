using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Akinify_App {
	public class BaseVM : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;		
		public void OnPropertyChanged([CallerMemberName] string name = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
