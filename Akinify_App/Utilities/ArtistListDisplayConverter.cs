using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Akinify_App {

	[ValueConversion(typeof(string), typeof(string))]
	public class ArtistListDisplayConverter : IValueConverter {
        public ArtistListDisplayConverter() { }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			List<SimpleArtist> artists = value as List<SimpleArtist>;
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < artists.Count; i++) {
				if (i != 0) sb.Append(", ");
				sb.Append(artists[i].Name);
			}
			return sb.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
