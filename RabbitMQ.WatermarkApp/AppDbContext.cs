using Microsoft.EntityFrameworkCore;
using RabbitMQ.WatermarkApp.Models;

namespace RabbitMQ.WatermarkApp
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {
            
        }
        public DbSet<Product> Products { get; set; }
    }
}
