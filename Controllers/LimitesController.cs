using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BetControlAPI.Data;
using BetControlAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BetControlAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LimitesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public LimitesController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Limite>>> GetAll()
        {
            var limites = await _db.Limites.AsNoTracking().ToListAsync();
            return Ok(limites);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Limite>> GetById(int id)
        {
            var limite = await _db.Limites.FindAsync(id);
            if (limite == null) return NotFound();
            return Ok(limite);
        }

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


