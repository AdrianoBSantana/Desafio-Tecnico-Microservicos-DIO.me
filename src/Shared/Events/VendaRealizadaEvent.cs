namespace Shared.Events
{
    public record VendaItem(int ProdutoId, int Quantidade);

    public record VendaRealizadaEvent(int PedidoId, IEnumerable<VendaItem> Itens, decimal ValorTotal);
}
