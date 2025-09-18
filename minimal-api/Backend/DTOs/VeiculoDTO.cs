namespace Backend.DTOs;

public record VeiculoDTO
{
    public string Nome { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public int Ano { get; set; }
}
