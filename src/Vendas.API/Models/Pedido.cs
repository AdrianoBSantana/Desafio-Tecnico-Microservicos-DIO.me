// src/Vendas.API/Models/Pedido.cs

using System.ComponentModel.DataAnnotations.Schema;

namespace Vendas.API.Models
{
    public class Pedido
    {
        public Guid Id { get; set; }
        
        // Identificador do cliente que fez o pedido (necessário para autenticação futura)
        public required Guid ClienteId { get; set; } 
        
        // Status do pedido (ex: "Criado", "Processando", "Enviado")
        public required string Status { get; set; } 
        
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // O valor total do pedido, calculado a partir dos itens
        [Column(TypeName = "decimal(18, 2)")] // Garante precisão no banco
        public decimal ValorTotal { get; set; } 

        // Relação 1-para-Muitos: Um pedido pode ter muitos itens
        public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
    }
}