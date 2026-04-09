using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShared.Models
{
    public class ChatPayload
    {
        public string Sender { get; set; } = "";
        public string Text { get; set; } = "";
        public string Room { get; set; } = "";
    }
}
