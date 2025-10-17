using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vendas.API.Data;
using Vendas.API.Models;
using Microsoft.AspNetCore.Authorization;
using Vendas.API.Services;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly VendasDbContext _context;
    private readonly EstoqueService _estoqueService;
    private readonly Shared.Services.RabbitMQService _rabbitMQService;

    public PedidosController(VendasDbContext context, EstoqueService estoqueService, Shared.Services.RabbitMQService rabbitMQService)
    {
        _context = context;
        _estoqueService = estoqueService;
        _rabbitMQService = rabbitMQService;
    }

    /// <summary>
    /// Cria um novo pedido, valida o estoque e notifica os sistemas.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Pedido>> PostPedido(Pedido pedido)
    {
        // Garante que o pedido não está vazio.
        if (pedido.Itens == null || !pedido.Itens.Any())
        {
            return BadRequest("O pedido deve conter pelo menos um item.");
        }

        pedido.DataPedido = DateTime.UtcNow;
        pedido.Status = StatusPedido.Processando;
        
        // Itera sobre os itens para validar o estoque e buscar o preço mais atual.
        foreach (var item in pedido.Itens)
        {
            var produtoInfo = await _estoqueService.VerificarEstoqueEPrecoAsync(item.ProdutoId);
            
            if (produtoInfo == null)
            {
                return BadRequest($"Produto {item.ProdutoId} não encontrado no estoque.");
            }
            
            if (produtoInfo.Quantidade < item.Quantidade)
            {
                return BadRequest($"Estoque insuficiente para o produto '{produtoInfo.Nome}'. Disponível: {produtoInfo.Quantidade}, Solicitado: {item.Quantidade}");
            }
            
            // Garante que o preço do item corresponde ao preço atual do produto no estoque.
            item.PrecoUnitario = produtoInfo.Preco;
        }

        // Calcula o valor total com base nos preços atualizados.
        pedido.ValorTotal = pedido.Itens.Sum(i => i.Quantidade * i.PrecoUnitario);

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        // Após criar o pedido, efetua a baixa no estoque de forma síncrona.
        foreach (var item in pedido.Itens)
        {
            var sucesso = await _estoqueService.AtualizarEstoqueAsync(item.ProdutoId, item.Quantidade);
            if (!sucesso)
            {
                // Ponto de melhoria: Implementar uma transação de compensação (rollback)
                // caso a atualização de estoque falhe após a criação do pedido.
                return StatusCode(500, $"Falha crítica ao atualizar o estoque do produto {item.ProdutoId}. O pedido foi criado mas o estoque não foi baixado.");
            }
        }

        // Publica uma notificação de venda no RabbitMQ para outros serviços interessados.
        var vendaNotification = new Shared.Models.VendaNotification
        {
            PedidoId = pedido.Id,
            DataVenda = pedido.DataPedido,
            Itens = pedido.Itens.Select(i => new Shared.Models.ItemVenda
            {
                ProdutoId = i.ProdutoId,
                Quantidade = i.Quantidade
            }).ToList()
        };

        _rabbitMQService.PublishMessage("venda_notifications", vendaNotification);

        // Finalmente, marca o pedido como aprovado.
        pedido.Status = StatusPedido.Aprovado;
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id }, pedido);
    }

    /// <summary>
    /// Retorna todos os pedidos cadastrados.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
    {
        // O .Include() é usado para carregar os itens de cada pedido (Eager Loading).
        return await _context.Pedidos.Include(p => p.Itens).ToListAsync();
    }

    /// <summary>
    /// Busca um pedido específico pelo seu ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Pedido>> GetPedido(int id)
    {
        // Encontra o pedido pelo ID, incluindo seus itens.
        var pedido = await _context.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
        {
            return NotFound();
        }

        return pedido;
    }

    /// <summary>
    /// Filtra os pedidos por um status específico.
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidosPorStatus(StatusPedido status)
    {
        var pedidos = await _context.Pedidos
            .Include(p => p.Itens)
            .Where(p => p.Status == status)
            .ToListAsync();

        return pedidos;
    }

    /// <summary>
    /// Altera o status de um pedido existente.
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> AtualizarStatusPedido(int id, [FromBody] AtualizarStatusRequest request)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null)
        {
            return NotFound();
        }

        pedido.Status = request.Status;
        await _context.SaveChangesAsync();

        return Ok(pedido);
    }

    /// <summary>
    /// Gera um relatório de vendas para um período específico.
    /// </summary>
    [HttpGet("relatorio")]
    public async Task<ActionResult<RelatorioVendas>> GetRelatorioVendas([FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
    {
        // Define o período padrão como os últimos 30 dias se não for especificado.
        var dataInicioFiltro = dataInicio ?? DateTime.UtcNow.AddDays(-30);
        var dataFimFiltro = dataFim ?? DateTime.UtcNow;

        var pedidos = await _context.Pedidos
            .Include(p => p.Itens)
            .Where(p => p.DataPedido >= dataInicioFiltro && p.DataPedido <= dataFimFiltro)
            .ToListAsync();

        var relatorio = new RelatorioVendas
        {
            DataInicio = dataInicioFiltro,
            DataFim = dataFimFiltro,
            TotalPedidos = pedidos.Count,
            TotalVendas = pedidos.Where(p => p.Status == StatusPedido.Aprovado).Sum(p => p.ValorTotal),
            PedidosAprovados = pedidos.Count(p => p.Status == StatusPedido.Aprovado),
            PedidosProcessando = pedidos.Count(p => p.Status == StatusPedido.Processando),
            PedidosCancelados = pedidos.Count(p => p.Status == StatusPedido.Cancelado)
        };

        return relatorio;
    }
}

// DTO para a requisição de atualização de status
public class AtualizarStatusRequest
{
    public StatusPedido Status { get; set; }
}

// DTO para o resultado do relatório de vendas
public class RelatorioVendas
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public int TotalPedidos { get; set; }
    public decimal TotalVendas { get; set; }
    public int PedidosAprovados { get; set; }
    public int PedidosProcessando { get; set; }
    public int PedidosCancelados { get; set; }
}
