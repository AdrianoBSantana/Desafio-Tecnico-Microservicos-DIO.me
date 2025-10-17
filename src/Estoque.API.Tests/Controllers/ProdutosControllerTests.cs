using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Estoque.API.Data;
using Estoque.API.Models;
using System.Net.Http.Json;

namespace Estoque.API.Tests.Controllers
{
    public class ProdutosControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ProdutosControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<EstoqueDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<EstoqueDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDatabase");
                    });
                });
            });
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetProdutos_ReturnsEmptyList_WhenNoProducts()
        {
            // Act
            var response = await _client.GetAsync("/api/produtos");

            // Assert
            response.EnsureSuccessStatusCode();
            var produtos = await response.Content.ReadFromJsonAsync<List<Produto>>();
            Assert.NotNull(produtos);
            Assert.Empty(produtos);
        }

        [Fact]
        public async Task PostProduto_CreatesProduct_WhenValidData()
        {
            // Arrange
            var produto = new Produto
            {
                Nome = "Produto Teste",
                Descricao = "Descrição do produto teste",
                Preco = 29.99m,
                Quantidade = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/produtos", produto);

            // Assert
            response.EnsureSuccessStatusCode();
            var createdProduto = await response.Content.ReadFromJsonAsync<Produto>();
            Assert.NotNull(createdProduto);
            Assert.Equal(produto.Nome, createdProduto.Nome);
            Assert.Equal(produto.Preco, createdProduto.Preco);
            Assert.Equal(produto.Quantidade, createdProduto.Quantidade);
        }

        [Fact]
        public async Task GetProduto_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Act
            var response = await _client.GetAsync("/api/produtos/999");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PutProduto_UpdatesProduct_WhenValidData()
        {
            // Arrange
            var produto = new Produto
            {
                Nome = "Produto Original",
                Descricao = "Descrição original",
                Preco = 19.99m,
                Quantidade = 5
            };

            var createResponse = await _client.PostAsJsonAsync("/api/produtos", produto);
            var createdProduto = await createResponse.Content.ReadFromJsonAsync<Produto>();

            createdProduto!.Nome = "Produto Atualizado";
            createdProduto.Preco = 24.99m;

            // Act
            var response = await _client.PutAsJsonAsync($"/api/produtos/{createdProduto.Id}", createdProduto);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task DeleteProduto_RemovesProduct_WhenProductExists()
        {
            // Arrange
            var produto = new Produto
            {
                Nome = "Produto para Deletar",
                Descricao = "Descrição",
                Preco = 15.99m,
                Quantidade = 3
            };

            var createResponse = await _client.PostAsJsonAsync("/api/produtos", produto);
            var createdProduto = await createResponse.Content.ReadFromJsonAsync<Produto>();

            // Act
            var response = await _client.DeleteAsync($"/api/produtos/{createdProduto!.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
        }
    }
}


