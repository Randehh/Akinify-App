
namespace Akinify_App {
	public class BlendDisplayTable : DataDisplayTable {

		public BlendDisplayTable() : base() {
			CreateNewColumn("Blend name", "Name");
			CreateNewColumn("Users", "UserIds", new StringListDisplayConverter());
		}
	}
}
