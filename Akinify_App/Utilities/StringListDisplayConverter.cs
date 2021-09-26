using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Akinify_App {

	[ValueConversion(typeof(string), typeof(string))]
	public class StringListDisplayConverter : IValueConverter {
        public StringListDisplayConverter() { }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			List<string> entry = value as List<string>;
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < entry.Count; i++) {
				if (i != 0) sb.Append(", ");
				sb.Append(entry[i]);
			}
			return sb.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
