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

        protected void CreateNewColumn(string name, string path, IValueConverter valueConverter = null) {
            DataGridTextColumn column = new DataGridTextColumn();
            column.Header = name;
            column.Binding = new Binding(path) { Converter = valueConverter };
            column.Width = new DataGridLength(10, DataGridLengthUnitType.Star);
            ListViewParent.Columns.Add(column);
        }
    }
}
