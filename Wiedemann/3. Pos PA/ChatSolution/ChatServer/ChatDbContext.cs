using ChatShared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ChatServer
{
    public class ChatDbContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ChatRoom> ChatRoom {  get; set; }
        public DbSet<ChatRoomUser> ChatRoomUsers { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=ChatDatenbank.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatRoomUser>()
                .HasKey(x => new { x.ChatRoomId, x.UserId });

            modelBuilder.Entity<ChatRoomUser>()
            .HasOne(x => x.ChatRoom)
            .WithMany(x => x.ChatRoomUsers)
            .HasForeignKey(x => x.ChatRoomId);

            modelBuilder.Entity<ChatRoomUser>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId);
        }
    }
}
