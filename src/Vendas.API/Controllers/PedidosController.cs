using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vendas.API.Data;
using Vendas.API.Models;
using Shared.Events;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class PedidosController : ControllerBase
{
    private readonly VendasDbContext _context;
    private readonly EstoqueService _estoqueService;
    private readonly MassTransit.IPublishEndpoint _publisher;

    public PedidosController(VendasDbContext context, EstoqueService estoqueService, MassTransit.IPublishEndpoint publisher)
    {
        _context = context;
        _estoqueService = estoqueService;
        _publisher = publisher;
    }

    // POST: api/Pedidos
    [HttpPost]
    public async Task<ActionResult<Pedido>> PostPedido(Pedido pedido)
    {
        // Define a data do pedido para o momento atual e o status inicial
        pedido.DataPedido = DateTime.UtcNow;
        pedido.Status = StatusPedido.Processando;

        // Validação de estoque síncrona
        foreach (var item in pedido.Itens)
        {
            var estoque = await _estoqueService.VerificarEstoqueAsync(item.ProdutoId);
            if (estoque == null)
            {
                return BadRequest($"Produto {item.ProdutoId} não encontrado no estoque.");
            }

            if (estoque < item.Quantidade)
            {
                return BadRequest($"Produto {item.ProdutoId} sem estoque suficiente. Disponível: {estoque}, solicitado: {item.Quantidade}");
            }
        }

        // Se chegou até aqui, decrementa o estoque (tentativa síncrona)
        var decremented = new List<(int produtoId, int quantidade)>();
        foreach (var item in pedido.Itens)
        {
            var ok = await _estoqueService.DecrementarEstoqueAsync(item.ProdutoId, item.Quantidade);
            if (!ok)
            {
                // Compensação: reverter todos os decrementos já aplicados
                var compensationFailures = new List<int>();
                foreach (var d in decremented)
                {
                    var r = await _estoqueService.IncrementarEstoqueAsync(d.produtoId, d.quantidade);
                    if (!r) compensationFailures.Add(d.produtoId);
                }

                if (compensationFailures.Any())
                {
                    return StatusCode(500, $"Falha ao decrementar estoque do produto {item.ProdutoId}. Além disso, a compensação falhou para produtos: {string.Join(',', compensationFailures)}");
                }

                return StatusCode(500, $"Falha ao decrementar estoque do produto {item.ProdutoId}. Operações anteriores foram revertidas.");
            }

            decremented.Add((item.ProdutoId, item.Quantidade));
        }

        // Lógica para calcular o valor total do pedido com base nos itens
        pedido.ValorTotal = pedido.Itens.Sum(i => i.Quantidade * i.PrecoUnitario);

        _context.Pedidos.Add(pedido);

        // Criar mensagem outbox para publicar após commit
        var itens = pedido.Itens.Select(i => new VendaItem(i.ProdutoId, i.Quantidade));
        var evt = new VendaRealizadaEvent(0, itens, pedido.ValorTotal); // Id será atualizado após salvar

        var outbox = new Vendas.API.Outbox.OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOn = DateTime.UtcNow,
            Type = "VendaRealizadaEvent",
            Content = System.Text.Json.JsonSerializer.Serialize(evt),
            Processed = false
        };

        _context.Add(outbox);

        await _context.SaveChangesAsync();

        // Update event with actual Pedido.Id
        evt = new VendaRealizadaEvent(pedido.Id, itens, pedido.ValorTotal);
        outbox.Content = System.Text.Json.JsonSerializer.Serialize(evt);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id }, pedido);
    }

    // GET: api/Pedidos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
    {
        // Usamos .Include() para que os itens de cada pedido também sejam retornados na consulta
        return await _context.Pedidos.Include(p => p.Itens).ToListAsync();
    }

    // GET: api/Pedidos/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Pedido>> GetPedido(int id)
    {
        // Encontra o pedido e também inclui os seus itens
        var pedido = await _context.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
        {
            return NotFound();
        }

        return pedido;
    }
}