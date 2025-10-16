using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BetControlAPI.Models
{
    /// <summary>
    /// Representa um limite mensal de apostas para um usuário
    /// </summary>
    public class Limite
    {
        /// <summary>
        /// Identificador único do limite
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID do usuário proprietário do limite (chave estrangeira)
        /// </summary>
        [ForeignKey(nameof(Usuario))]
        public int UsuarioId { get; set; }
        
        /// <summary>
        /// Navegação para o usuário proprietário do limite
        /// </summary>
        public Usuario? Usuario { get; set; }

        /// <summary>
        /// Valor máximo permitido para apostas no mês (deve ser maior ou igual a zero)
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal ValorMaximoMensal { get; set; }

        /// <summary>
        /// Valor atual gasto em apostas no mês (deve ser maior ou igual a zero)
        /// Atualizado automaticamente quando apostas são criadas/atualizadas/removidas
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal ValorAtual { get; set; }

        /// <summary>
        /// Mês de referência do limite no formato yyyy-MM (obrigatório, máximo 7 caracteres)
        /// Exemplo: "2025-10"
        /// </summary>
        [Required]
        [MaxLength(7)]
        public string MesReferencia { get; set; } = string.Empty;

        /// <summary>
        /// Converte uma data UTC para o formato de mês de referência (yyyy-MM)
        /// </summary>
        /// <param name="dateUtc">Data em UTC</param>
        /// <returns>String no formato yyyy-MM</returns>
        public static string GetMesReferencia(DateTime dateUtc)
        {
            return dateUtc.ToString("yyyy-MM");
        }
    }
}


