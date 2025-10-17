// src/Vendas.API/Services/EstoqueService.cs

public class EstoqueService
{
    private readonly HttpClient _httpClient;

    // A URL base do microserviço de Estoque será injetada
    public EstoqueService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private async Task<string?> GetServiceTokenAsync()
    {
        try
        {
            var login = new { username = "admin", password = "password" };
            var resp = await _httpClient.PostAsJsonAsync("/api/auth/login", login);
            if (!resp.IsSuccessStatusCode) return null;
            var dict = await resp.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            if (dict != null && dict.TryGetValue("token", out var t)) return t;
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<int?> VerificarEstoqueAsync(int produtoId)
    {
        var token = await GetServiceTokenAsync();
        if (token == null) return null;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/produtos/{produtoId}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var produto = await response.Content.ReadFromJsonAsync<ProdutoEstoqueDto>();
            return produto?.Quantidade;
        }

        return null;
    }

    public async Task<bool> DecrementarEstoqueAsync(int produtoId, int quantidade)
    {
        var token = await GetServiceTokenAsync();
        if (token == null) return false;

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/produtos/{produtoId}/decrement?quantidade={quantidade}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> IncrementarEstoqueAsync(int produtoId, int quantidade)
    {
        var token = await GetServiceTokenAsync();
        if (token == null) return false;

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/produtos/{produtoId}/increment?quantidade={quantidade}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }
}

// DTO para receber dados do Estoque.API
public class ProdutoEstoqueDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal Preco { get; set; }
}