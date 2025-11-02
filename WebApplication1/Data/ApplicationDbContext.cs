using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Student> Students {get; set;}
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<LockerRequest> LockerRequests { get; set; }    

        public DbSet<GatePassModel> GatePasses { get; set; }
    }
}
