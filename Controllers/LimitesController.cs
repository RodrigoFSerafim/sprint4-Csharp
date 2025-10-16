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
    /// Controller responsável pelas operações CRUD de limites mensais de apostas
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LimitesController : ControllerBase
    {
        private readonly AppDbContext _db;

        /// <summary>
        /// Inicializa o controller com o contexto do banco de dados
        /// </summary>
        /// <param name="db">Contexto do Entity Framework</param>
        public LimitesController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Lista todos os limites cadastrados
        /// </summary>
        /// <returns>Lista de limites</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Limite>>> GetAll()
        {
            var limites = await _db.Limites.AsNoTracking().ToListAsync();
            return Ok(limites);
        }

        /// <summary>
        /// Busca um limite específico pelo ID
        /// </summary>
        /// <param name="id">ID do limite</param>
        /// <returns>Dados do limite ou 404 se não encontrado</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Limite>> GetById(int id)
        {
            var limite = await _db.Limites.FindAsync(id);
            if (limite == null) return NotFound();
            return Ok(limite);
        }

        /// <summary>
        /// Cria um novo limite mensal para um usuário
        /// Se MesReferencia não informado, usa o mês atual
        /// Inicializa ValorAtual com o somatório das apostas existentes do mês
        /// </summary>
        /// <param name="limite">Dados do limite a ser criado</param>
        /// <returns>Limite criado com ID gerado</returns>
        [HttpPost]
        public async Task<ActionResult<Limite>> Create(Limite limite)
        {
            var usuarioExists = await _db.Usuarios.AnyAsync(u => u.Id == limite.UsuarioId);
            if (!usuarioExists) return BadRequest("UsuarioId inválido");

            // Se não informado, define mês atual
            if (string.IsNullOrWhiteSpace(limite.MesReferencia))
            {
                limite.MesReferencia = Limite.GetMesReferencia(System.DateTime.UtcNow);
            }

            // Inicializa ValorAtual com somatório das apostas do mês
            var gasto = await _db.Apostas
                .Where(a => a.UsuarioId == limite.UsuarioId && Limite.GetMesReferencia(a.Data) == limite.MesReferencia)
                .SumAsync(a => (decimal?)a.Valor) ?? 0m;
            limite.ValorAtual = gasto;

            _db.Limites.Add(limite);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = limite.Id }, limite);
        }

        /// <summary>
        /// Atualiza um limite existente
        /// </summary>
        /// <param name="id">ID do limite a ser atualizado</param>
        /// <param name="input">Novos dados do limite</param>
        /// <returns>204 No Content se sucesso, 400 se IDs não coincidem, 404 se não encontrado</returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Limite input)
        {
            if (id != input.Id) return BadRequest("Id mismatch");

            var exists = await _db.Limites.AnyAsync(l => l.Id == id);
            if (!exists) return NotFound();

            _db.Entry(input).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Remove um limite do sistema
        /// </summary>
        /// <param name="id">ID do limite a ser removido</param>
        /// <returns>204 No Content se sucesso, 404 se não encontrado</returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var limite = await _db.Limites.FindAsync(id);
            if (limite == null) return NotFound();
            _db.Limites.Remove(limite);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}


