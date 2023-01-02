using Microsoft.EntityFrameworkCore;
using Register_Login_Otp.Model;
using System.Collections.Generic;

namespace Register_Login_Otp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Register> Registers  { get; set; }

    }
}
