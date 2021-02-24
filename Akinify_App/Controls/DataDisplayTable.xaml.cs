using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Akinify_App
{
    public partial class DataDisplayTable : UserControl
    {
        public DataDisplayTable()
        {
            InitializeComponent();
        }

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
			base.OnRenderSizeChanged(sizeInfo);
            ResizeColumns();
        }

        protected void CreateNewColumn(string name, string path, IValueConverter valueConverter = null) {
            GridViewColumn column = new GridViewColumn();
            column.Header = name;
            column.DisplayMemberBinding = new Binding(path) { Converter = valueConverter };
            GridViewEntries.Columns.Add(column);
        }

        protected void ResizeColumns() {
            GridViewColumnCollection columns = GridViewEntries.Columns;
            int count = columns.Count;
            int gridWidth = (int)(ListViewParent.ActualWidth);
            int columnWidth = gridWidth / count;
            foreach(GridViewColumn column in GridViewEntries.Columns) {
                column.Width = columnWidth;
			}
        }
    }
}
