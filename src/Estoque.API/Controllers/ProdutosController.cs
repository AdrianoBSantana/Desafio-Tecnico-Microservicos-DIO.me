using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Estoque.API.Data;
using Estoque.API.Models;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class ProdutosController : ControllerBase
{
    private readonly EstoqueDbContext _context;

    public ProdutosController(EstoqueDbContext context)
    {
        _context = context;
    }

    // GET: api/Produtos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
    {
        return await _context.Produtos.ToListAsync();
    }

    // GET: api/Produtos/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Produto>> GetProduto(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if (produto == null)
        {
            return NotFound();
        }

        return produto;
    }

    // POST: api/Produtos
    [HttpPost]
    public async Task<ActionResult<Produto>> PostProduto(Produto produto)
    {
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduto), new { id = produto.Id }, produto);
    }

    // PUT: api/Produtos/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProduto(int id, Produto produto)
    {
        if (id != produto.Id)
        {
            return BadRequest();
        }

        _context.Entry(produto).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Produtos.Any(e => e.Id == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Produtos/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduto(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);
        if (produto == null)
        {
            return NotFound();
        }

        _context.Produtos.Remove(produto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/Produtos/{id}/decrement?quantidade=1
    [HttpPost("{id}/decrement")]
    public async Task<IActionResult> DecrementarEstoque(int id, [FromQuery] int quantidade)
    {
        if (quantidade <= 0)
            return BadRequest("Quantidade deve ser maior que zero.");

        var produto = await _context.Produtos.FindAsync(id);
        if (produto == null)
            return NotFound();

        if (produto.Quantidade < quantidade)
            return BadRequest("Estoque insuficiente.");

        produto.Quantidade -= quantidade;
        await _context.SaveChangesAsync();

        return Ok(new { produto.Id, produto.Nome, produto.Quantidade });
    }

    // POST: api/Produtos/{id}/increment?quantidade=1
    [HttpPost("{id}/increment")]
    public async Task<IActionResult> IncrementarEstoque(int id, [FromQuery] int quantidade)
    {
        if (quantidade <= 0)
            return BadRequest("Quantidade deve ser maior que zero.");

        var produto = await _context.Produtos.FindAsync(id);
        if (produto == null)
            return NotFound();

        produto.Quantidade += quantidade;
        await _context.SaveChangesAsync();

        return Ok(new { produto.Id, produto.Nome, produto.Quantidade });
    }
}