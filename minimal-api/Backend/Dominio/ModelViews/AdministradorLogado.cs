using Backend.Dominio.Enums;

namespace Backend.Dominio.ModelViews
{
    public record AdministradorLogado
    {
        public string Email { get; init; } = string.Empty;
        public Perfil Perfil { get; init; } = Perfil.Adm;
        public string Token { get; init; } = string.Empty;
    }
}
