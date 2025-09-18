namespace Backend.Dominio.ModelViews
{
    public record Home
    {
        public string Mensagem { get; init; } = "Bem-vindo à API de veículos - Minimal API";
        public string Documentacao { get; init; } = "/swagger";
    }
}
