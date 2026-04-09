using System;
using System.Windows.Controls;

namespace DreiRaucher
{
    internal class Raucher
    {
        private Table table;
        private RESOURCE unlimitedResource;
        private RESOURCE neededResource1;
        private RESOURCE neededResource2;
        private bool hasNeededResource1 = false;
        private bool hasNeededResource2 = false;
        private System.Windows.Controls.Label smokerResouceLabel;

        private int smokeTime;

        public Raucher(Table table, RESOURCE unlimitedResource, int smokeTime, Label smokerResouceLabel)
        {
            this.table = table;
            this.unlimitedResource = unlimitedResource;
            this.smokeTime = smokeTime;
            this.smokerResouceLabel = smokerResouceLabel;

            // Determine the needed resources based on the unlimited resource
            switch (unlimitedResource)
            {
                case RESOURCE.TABACCO:
                    neededResource1 = RESOURCE.PAPER;
                    neededResource2 = RESOURCE.FLINT_AND_STEEL;
                    break;
                case RESOURCE.PAPER:
                    neededResource1 = RESOURCE.TABACCO;
                    neededResource2 = RESOURCE.FLINT_AND_STEEL;
                    break;
                case RESOURCE.FLINT_AND_STEEL:
                    neededResource1 = RESOURCE.TABACCO;
                    neededResource2 = RESOURCE.PAPER;
                    break;
            }
        }

        public void runThread()
        {
            while (true)
            {
                smoke();
            }
        }

        public void smoke()
        {
            lock(table)
            {
                List<RESOURCE> resourcesOnTable = table.getResources();

                // take the resource if the need one, max 1
                foreach (var resource in resourcesOnTable)
                {
                    if (resource == neededResource1 && !hasNeededResource1)
                    {
                        hasNeededResource1 = true;
                        table.RemoveResource(neededResource1);
                        break;
                    }
                    else if (resource == neededResource2 && !hasNeededResource2)
                    {
                        hasNeededResource2 = true;
                        table.RemoveResource(neededResource2);
                        break;
                    }
                }

                // draw to resourceLabel which resource the smoker is missing
                smokerResouceLabel.Dispatcher.Invoke(() =>
                {
                    if (!hasNeededResource1 && !hasNeededResource2)
                        smokerResouceLabel.Content = $"Missing: {neededResource1} and {neededResource2}.";
                    else if (!hasNeededResource1)
                        smokerResouceLabel.Content = $"Missing: {neededResource1}.";
                    else if (!hasNeededResource2)
                        smokerResouceLabel.Content = $"Missing: {neededResource2}.";
                    else
                        smokerResouceLabel.Content = $"Missing: nothing";
                });

                Monitor.PulseAll(table);
                trySmoke();
            }
        }

        public void trySmoke()
        {
            // if the smoker has all resources needed, then smoke and remove all again
            if (hasNeededResource1 && hasNeededResource2)
            {
                smokerResouceLabel.Dispatcher.Invoke(() =>
                {
                    smokerResouceLabel.Content = "Smoking...";
                });
                hasNeededResource1 = false;
                hasNeededResource2 = false;

                Thread.Sleep(smokeTime);
            }
        }

        public RESOURCE getUnlimitedResource()
        {
            return unlimitedResource;
        }
    }
}
