using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BetControlAPI.Models
{
    /// <summary>
    /// Representa um usuário do sistema de controle de apostas
    /// </summary>
    public class Usuario
    {
        /// <summary>
        /// Identificador único do usuário
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome completo do usuário (obrigatório, máximo 120 caracteres)
        /// </summary>
        [Required]
        [MaxLength(120)]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Email do usuário (obrigatório, formato válido, máximo 160 caracteres, único)
        /// </summary>
        [Required]
        [EmailAddress]
        [MaxLength(160)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Saldo atual do usuário (deve ser maior ou igual a zero)
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Saldo { get; set; }

        /// <summary>
        /// Coleção de apostas realizadas pelo usuário
        /// </summary>
        public ICollection<Aposta> Apostas { get; set; } = new List<Aposta>();

        /// <summary>
        /// Coleção de limites mensais definidos para o usuário
        /// </summary>
        public ICollection<Limite> Limites { get; set; } = new List<Limite>();
    }
}


