using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BackEndWallet.Models;

namespace BackEndWallet.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
      
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }

        public DbSet<Usuarios> usuarios { get; set; }
        public DbSet<Billetera> billeteras { get; set; }
        public DbSet<Transacciones> transacciones { get; set; }

    }
}
