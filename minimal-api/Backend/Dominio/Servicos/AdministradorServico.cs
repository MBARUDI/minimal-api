using System.Collections.Generic;
using System.Linq;
using Backend.Dominio.Entidades;
using Backend.DTOs;
using Backend.Infraestrutura.Db;
using Backend.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Dominio.Servicos
{
    public class AdministradorServico : IAdministradorServico
    {
        private readonly DbContexto _contexto;

        public AdministradorServico(DbContexto contexto)
        {
            _contexto = contexto;
        }

        public Administrador? BuscaPorId(int id)
        {
            return _contexto.Administradores
                            .FirstOrDefault(a => a.Id == id);
        }

        public Administrador? BuscaPorEmail(string email)
        {
            return _contexto.Administradores
                            .FirstOrDefault(a => a.Email == email);
        }

        public Administrador Incluir(Administrador administrador)
        {
            _contexto.Administradores.Add(administrador);
            _contexto.SaveChanges();
            return administrador;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            return _contexto.Administradores
                            .FirstOrDefault(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha);
        }

        public List<Administrador> Todos(int? pagina = 1)
        {
            const int itensPorPagina = 10;
            var query = _contexto.Administradores.AsQueryable();

            if (pagina.HasValue && pagina > 0)
            {
                query = query.Skip((pagina.Value - 1) * itensPorPagina)
                             .Take(itensPorPagina);
            }

            return query.ToList();
        }
    }
}
