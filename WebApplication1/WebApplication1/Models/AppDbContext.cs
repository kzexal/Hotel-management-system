using System.Data.Entity;
using System.Web.UI.WebControls;

namespace WebApplication1.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("HotelDbConnection") { }

        public DbSet<Login> Logins { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Login>().ToTable("Login", "Authentication");
            base.OnModelCreating(modelBuilder);
        }
    }

}