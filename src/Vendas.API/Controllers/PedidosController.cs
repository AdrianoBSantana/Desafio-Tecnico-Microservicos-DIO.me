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
        // Ao criar, definimos a data e o status inicial
        pedido.DataPedido = DateTime.UtcNow;
        pedido.Status = StatusPedido.Processando;
        
        // Lógica para calcular o valor total, se necessário
        // pedido.ValorTotal = pedido.Itens.Sum(i => i.Quantidade * i.PrecoUnitario);

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id }, pedido);
    }

    // GET: api/Pedidos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
    {
        // Inclui os itens de cada pedido na consulta
        return await _context.Pedidos.Include(p => p.Itens).ToListAsync();
    }

    // GET: api/Pedidos/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Pedido>> GetPedido(int id)
    {
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