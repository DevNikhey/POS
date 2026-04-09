using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShared.Models
{
    public class ChatRoom
    {
        public int Id { get; set; } 
        public string Name { get; set; }

        public List<ChatRoomUser> ChatRoomUsers { get; set; }
    }
}
