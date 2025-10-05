using Vendas.API.Data;
using Vendas.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Vendas.API.Controllers
{
    [ApiController]
    [Route("api/pedidos")] // A rota que o cliente usará
    public class PedidosController : ControllerBase
    {
        private readonly VendasDbContext _context;
        private readonly EstoqueService _estoqueService;

        public PedidosController(VendasDbContext context, EstoqueService estoqueService)
        {
            _context = context;
    _estoqueService = estoqueService;
        }
        
        // Endpoint: GET api/pedidos/{id}
        /// <summary>
        /// Consulta um pedido pelo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Pedido>> ConsultarPedido(Guid id)
        {
            // Incluímos os Itens do Pedido na consulta (Eager Loading)
            var pedido = await _context.Pedidos
                                       .Include(p => p.Itens)
                                       .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                return NotFound(); // Retorna HTTP 404
            }

            return Ok(pedido); // Retorna o pedido com HTTP 200 OK
        }

        // Endpoint: POST api/pedidos
        /// <summary>
        /// Cria um novo pedido de venda (sem validação de estoque por enquanto).
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Pedido>> CriarPedido(Pedido novoPedido)
        {
            // 1. Definições Iniciais
            novoPedido.Id = Guid.NewGuid();
            novoPedido.DataCriacao = DateTime.UtcNow;
            novoPedido.Status = "Criado"; // Status inicial
            
            foreach (var item in novoPedido.Itens)
    {
        var estoqueDisponivel = await _estoqueService.VerificarEstoqueAsync(item.ProdutoId);

        // Se o estoque for null (erro ou produto não existe) ou insuficiente
        if (estoqueDisponivel == null || estoqueDisponivel < item.Quantidade)
        {
            // Retorna um erro 400 Bad Request, informando o problema
            return BadRequest($"O estoque para o produto {item.ProdutoId} é insuficiente ou o produto não existe.");
        }
    }
    // FIM DA VALIDAÇÃO

             novoPedido.ValorTotal = novoPedido.Itens.Sum(item => item.PrecoUnitario * item.Quantidade);

            // 3. Persistência
             _context.Pedidos.Add(novoPedido);
            await _context.SaveChangesAsync();
            
            // Retorna o HTTP 201 Created
            return CreatedAtAction(nameof(ConsultarPedido), new { id = novoPedido.Id }, novoPedido);
        }
    }
}