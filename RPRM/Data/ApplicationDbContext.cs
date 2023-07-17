using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RPRM.Models.Metiers;
using RPRM.Models.User;

namespace RPRM.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        public DbSet<Pays> Pays { get; set; }
        public DbSet<Groupe> Groupe { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<LookupTable> LookupTable { get; set; }
        public DbSet<Operateurs> operateurs { get; set; }
        public DbSet<ServiceOuvert> serviceOuverts { get; set; }
        public DbSet<Contact> contacts { get; set; }
        public DbSet<Incident> incidents { get; set; }
        public DbSet<SimSent> simSents { get; set; }
        public DbSet<SimReceived> simReceiveds { get; set; }
        public DbSet<Tarif> tarifs { get; set; }
        public DbSet<TestUnit> testUnits { get; set; }
        public DbSet<DocOperateur> docOperateurs { get; set; }
        public DbSet<NetworkInfo> networkInfo { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
    }
}