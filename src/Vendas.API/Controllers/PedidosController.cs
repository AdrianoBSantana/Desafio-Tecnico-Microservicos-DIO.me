using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vendas.API.Data;
using Vendas.API.Models;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly VendasDbContext _context;

    public PedidosController(VendasDbContext context)
    {
        _context = context;
    }

    // POST: api/Pedidos
    [HttpPost]
    public async Task<ActionResult<Pedido>> PostPedido(Pedido pedido)
    {
        // Define a data do pedido para o momento atual e o status inicial
        pedido.DataPedido = DateTime.UtcNow;
        pedido.Status = StatusPedido.Processando;
        
        // ---- Futuramente, aqui entrará a lógica para se comunicar com a Estoque.API ----
        // 1. Para cada item no 'pedido.Itens', verificar se há estoque suficiente na Estoque.API.
        // 2. Se não houver, retorna um BadRequest("Produto sem estoque.").
        // 3. Se houver, continua e dá baixa no estoque.
        // --------------------------------------------------------------------------------

        // Lógica para calcular o valor total do pedido com base nos itens
        pedido.ValorTotal = pedido.Itens.Sum(i => i.Quantidade * i.PrecoUnitario);

        _context.Pedidos.Add(pedido);
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