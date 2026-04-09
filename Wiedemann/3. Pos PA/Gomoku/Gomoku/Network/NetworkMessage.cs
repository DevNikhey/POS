using Gomoku.shared.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku.Network
{
    public class NetworkMessage
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Player Player { get; set; }
    }
}
