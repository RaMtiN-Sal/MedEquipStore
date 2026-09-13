using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MedEquipStore.Models
{
    public class StoreDbContext : IdentityDbContext

    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options)
        {

        }


    }
}
