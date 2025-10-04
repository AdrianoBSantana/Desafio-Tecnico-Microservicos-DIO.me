// Topo do Program.cs

using Estoque.API.Data;
using Microsoft.EntityFrameworkCore;
// Os seguintes são essenciais para o Web API funcionar:
using Microsoft.AspNetCore.Builder; 
using Microsoft.Extensions.DependencyInjection; 
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

// =================================================================
// 2. ADICIONANDO SERVIÇOS (Configuration)
// =================================================================

// Adicionando o DbContext ao contêiner de Injeção de Dependência (DI)
// A string de conexão é lida a partir do arquivo appsettings.json
builder.Services.AddDbContext<EstoqueDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Adicionando serviços essenciais: Controllers e Swagger/OpenAPI
builder.Services.AddControllers(); // Necessário para usar Controllers MVC
builder.Services.AddEndpointsApiExplorer(); // Necessário para o Swagger
builder.Services.AddSwaggerGen();



var app = builder.Build();

// =================================================================
// 3. CONFIGURANDO O PIPELINE DE REQUISIÇÕES (Middleware)
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



// Roda a aplicação
app.Run();