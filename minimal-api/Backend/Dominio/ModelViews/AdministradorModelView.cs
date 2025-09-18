using Backend.Dominio.Enums;

namespace Backend.Dominio.ModelViews
{
    public record AdministradorModelView
    {
        public int Id { get; init; }
        public string Email { get; init; } = string.Empty;
        public Perfil Perfil { get; init; } = Perfil.Adm;
    }
}
