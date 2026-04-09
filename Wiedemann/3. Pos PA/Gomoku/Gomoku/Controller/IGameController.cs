using Gomoku.shared.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku.Controller
{
    public interface IGameController
    {
        void MakeMove(Cell cell);
    }
}
