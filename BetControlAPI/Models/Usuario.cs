using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BetControlAPI.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(160)]
        public string Email { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Saldo { get; set; }

        public ICollection<Aposta> Apostas { get; set; } = new List<Aposta>();

        public ICollection<Limite> Limites { get; set; } = new List<Limite>();
    }
}


