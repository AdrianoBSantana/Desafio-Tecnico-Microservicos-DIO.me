using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Vendas.API.Services
{
    /// <summary>
    /// Serviço para comunicação com a API de Estoque.
    /// Utiliza um HttpClient configurado para interagir com os endpoints de produtos.
    /// </summary>
    public class EstoqueService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EstoqueService> _logger;

        public EstoqueService(HttpClient httpClient, ILogger<EstoqueService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Consulta a API de Estoque para obter os dados de um produto específico.
        /// </summary>
        /// <returns>O DTO do produto ou null se não for encontrado ou ocorrer um erro.</returns>
        public async Task<ProdutoEstoqueDto?> VerificarEstoqueEPrecoAsync(int produtoId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ProdutoEstoqueDto>($"/api/produtos/{produtoId}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Falha ao comunicar com a API de Estoque para obter o produto {ProdutoId}.", produtoId);
                return null;
            }
        }

        /// <summary>
        /// Solicita a atualização de estoque de um produto na API de Estoque.
        /// </summary>
        /// <remarks>
        /// ATENÇÃO: Esta implementação usa um padrão de "Leitura-Modificação-Escrita".
        /// Isso pode causar condições de corrida (race conditions) em um ambiente com muitas vendas simultâneas.
        /// Uma abordagem mais robusta seria ter um endpoint específico na API de Estoque para
        /// decrementar a quantidade de forma atômica (ex: via PATCH ou um endpoint dedicado).
        /// </remarks>
        /// <returns>True se a atualização foi bem-sucedida, False caso contrário.</returns>
        public async Task<bool> AtualizarEstoqueAsync(int produtoId, int quantidadeVendida)
        {
            var produto = await VerificarEstoqueEPrecoAsync(produtoId);
            if (produto == null)
            {
                _logger.LogWarning("Não foi possível atualizar o estoque pois o produto {ProdutoId} não foi encontrado.", produtoId);
                return false;
            }

            var novaQuantidade = produto.Quantidade - quantidadeVendida;

            var content = new StringContent(JsonSerializer.Serialize(new { quantidade = novaQuantidade }), Encoding.UTF8, "application/json");
            
            // Usando PUT para um endpoint que atualiza apenas o estoque.
            var response = await _httpClient.PutAsync($"/api/produtos/{produtoId}/atualizar-estoque", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("A API de Estoque retornou um erro ao tentar atualizar o produto {ProdutoId}. Status: {StatusCode}", produtoId, response.StatusCode);
            }
            
            return response.IsSuccessStatusCode;
        }
    }

    /// <summary>
    /// DTO que representa os dados de um produto conforme recebido da API de Estoque.
    /// </summary>
    public class ProdutoEstoqueDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal Preco { get; set; }
    }
}
