// Topo do Program.cs
using Vendas.API.Data;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Vendas.API.Outbox;

var builder = WebApplication.CreateBuilder(args);

// =================================================================
// 1. ADICIONANDO SERVIÇOS (Configuration - Tudo em um só lugar)
// =================================================================

// Configuração do DbContext para Vendas, usando SQLite
builder.Services.AddDbContext<VendasDbContext>(options =>
{
    // Usando SQLite e lendo a Connection String do appsettings.json
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Adicionando suporte a Controllers e ao Swagger/OpenAPI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Necessário para o Swagger
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<EstoqueService>(client =>
{
    // ❗ A URL DO SEU SERVIÇO DE ESTOQUE (Verifique a porta do Estoque.API!)
    client.BaseAddress = new Uri("http://localhost:5065"); 
});

// Configura MassTransit: prefere RabbitMQ quando configurado, senão usa InMemory
var rabbitSection = builder.Configuration.GetSection("RabbitMQ");
builder.Services.AddMassTransit(x =>
{
    if (!string.IsNullOrWhiteSpace(rabbitSection.GetValue<string>("Host")))
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(rabbitSection.GetValue<string>("Host"), h =>
            {
                h.Username(rabbitSection.GetValue<string>("Username"));
                h.Password(rabbitSection.GetValue<string>("Password"));
            });
            cfg.ConfigureEndpoints(context);
        });
    }
    else
    {
        x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
    }
});

// Registrar o HostedService que publica mensagens outbox
builder.Services.AddHostedService<OutboxPublisherService>();


var app = builder.Build();

// =================================================================
// 2. CONFIGURANDO O PIPELINE DE REQUISIÇÕES (Middleware)
// =================================================================

// Configuração para uso do Swagger/OpenAPI em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Mapeia os controladores para receber requisições HTTP
app.MapControllers(); 

app.Run();