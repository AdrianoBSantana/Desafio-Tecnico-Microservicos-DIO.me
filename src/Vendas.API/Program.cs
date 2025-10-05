// Topo do Program.cs
using Vendas.API.Data;
using Microsoft.EntityFrameworkCore;

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