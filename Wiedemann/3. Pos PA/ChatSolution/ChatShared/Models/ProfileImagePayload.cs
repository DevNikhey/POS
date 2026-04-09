using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShared.Models
{
    public class ProfileImagePayload
    {
        public string Username { get; set; }
        public string ImageBase64 { get; set; }
    }
}
