using BulkyBookWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookWeb.Data
{
    public class ApplicationDbContext :DbContext
    {
        // allows the configuration of DbContext
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // creates a Category table in the database
        public DbSet<Category> Categories { get; set; }
    }
}
