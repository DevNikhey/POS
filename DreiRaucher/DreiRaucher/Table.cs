using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DreiRaucher
{
    internal class Table
    {
        private List<RESOURCE> resourcesOnTable;
        private Label tableResourceLabel;

        public Table(Label tableResourceLabel)
        {
            this.tableResourceLabel = tableResourceLabel;
            resourcesOnTable = new List<RESOURCE>();
        }

        public void AddResource(RESOURCE resource)
        {
            resourcesOnTable.Add(resource);
            redraw();
        }

        public void RemoveResource(RESOURCE resource)
        {
            resourcesOnTable.Remove(resource);
            redraw();
        }

        private void redraw()
        {
            tableResourceLabel.Dispatcher.Invoke(() =>
            {
                tableResourceLabel.Content = "Table: " + string.Join(", ", resourcesOnTable);
            });
        }

        public void clearResource()
        {
            this.resourcesOnTable.Clear();
        }

        public List<RESOURCE> getResources()
        {
            return resourcesOnTable;
        }
    }
}
