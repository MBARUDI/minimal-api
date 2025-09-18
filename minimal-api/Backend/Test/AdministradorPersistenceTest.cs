using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Backend.Dominio.Entidades;
using Backend.Dominio.Enums;
using Backend.Infraestrutura.Db;
using Microsoft.VisualStudio.TestTools.UnitTesting; // Já estava correto

namespace Test.Persistence;

[TestClass]
public class AdministradorPersistenceTest
{
    private DbContexto _contexto = default!;

    [TestInitialize]
    public void Setup()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Test.json")
            .Build();

        var connectionString = config.GetConnectionString("MySql");

        var options = new DbContextOptionsBuilder<DbContexto>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            .Options;

        _contexto = new DbContexto(options);

        // Garante que o banco de dados de teste está limpo e com o schema atualizado
        _contexto.Database.EnsureDeleted();
        _contexto.Database.EnsureCreated();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _contexto?.Dispose();
    }

    [TestMethod]
    public async Task DevePersistirUmAdministradorNoBancoDeDados()
    {
        // Arrange
        var novoAdministrador = new Administrador
        {
            Email = "persistence@test.com",
            Senha = "uma_senha_hash", // Em um cenário real, seria um hash
            Perfil = Perfil.Editor.ToString()
        };

        // Act
        _contexto.Administradores.Add(novoAdministrador);
        await _contexto.SaveChangesAsync();

        // Assert
        // Busca o registro em um novo contexto para garantir que foi salvo no banco
        var admDoBanco = await _contexto.Administradores.FirstOrDefaultAsync(a => a.Id == novoAdministrador.Id);

        Assert.IsNotNull(admDoBanco);
        Assert.IsTrue(admDoBanco.Id > 0);
        Assert.AreEqual("persistence@test.com", admDoBanco.Email);
        Assert.AreEqual(Perfil.Editor.ToString(), admDoBanco.Perfil);
    }
}