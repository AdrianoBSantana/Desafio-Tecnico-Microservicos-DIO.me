namespace Vendas.API.Models
{
    public class Pedido
    {
        public int Id { get; set; } // <-- ALTERADO PARA INT
        public DateTime DataPedido { get; set; }
        public StatusPedido Status { get; set; } // <-- ADICIONADO
        public decimal ValorTotal { get; set; }
        public List<ItemPedido> Itens { get; set; } = new();
    }
}