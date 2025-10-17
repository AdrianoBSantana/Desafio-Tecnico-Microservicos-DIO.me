namespace Shared.Models
{
    public class VendaNotification
    {
        public int PedidoId { get; set; }
        public DateTime DataVenda { get; set; }
        public List<ItemVenda> Itens { get; set; } = new();
    }

    public class ItemVenda
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }
}

