using AuthenticationApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace AuthenticationApi.Infrastruture.Data
{
    public class AuthenticationDBContext(DbContextOptions<AuthenticationDBContext> options):DbContext(options)
    {
        public DbSet<AppUser> Users { get; set; }    
    }
}
