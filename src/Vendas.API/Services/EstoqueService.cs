// src/Vendas.API/Services/EstoqueService.cs

public class EstoqueService
{
    private readonly HttpClient _httpClient;

    // A URL base do microserviço de Estoque será injetada
    public EstoqueService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int?> VerificarEstoqueAsync(Guid produtoId)
    {
        // Monta a URL para consultar o produto
        var response = await _httpClient.GetAsync($"/api/produtos/{produtoId}");

        if (response.IsSuccessStatusCode)
        {
            // Desserializa a resposta (assumindo que o Estoque retorna o objeto Produto)
            var produto = await response.Content.ReadFromJsonAsync<ProdutoEstoqueDto>();
            
            // Retorna a quantidade em estoque
            return produto?.QuantidadeEmEstoque; 
        }

        // Retorna null ou 0 se o produto não for encontrado (404) ou se houver erro
        return null; 
    }
}

// DTO para receber dados do Estoque.API (crie esta classe em uma pasta 'Dtos')
public class ProdutoEstoqueDto
{
    public Guid Id { get; set; }
    public int QuantidadeEmEstoque { get; set; }
}