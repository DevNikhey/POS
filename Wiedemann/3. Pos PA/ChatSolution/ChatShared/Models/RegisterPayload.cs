using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShared.Models
{
    public class RegisterPayload
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
