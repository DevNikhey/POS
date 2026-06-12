using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DataModels;

namespace DataModels
{
    public partial class Waldwunder
    {
        public override string ToString()
        {
            return $"{Name} - {Description}";
        }
    }
}
