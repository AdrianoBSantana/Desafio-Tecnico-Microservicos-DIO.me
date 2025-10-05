// src/Vendas.API/Models/Pedido.cs

using System.ComponentModel.DataAnnotations.Schema;

namespace Vendas.API.Models
{
    public class Pedido
    {
        public Guid Id { get; set; }
        
        // Este deve ser requerido pelo cliente
        public required Guid ClienteId { get; set; } 
        
        // ESTES NÃO SÃO REQUERIDOS NA ENTRADA, POIS SÃO GERADOS:
        public string? Status { get; set; }
        public DateTime DataCriacao { get; set; } 
        
        [Column(TypeName = "decimal(18, 2)")]
       public decimal? ValorTotal { get; set; }  

        public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
    }
}