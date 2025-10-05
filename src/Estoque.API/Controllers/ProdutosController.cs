using Estoque.API.Data;
using Estoque.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Estoque.API.Controllers
{
    [ApiController]
    [Route("api/produtos")] // A rota que o cliente usará
    public class ProdutosController : ControllerBase
    {
        private readonly EstoqueDbContext _context;

        // Injeção de Dependência: O .NET fornece o DbContext
        public ProdutosController(EstoqueDbContext context)
        {
            _context = context;
        }

        // Endpoint: POST api/produtos
        /// <summary>
        /// Cadastra um novo produto no estoque.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Produto>> CadastrarProduto(Produto produto)
        {   produto.Id = Guid.NewGuid();
            // Adiciona a entidade ao DbContext (ainda não está no banco)
            _context.Produtos.Add(produto);
            
            // Salva as mudanças no banco de dados
            await _context.SaveChangesAsync();
            
            // Retorna a resposta HTTP 201 Created com a localização do novo recurso
            return CreatedAtAction(nameof(ConsultarPorId), new { id = produto.Id }, produto);
        }

        // Endpoint: GET api/produtos/{id}
        /// <summary>
        /// Consulta um produto pelo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> ConsultarPorId(Guid id)
        {
            var produto = await _context.Produtos.FindAsync(id);

            if (produto == null)
            {
                // Retorna HTTP 404 Not Found se o produto não existir
                return NotFound();
            }

            // Retorna o produto com HTTP 200 OK
            return Ok(produto);
        }
    }
}