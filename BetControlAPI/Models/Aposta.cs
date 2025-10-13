using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BetControlAPI.Models
{
    public class Aposta
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Usuario))]
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }

        [Required]
        [MaxLength(60)]
        public string Tipo { get; set; } = string.Empty; // futebol, cassino, corrida

        public DateTime Data { get; set; } = DateTime.UtcNow;

        public bool Ganhou { get; set; }
    }
}


