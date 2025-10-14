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
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UsuariosController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetAll()
        {
            var usuarios = await _db.Usuarios.AsNoTracking().ToListAsync();
            return Ok(usuarios);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Usuario>> GetById(int id)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            return Ok(usuario);
        }

        [HttpPost]
        public async Task<ActionResult<Usuario>> Create(Usuario usuario)
        {
            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, usuario);
        }

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

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            _db.Usuarios.Remove(usuario);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Usuários que ultrapassaram limite mensal: compara somatório de apostas do mês com limite
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


