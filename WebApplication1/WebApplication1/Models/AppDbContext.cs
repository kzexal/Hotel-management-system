using System.Data.Entity;

namespace WebApplication1.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("HotelDbConnection")
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Login> Logins { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<RoomBooked> RoomBooked { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServicesUsed> ServicesUsed { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Login>().ToTable("Login", "Authentication");
            modelBuilder.Entity<Room>().ToTable("Room", "Rooms");
            modelBuilder.Entity<RoomType>().ToTable("RoomType", "Rooms");
            modelBuilder.Entity<RoomBooked>().ToTable("RoomBooked", "Rooms");
            modelBuilder.Entity<Booking>().ToTable("Booking", "Bookings");
            modelBuilder.Entity<Payment>().ToTable("Payments", "Bookings");
            modelBuilder.Entity<Guest>().ToTable("Guests", "Hotel");
            modelBuilder.Entity<Service>().ToTable("Services", "HotelService");
            modelBuilder.Entity<ServicesUsed>().ToTable("ServicesUsed", "HotelService");

            base.OnModelCreating(modelBuilder);
        }
    }
}