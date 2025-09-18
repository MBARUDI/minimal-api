using Backend.Dominio.Entidades;
using Backend.DTOs;
using System.Collections.Generic;

namespace Backend.Dominio.Interfaces
{
    public interface IAdministradorServico
    {
        Administrador? Login(LoginDTO loginDTO);
        Administrador Incluir(Administrador administrador);
        Administrador? BuscaPorId(int id);
        List<Administrador> Todos(int? pagina = 1);
    }
}
