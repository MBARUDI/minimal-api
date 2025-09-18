using System.Collections.Generic;
using System.Linq;
using Backend.Dominio.Entidades;
using Backend.DTOs;
using Backend.Infraestrutura.Db;
using Backend.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Dominio.Servicos
{
    public class VeiculoServico : IVeiculoServico
    {
        private readonly DbContexto _contexto;

        public VeiculoServico(DbContexto contexto)
        {
            _contexto = contexto;
        }

        public void Incluir(Veiculo veiculo)
        {
            _contexto.Veiculos.Add(veiculo);
            _contexto.SaveChanges();
        }

        public void Atualizar(Veiculo veiculo)
        {
            _contexto.Veiculos.Update(veiculo);
            _contexto.SaveChanges();
        }

        public void Apagar(Veiculo veiculo)
        {
            _contexto.Veiculos.Remove(veiculo);
            _contexto.SaveChanges();
        }

        public Veiculo? BuscaPorId(int id)
        {
            return _contexto.Veiculos.FirstOrDefault(v => v.Id == id);
        }

        public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null)
        {
            var query = _contexto.Veiculos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
            {
                query = query.Where(v => EF.Functions.Like(v.Nome.ToLower(), $"%{nome.ToLower()}%"));
            }

            if (!string.IsNullOrWhiteSpace(marca))
            {
                query = query.Where(v => EF.Functions.Like(v.Marca.ToLower(), $"%{marca.ToLower()}%"));
            }

            const int itensPorPagina = 10;

            if (pagina.HasValue && pagina > 0)
            {
                query = query.Skip((pagina.Value - 1) * itensPorPagina)
                             .Take(itensPorPagina);
            }

            return query.ToList();
        }
    }
}
