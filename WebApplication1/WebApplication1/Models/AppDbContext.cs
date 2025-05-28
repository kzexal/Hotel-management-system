using System.Data.Entity;
using System.Web.Services.Description;

namespace WebApplication1.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("HotelDbConnection") { }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Login> Logins { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Login>().ToTable("Login", "Authentication");
            modelBuilder.Entity<Room>().ToTable("Room", "Rooms");
            modelBuilder.Entity<RoomType>().ToTable("RoomType", "Rooms");
            base.OnModelCreating(modelBuilder);
        }
    }

}
