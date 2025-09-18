using System.Collections.Generic;

namespace Backend.Dominio.ModelViews
{
    public record ErrosDeValidacao
    {
        public List<string> Mensagens { get; init; } = new();
    }
}
