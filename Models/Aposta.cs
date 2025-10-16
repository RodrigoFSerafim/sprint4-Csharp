using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BetControlAPI.Models
{
    /// <summary>
    /// Representa uma aposta realizada por um usuário
    /// </summary>
    public class Aposta
    {
        /// <summary>
        /// Identificador único da aposta
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID do usuário que realizou a aposta (chave estrangeira)
        /// </summary>
        [ForeignKey(nameof(Usuario))]
        public int UsuarioId { get; set; }
        
        /// <summary>
        /// Navegação para o usuário proprietário da aposta
        /// </summary>
        public Usuario? Usuario { get; set; }

        /// <summary>
        /// Valor apostado (deve ser maior que 0.01)
        /// </summary>
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }

        /// <summary>
        /// Tipo da aposta (obrigatório, máximo 60 caracteres)
        /// Exemplos: futebol, cassino, corrida
        /// </summary>
        [Required]
        [MaxLength(60)]
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora da aposta (padrão: momento atual em UTC)
        /// </summary>
        public DateTime Data { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indica se a aposta foi vencedora
        /// </summary>
        public bool Ganhou { get; set; }
    }
}


