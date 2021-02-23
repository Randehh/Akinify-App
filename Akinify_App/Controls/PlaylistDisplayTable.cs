namespace Akinify_App {
	public class PlaylistDisplayTable : DataDisplayTable {

		public PlaylistDisplayTable() : base() {
			CreateNewColumn("Playlist name", "Name");
			CreateNewColumn("Author", "Owner.DisplayName");
		}
	}
}
