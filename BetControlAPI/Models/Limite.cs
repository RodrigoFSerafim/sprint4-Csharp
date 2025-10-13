using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BetControlAPI.Models
{
    public class Limite
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Usuario))]
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ValorMaximoMensal { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ValorAtual { get; set; }

        [Required]
        [MaxLength(7)]
        public string MesReferencia { get; set; } = string.Empty; // formato: yyyy-MM

        public static string GetMesReferencia(DateTime dateUtc)
        {
            return dateUtc.ToString("yyyy-MM");
        }
    }
}


