using System.Data.Entity;
using SmartStock.Models;

namespace SmartStock.Data
{
    public class SmartStockDbContext : DbContext
    {
        // matches the connection string name in Web.config
        public SmartStockDbContext() : base("name=SmartStockDbContext")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<StockIn> StockIns { get; set; }
        public DbSet<StockOut> StockOuts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // created the DB manually with our own SQL script,
            // so tell EF not to try to create/migrate it.
            Database.SetInitializer<SmartStockDbContext>(null);

            base.OnModelCreating(modelBuilder);
        }
    }
}