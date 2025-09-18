using Microsoft.EntityFrameworkCore;
using Backend.Dominio.Entidades;
using Backend.DTOs;
using Backend.Dominio.Enums;
using Backend.Dominio.Servicos;
using Backend.Infraestrutura.Db;


namespace Test.Dominio.Servicos;

[TestClass] 
public class AdministradorServicoTest
{
    public AdministradorServicoTest()
    {
        // Construtor vazio para evitar problemas com a inicialização de testes.
    }

    private DbContexto CriarContextoDeTeste()
    {
        var options = new DbContextOptionsBuilder<DbContexto>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Usa um nome único para cada teste
            .Options;
        
        var context = new DbContexto(options);
        // Opcional: Adicionar o seed de dados se seus testes dependerem dele
        // context.Database.EnsureCreated(); 
        return context;
    }

    [TestMethod]
    public void TestandoSalvarAdministrador()
    {
        // Arrange
        var context = CriarContextoDeTeste();

        var adm = new Administrador();
        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = Perfil.Adm.ToString();

        var administradorServico = new AdministradorServico(context);

        // Act
        administradorServico.Incluir(adm);

        // Assert
        var administradores = administradorServico.Todos(1);
        Assert.AreEqual(1, administradores.Count());
        Assert.AreEqual("teste@teste.com", administradores.First().Email);
        Assert.IsTrue(adm.Id > 0); // Garante que o ID foi gerado pelo banco
    }

    [TestMethod]
    public void TestandoBuscaPorId()
    {
        // Arrange
        var context = CriarContextoDeTeste();

        var adm = new Administrador();
        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = Perfil.Editor.ToString();

        var administradorServico = new AdministradorServico(context);
        administradorServico.Incluir(adm);

        // Act
        var admDoBanco = administradorServico.BuscaPorId(adm.Id);

        // Assert
        Assert.IsNotNull(admDoBanco);
        Assert.AreEqual(adm.Id, admDoBanco.Id);
        Assert.AreEqual("teste@teste.com", admDoBanco.Email);
        Assert.AreEqual(Perfil.Editor.ToString(), admDoBanco.Perfil);
    }

    [TestMethod]
    public void TestandoBuscaPorEmail()
    {
        // Arrange
        var context = CriarContextoDeTeste();
        var emailParaBuscar = "busca@email.com";

        var adm1 = new Administrador { Email = "outro@email.com", Senha = "123", Perfil = Perfil.Adm.ToString() };
        var adm2 = new Administrador { Email = emailParaBuscar, Senha = "456", Perfil = Perfil.Editor.ToString() };

        var administradorServico = new AdministradorServico(context);
        administradorServico.Incluir(adm1);
        administradorServico.Incluir(adm2);

        // Act
        var admEncontrado = administradorServico.BuscaPorEmail(emailParaBuscar);

        // Assert
        Assert.IsNotNull(admEncontrado);
        Assert.AreEqual(adm2.Id, admEncontrado.Id);
        Assert.AreEqual(emailParaBuscar, admEncontrado.Email);
    }

    [TestMethod]
    public void TestandoBuscaPorEmail_QuandoNaoEncontra_RetornaNulo()
    {
        // Arrange
        var context = CriarContextoDeTeste();
        var administradorServico = new AdministradorServico(context);

        // Act & Assert
        Assert.IsNull(administradorServico.BuscaPorEmail("inexistente@email.com"));
    }
}