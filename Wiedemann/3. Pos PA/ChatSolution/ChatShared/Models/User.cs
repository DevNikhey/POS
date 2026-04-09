using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShared.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Color { get; set; }
        public string? ProfileImageBase64 { get; set; }

        public List<ChatRoomUser> ChatRoomUsers { get; set; }
    }
}
