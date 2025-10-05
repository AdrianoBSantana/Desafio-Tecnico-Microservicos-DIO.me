// src/Vendas.API/Models/ItemPedido.cs

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace Vendas.API.Models
{
    public class ItemPedido
    {
        public int Id { get; set; }
        
        // ID do produto (referência ao Microserviço de Estoque)
        public required Guid ProdutoId { get; set; } 
        
        public required int Quantidade { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")] // Preço unitário no momento da venda
        public decimal PrecoUnitario { get; set; } 

        // Chave estrangeira para o Pedido pai
        public Guid PedidoId { get; set; }
        
        // Propriedade de navegação para o Pedido
        [JsonIgnore] 
    public Pedido? Pedido { get; set; } 
    }
}