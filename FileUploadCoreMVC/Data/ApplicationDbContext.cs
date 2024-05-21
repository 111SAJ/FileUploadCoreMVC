using FileUploadCoreMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace FileUploadCoreMVC.Data
{
    public class ApplicationDbContext : DbContext
    {
        //Model
        public DbSet<Employee> Employee { get; set; }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
    }
}
