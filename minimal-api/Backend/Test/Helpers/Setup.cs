using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Backend; // Add reference to the Backend namespace
using Backend.Infraestrutura.Db;
using Microsoft.EntityFrameworkCore;

namespace Test.Helpers;

public class Setup
{
    public const string PORT = "5001";
    public static TestContext testContext = default!;
    public static WebApplicationFactory<Program> http = default!;
    public static HttpClient client = default!;

    public static void ClassInit(TestContext testContext)
    {
        Setup.testContext = testContext;
        Setup.http = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove a configuração do DbContext da API
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DbContexto>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    // Adiciona o DbContext usando um banco de dados em memória para os testes
                    services.AddDbContext<DbContexto>(options => options.UseInMemoryDatabase("TesteDeIntegracaoDB"));
                });
            });

        Setup.client = Setup.http.CreateClient();
    }

    public static void ClassCleanup()
    {
        Setup.http.Dispose();
    }
}