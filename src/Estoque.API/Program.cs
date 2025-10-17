using Estoque.API.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Middleware;
using Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// Configura os serviços que serão injetados.

// Adiciona o DbContext para interagir com o banco de dados usando Entity Framework.
// A string de conexão é lida do appsettings.json.
builder.Services.AddDbContext<EstoqueDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Centraliza a configuração de autenticação JWT usando o método do serviço compartilhado.
JwtService.ConfigureJwtAuthentication(builder.Services, builder.Configuration);

// Adiciona suporte para controllers e documentação de API (Swagger).
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registra os serviços do RabbitMQ para comunicação assíncrona.
builder.Services.AddSingleton<RabbitMQService>();
builder.Services.AddHostedService<Estoque.API.Services.EstoqueRabbitMQService>();

// Configura os health checks para monitorar a saúde do banco de dados e do RabbitMQ.
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck<EstoqueDbContext>>("database")
    .AddCheck<RabbitMQHealthCheck>("rabbitmq");

var app = builder.Build();

// Configura o pipeline de requisições HTTP (middlewares).

// Em ambiente de desenvolvimento, habilita o Swagger para documentação e teste da API.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Middleware customizado para logar as requisições e respostas.
app.UseMiddleware<LoggingMiddleware>();

// Habilita autenticação e autorização para as requisições.
app.UseAuthentication();
app.UseAuthorization();

// Mapeia o endpoint /health para os health checks.
app.MapHealthChecks("/health");

// Mapeia as rotas para os controllers.
app.MapControllers();

app.Run();