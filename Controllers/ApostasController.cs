using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BetControlAPI.Data;
using BetControlAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BetControlAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApostasController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;

        public ApostasController(AppDbContext db, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Aposta>>> GetAll()
        {
            var apostas = await _db.Apostas.AsNoTracking().ToListAsync();
            return Ok(apostas);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Aposta>> GetById(int id)
        {
            var aposta = await _db.Apostas.FindAsync(id);
            if (aposta == null) return NotFound();
            return Ok(aposta);
        }

        [HttpPost]
        public async Task<ActionResult<Aposta>> Create(Aposta aposta)
        {
            var usuarioExists = await _db.Usuarios.AnyAsync(u => u.Id == aposta.UsuarioId);
            if (!usuarioExists) return BadRequest("UsuarioId inválido");

            _db.Apostas.Add(aposta);

            // Atualiza ValorAtual do Limite do mês
            var mes = Limite.GetMesReferencia(aposta.Data);
            var limite = await _db.Limites.FirstOrDefaultAsync(l => l.UsuarioId == aposta.UsuarioId && l.MesReferencia == mes);
            if (limite != null)
            {
                limite.ValorAtual += aposta.Valor;
            }

            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = aposta.Id }, aposta);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Aposta input)
        {
            if (id != input.Id) return BadRequest("Id mismatch");

            var original = await _db.Apostas.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (original == null) return NotFound();

            // Ajusta o limite do mês se mudar o valor/data/usuario
            if (original.UsuarioId == input.UsuarioId && Limite.GetMesReferencia(original.Data) == Limite.GetMesReferencia(input.Data))
            {
                var mes = Limite.GetMesReferencia(input.Data);
                var limite = await _db.Limites.FirstOrDefaultAsync(l => l.UsuarioId == input.UsuarioId && l.MesReferencia == mes);
                if (limite != null)
                {
                    limite.ValorAtual += (input.Valor - original.Valor);
                }
            }

            _db.Entry(input).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var aposta = await _db.Apostas.FindAsync(id);
            if (aposta == null) return NotFound();

            var mes = Limite.GetMesReferencia(aposta.Data);
            var limite = await _db.Limites.FirstOrDefaultAsync(l => l.UsuarioId == aposta.UsuarioId && l.MesReferencia == mes);
            if (limite != null)
            {
                limite.ValorAtual = Math.Max(0, limite.ValorAtual - aposta.Valor);
            }

            _db.Apostas.Remove(aposta);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Valor médio das apostas
        [HttpGet("media")]
        public async Task<ActionResult<decimal>> MediaApostas()
        {
            var media = await _db.Apostas.AnyAsync()
                ? await _db.Apostas.AverageAsync(a => a.Valor)
                : 0m;
            return Ok(media);
        }

        // Apostas acima da média
        [HttpGet("acima-da-media")]
        public async Task<ActionResult<IEnumerable<Aposta>>> ApostasAcimaDaMedia()
        {
            if (!await _db.Apostas.AnyAsync()) return Ok(Array.Empty<Aposta>());
            var media = await _db.Apostas.AverageAsync(a => a.Valor);
            var acima = await _db.Apostas.AsNoTracking().Where(a => a.Valor > media).ToListAsync();
            return Ok(acima);
        }

        // Conversão para USD via API pública (exchangerate.host)
        [HttpGet("{id:int}/valor-usd")]
        public async Task<ActionResult<object>> ConverterValorParaUSD(int id)
        {
            var aposta = await _db.Apostas.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (aposta == null) return NotFound();

            var client = _httpClientFactory.CreateClient();
            // API gratuita sem chave: https://api.exchangerate.host/latest?base=BRL&symbols=USD
            var url = "https://api.exchangerate.host/latest?base=BRL&symbols=USD";
            try
            {
                var resp = await client.GetFromJsonAsync<ExchangeRateResponse>(url);
                if (resp?.Rates == null || !resp.Rates.TryGetValue("USD", out var usdRate) || usdRate <= 0)
                {
                    return StatusCode(502, "Falha ao obter cotação");
                }
                var valorUsd = aposta.Valor * (decimal)usdRate;
                return Ok(new { aposta.Id, aposta.Valor, USD = valorUsd, Cotacao = usdRate });
            }
            catch
            {
                return StatusCode(502, "Erro ao consultar serviço externo");
            }
        }

        private class ExchangeRateResponse
        {
            public Dictionary<string, double> Rates { get; set; } = new();
        }
    }
}


