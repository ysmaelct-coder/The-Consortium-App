using Microsoft.EntityFrameworkCore;
using TheConsortiumApp.Models;

namespace TheConsortiumApp.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Empresa> Empresas { get; set; } = null!;
        public DbSet<Consorcio> Consorcios { get; set; } = null!;
        public DbSet<UnidadFuncional> UnidadesFuncionales { get; set; } = null!;
        public DbSet<Gasto> Gastos { get; set; } = null!;
        public DbSet<LiquidacionExpensa> LiquidacionesExpensa { get; set; } = null!;
        public DbSet<LiquidacionExpensaDetalle> LiquidacionesExpensaDetalle { get; set; } = null!;
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<ComprobantePago> ComprobantesPagos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UnidadFuncional>()
                .Property(u => u.Coeficiente)
                .HasColumnType("decimal(18, 4)");

            modelBuilder.Entity<LiquidacionExpensa>()
                .Property(x => x.TotalGastos)
                .HasColumnType("decimal(18,2)");

            // Relaciones y reglas de borrado existentes
            modelBuilder.Entity<LiquidacionExpensaDetalle>()
                .HasOne(d => d.UnidadFuncional)
                .WithMany()
                .HasForeignKey(d => d.UnidadFuncionalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ComprobantePago>()
                .HasKey(c => c.Id);
        }
    }
}