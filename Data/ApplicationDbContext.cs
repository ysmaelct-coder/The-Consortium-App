using Microsoft.EntityFrameworkCore;
using TheConsortiumApp.Models;

namespace TheConsortiumApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Usamos '= null!;' para decirle al compilador: 
        // "Confía en mí, Entity Framework inicializará esto, no es null"
        public DbSet<Empresa> Empresas { get; set; } = null!;
        public DbSet<Consorcio> Consorcios { get; set; } = null!;
        public DbSet<UnidadFuncional> UnidadesFuncionales { get; set; } = null!;
        public DbSet<Gasto> Gastos { get; set; } = null!;
        public DbSet<LiquidacionExpensa> LiquidacionesExpensa { get; set; } = null!;
        public DbSet<LiquidacionExpensaDetalle> LiquidacionesExpensaDetalle { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UnidadFuncional>()
                .Property(u => u.Coeficiente)
                .HasColumnType("decimal(18, 4)");
            modelBuilder.Entity<LiquidacionExpensa>()
    .Property(x => x.TotalGastos)
    .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<LiquidacionExpensaDetalle>()
    .HasOne(d => d.UnidadFuncional)
    .WithMany()
    .HasForeignKey(d => d.UnidadFuncionalId)
    .OnDelete(DeleteBehavior.Restrict); // 🔥 ESTA ES LA CLAVE

        }

    }
}