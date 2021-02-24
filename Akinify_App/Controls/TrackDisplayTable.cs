namespace Akinify_App {
	public class TrackDisplayTable : DataDisplayTable {

		public TrackDisplayTable() : base() {
			CreateNewColumn("Name", "Name");
			CreateNewColumn("Artist(s)", "Artists", new ArtistListDisplayConverter());
			CreateNewColumn("Album", "Album.Name");
			CreateNewColumn("Popularity", "Popularity");
		}
	}
}
