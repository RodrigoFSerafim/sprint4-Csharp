using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BetControlAPI.Data;
using BetControlAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BetControlAPI.Controllers
{
    /// <summary>
    /// Controller responsável pelas operações CRUD de usuários e consultas relacionadas a limites
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _db;

        /// <summary>
        /// Inicializa o controller com o contexto do banco de dados
        /// </summary>
        /// <param name="db">Contexto do Entity Framework</param>
        public UsuariosController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Lista todos os usuários cadastrados
        /// </summary>
        /// <returns>Lista de usuários</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetAll()
        {
            var usuarios = await _db.Usuarios.AsNoTracking().ToListAsync();
            return Ok(usuarios);
        }

        /// <summary>
        /// Busca um usuário específico pelo ID
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <returns>Dados do usuário ou 404 se não encontrado</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Usuario>> GetById(int id)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            return Ok(usuario);
        }

        /// <summary>
        /// Cria um novo usuário no sistema
        /// </summary>
        /// <param name="usuario">Dados do usuário a ser criado</param>
        /// <returns>Usuário criado com ID gerado</returns>
        [HttpPost]
        public async Task<ActionResult<Usuario>> Create(Usuario usuario)
        {
            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, usuario);
        }

        /// <summary>
        /// Atualiza os dados de um usuário existente
        /// </summary>
        /// <param name="id">ID do usuário a ser atualizado</param>
        /// <param name="input">Novos dados do usuário</param>
        /// <returns>204 No Content se sucesso, 400 se IDs não coincidem, 404 se não encontrado</returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Usuario input)
        {
            if (id != input.Id) return BadRequest("Id mismatch");

            var exists = await _db.Usuarios.AnyAsync(u => u.Id == id);
            if (!exists) return NotFound();

            _db.Entry(input).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Remove um usuário do sistema
        /// </summary>
        /// <param name="id">ID do usuário a ser removido</param>
        /// <returns>204 No Content se sucesso, 404 se não encontrado</returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            _db.Usuarios.Remove(usuario);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Lista usuários que ultrapassaram o limite mensal de apostas
        /// Compara o somatório de apostas do mês com o limite definido
        /// </summary>
        /// <param name="mes">Mês de referência no formato yyyy-MM</param>
        /// <returns>Lista de usuários que excederam o limite com dados de gasto</returns>
        [HttpGet("excederam-limite/{mes}")]
        public async Task<ActionResult<IEnumerable<object>>> UsuariosExcederamLimite(string mes)
        {
            var query = from l in _db.Limites.AsNoTracking()
                        join u in _db.Usuarios.AsNoTracking() on l.UsuarioId equals u.Id
                        where l.MesReferencia == mes
                        let gasto = _db.Apostas
                            .Where(a => a.UsuarioId == u.Id && Limite.GetMesReferencia(a.Data) == mes)
                            .Sum(a => (decimal?)a.Valor) ?? 0m
                        where gasto > l.ValorMaximoMensal
                        select new { u.Id, u.Nome, u.Email, l.MesReferencia, Limite = l.ValorMaximoMensal, Gasto = gasto };

            var result = await query.ToListAsync();
            return Ok(result);
        }
    }
}


