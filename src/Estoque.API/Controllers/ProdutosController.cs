using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Estoque.API.Data;
using Estoque.API.Models;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProdutosController : ControllerBase
{
    private readonly EstoqueDbContext _context;

    public ProdutosController(EstoqueDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retorna uma lista de todos os produtos.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
    {
        return await _context.Produtos.ToListAsync();
    }

    /// <summary>
    /// Busca um produto específico pelo seu ID.
    /// </summary>
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

    /// <summary>
    /// Cadastra um novo produto no estoque.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Produto>> PostProduto(Produto produto)
    {
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduto), new { id = produto.Id }, produto);
    }

    /// <summary>
    /// Atualiza os dados de um produto existente.
    /// </summary>
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

    /// <summary>
    /// Remove um produto do estoque.
    /// </summary>
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

    /// <summary>
    /// Busca produtos por nome ou descrição.
    /// </summary>
    [HttpGet("buscar")]
    public async Task<ActionResult<IEnumerable<Produto>>> BuscarProdutos([FromQuery] string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            return BadRequest("O parâmetro 'nome' é obrigatório.");
        }

        var produtos = await _context.Produtos
            .Where(p => p.Nome.Contains(nome) || p.Descricao.Contains(nome))
            .ToListAsync();

        return produtos;
    }

    /// <summary>
    /// Lista produtos com estoque abaixo de um limite especificado.
    /// </summary>
    /// <param name="limite">O nível de estoque máximo para incluir no relatório. O padrão é 10.</param>
    [HttpGet("estoque-baixo")]
    public async Task<ActionResult<IEnumerable<Produto>>> ProdutosEstoqueBaixo([FromQuery] int limite = 10)
    {
        var produtos = await _context.Produtos
            .Where(p => p.Quantidade <= limite)
            .OrderBy(p => p.Quantidade)
            .ToListAsync();

        return produtos;
    }

    /// <summary>
    /// Define uma nova quantidade de estoque para um produto.
    /// </summary>
    [HttpPut("{id}/atualizar-estoque")]
    public async Task<IActionResult> AtualizarEstoque(int id, [FromBody] AtualizarEstoqueRequest request)
    {
        var produto = await _context.Produtos.FindAsync(id);
        if (produto == null)
        {
            return NotFound();
        }

        if (request.Quantidade < 0)
        {
            return BadRequest("A quantidade não pode ser um valor negativo.");
        }

        produto.Quantidade = request.Quantidade;
        await _context.SaveChangesAsync();

        return Ok(produto);
    }
}

/// <summary>
/// DTO para a requisição de atualização de estoque.
/// </summary>
public class AtualizarEstoqueRequest
{
    public int Quantidade { get; set; }
}
