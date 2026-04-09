using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreiRaucher
{
    internal class Dealer
    {
        private Table table;
        public Dealer(Table table)
        {
            this.table = table;
        }


        public void Run()
        {
            while (true)
            {
                PlaceResourcesOnTable();
            }
        }
        public void PlaceResourcesOnTable()
        {
            lock(table)
            {
                if(table.getResources().Count == 0)
                {
                    table.clearResource();
                    // Put two random resources on the table
                    Random rand = new Random();
                    RESOURCE firstResource = (RESOURCE)rand.Next(0, 3);
                    RESOURCE secondResource = (RESOURCE)rand.Next(0, 3);
                    while (secondResource == firstResource)
                    {
                        secondResource = (RESOURCE)rand.Next(0, 3);
                    }
                    table.AddResource(firstResource);
                    table.AddResource(secondResource);
                    Debug.WriteLine($"Dealer placed {firstResource} and {secondResource} on the table.");
                }

                Monitor.PulseAll(table);
            }
        }
    }
}
