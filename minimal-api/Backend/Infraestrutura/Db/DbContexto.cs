using Microsoft.EntityFrameworkCore;
using Backend.Dominio.Entidades;
using Backend.Dominio.Enuns;
using System.Reflection;

namespace Backend.Infraestrutura.Db;

public class DbContexto : DbContext
{
    public DbContexto(DbContextOptions<DbContexto> options)
        : base(options)
    {
    }

    public DbSet<Administrador> Administradores { get; set; } = default!;
    public DbSet<Veiculo> Veiculos { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Aplica todas as configurações de entidade (IEntityTypeConfiguration<T>)
        // que estão neste mesmo projeto (assembly).
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
