using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vendas.API.Data;
using Vendas.API.Models;
using System.Net.Http.Json;
using Moq;
using Vendas.API.Services;

namespace Vendas.API.Tests.Controllers
{
    public class PedidosControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly Mock<EstoqueService> _mockEstoqueService;
        private readonly Mock<Shared.Services.RabbitMQService> _mockRabbitMQService;

        public PedidosControllerTests(WebApplicationFactory<Program> factory)
        {
            _mockEstoqueService = new Mock<EstoqueService>(Mock.Of<HttpClient>());
            _mockRabbitMQService = new Mock<Shared.Services.RabbitMQService>(Mock.Of<IConfiguration>(), Mock.Of<ILogger<Shared.Services.RabbitMQService>>());

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<VendasDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<VendasDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestVendasDatabase");
                    });

                    services.AddSingleton(_mockEstoqueService.Object);
                    services.AddSingleton(_mockRabbitMQService.Object);
                });
            });
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetPedidos_ReturnsEmptyList_WhenNoOrders()
        {
            // Act
            var response = await _client.GetAsync("/api/pedidos");

            // Assert
            response.EnsureSuccessStatusCode();
            var pedidos = await response.Content.ReadFromJsonAsync<List<Pedido>>();
            Assert.NotNull(pedidos);
            Assert.Empty(pedidos);
        }

        [Fact]
        public async Task PostPedido_CreatesOrder_WhenValidData()
        {
            // Arrange
            var produtoEstoque = new ProdutoEstoqueDto
            {
                Id = 1,
                Nome = "Produto Teste",
                Descricao = "Descrição",
                Preco = 29.99m,
                Quantidade = 10
            };

            _mockEstoqueService.Setup(x => x.VerificarEstoqueEPrecoAsync(1))
                .ReturnsAsync(produtoEstoque);

            _mockEstoqueService.Setup(x => x.AtualizarEstoqueAsync(1, 2))
                .ReturnsAsync(true);

            var pedido = new Pedido
            {
                Itens = new List<ItemPedido>
                {
                    new ItemPedido
                    {
                        ProdutoId = 1,
                        Quantidade = 2,
                        PrecoUnitario = 29.99m
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/pedidos", pedido);

            // Assert
            response.EnsureSuccessStatusCode();
            var createdPedido = await response.Content.ReadFromJsonAsync<Pedido>();
            Assert.NotNull(createdPedido);
            Assert.Equal(StatusPedido.Aprovado, createdPedido.Status);
            Assert.Equal(59.98m, createdPedido.ValorTotal);
        }

        [Fact]
        public async Task PostPedido_ReturnsBadRequest_WhenNoItems()
        {
            // Arrange
            var pedido = new Pedido
            {
                Itens = new List<ItemPedido>()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/pedidos", pedido);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostPedido_ReturnsBadRequest_WhenInsufficientStock()
        {
            // Arrange
            var produtoEstoque = new ProdutoEstoqueDto
            {
                Id = 1,
                Nome = "Produto Teste",
                Descricao = "Descrição",
                Preco = 29.99m,
                Quantidade = 1 // Estoque insuficiente
            };

            _mockEstoqueService.Setup(x => x.VerificarEstoqueEPrecoAsync(1))
                .ReturnsAsync(produtoEstoque);

            var pedido = new Pedido
            {
                Itens = new List<ItemPedido>
                {
                    new ItemPedido
                    {
                        ProdutoId = 1,
                        Quantidade = 2, // Quantidade maior que estoque
                        PrecoUnitario = 29.99m
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/pedidos", pedido);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetPedido_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            // Act
            var response = await _client.GetAsync("/api/pedidos/999");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    public class ProdutoEstoqueDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal Preco { get; set; }
    }
}


