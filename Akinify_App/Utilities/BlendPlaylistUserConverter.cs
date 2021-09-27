using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Akinify_App {

	[ValueConversion(typeof(string), typeof(string))]
	public class BlendPlaylistUserConverter : IValueConverter {
        public BlendPlaylistUserConverter() { }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			ObservableCollection<BlendPlaylistUser> users = value as ObservableCollection<BlendPlaylistUser>;
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < users.Count; i++) {
				if (i != 0) sb.Append(", ");
				sb.Append(users[i].UserId);
			}
			return sb.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
