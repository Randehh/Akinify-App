namespace Akinify_App {
	public interface ISearchQuery {
		string SearchText { get; set; }
		void Search();
		bool CanGeneratePlaylist { get; }
	}
}
