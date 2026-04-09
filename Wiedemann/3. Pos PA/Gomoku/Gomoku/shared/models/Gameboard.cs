using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku.shared.models
{
    public class Gameboard
    {
        public ObservableCollection<Cell> Cells { get; set; }
        public Gameboard(int size) 
        {
            Cells = new ObservableCollection<Cell>();
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Cells.Add(new Cell { X = x, Y = y });
                }
            }
        }

        public Cell? GetCell(int x, int y)
        {
            return Cells.FirstOrDefault(c => c.X == x && c.Y == y);
        }
    }
}
