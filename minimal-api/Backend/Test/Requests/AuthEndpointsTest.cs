using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting; // Já estava correto
using Backend.Dominio.Entidades;
using Backend.Dominio.Enums;
using Backend.Infraestrutura.Db;
using Backend.Dominio.ModelViews;
using Backend.DTOs;
using Test.Helpers;

namespace Test.Requests;

[TestClass]
public class AuthEndpointsTest
{
    [ClassInitialize]
    public static void ClassInit(TestContext testContext)
    {
        Setup.ClassInit(testContext);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        Setup.ClassCleanup();
    }

    [TestMethod]
    public async Task DeveRetornarOkAoLogarComCredenciaisValidas()
    {
        // Arrange
        var email = "login.test@email.com";
        var senha = "123456";

        // Adiciona um usuário de teste diretamente no banco de dados em memória
        using (var scope = Setup.http.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DbContexto>();
            dbContext.Administradores.Add(new Administrador
            {
                Email = email,
                Senha = BCrypt.Net.BCrypt.HashPassword(senha),
                Perfil = Perfil.Adm.ToString()
            });
            await dbContext.SaveChangesAsync();
        }

        var loginDTO = new LoginDTO
        {
            Email = email,
            Senha = senha
        };

        var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "application/json");

        // Act
        var response = await Setup.client.PostAsync("/login", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        var admLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.IsNotNull(admLogado);
        Assert.AreEqual(email, admLogado.Email);
        Assert.IsFalse(string.IsNullOrEmpty(admLogado.Token));
    }

    [TestMethod]
    public async Task DeveRetornarOkNaRotaHome()
    {
        // Act
        var response = await Setup.client.GetAsync("/");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task DeveRetornarUnauthorizedAoLogarComSenhaInvalida()
    {
        // Arrange
        var loginDTO = new LoginDTO
        {
            Email = "email.qualquer@teste.com",
            Senha = "senha-errada"
        };
        var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "application/json");
        // Act
        var response = await Setup.client.PostAsync("/login", content);
        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}