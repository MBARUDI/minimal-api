using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Frontend.Services; // Este namespace agora está correto porque o arquivo está em Frontend/Services

// DTOs and Models
public record LoginDTO(string Email, string Senha);
public record AdministradorLogado(string Email, string Perfil, string Token);
public class Veiculo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public int Ano { get; set; }
}
public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;

    // A URL base da sua API. Ajuste se necessário.
    private const string BaseUrl = "http://localhost:5169"; 

    public ApiService(HttpClient httpClient, AuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    public async Task<AdministradorLogado?> LoginAsync(LoginDTO loginDto)
    {
        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/login", loginDto);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AdministradorLogado>();
        }
        return null;
    }

    public async Task<List<Veiculo>?> GetVeiculosAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Veiculo>>($"{BaseUrl}/veiculos");
    }

    public async Task<Veiculo?> CreateVeiculoAsync(Veiculo veiculo)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _authService.GetTokenAsync());
        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/veiculos", veiculo);
        return await response.Content.ReadFromJsonAsync<Veiculo>();
    }

    public async Task UpdateVeiculoAsync(int id, Veiculo veiculo)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _authService.GetTokenAsync());
        await _httpClient.PutAsJsonAsync($"{BaseUrl}/veiculos/{id}", veiculo);
    }

    public async Task DeleteVeiculoAsync(int id)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _authService.GetTokenAsync());
        await _httpClient.DeleteAsync($"{BaseUrl}/veiculos/{id}");
    }
}