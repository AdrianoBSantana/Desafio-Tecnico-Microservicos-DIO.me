using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Shared.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Configuração dos Serviços ---

// Adiciona o serviço de geração de Token para ser injetado no AuthController.
builder.Services.AddScoped<JwtService>();

// Centraliza a configuração de autenticação JWT usando o método do serviço compartilhado.
JwtService.ConfigureJwtAuthentication(builder.Services, builder.Configuration);

// Carrega a configuração do Ocelot a partir do arquivo ocelot.json.
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

// Configura a política de CORS para permitir requisições de qualquer origem.
// ATENÇÃO: Em produção, restrinja para os domínios conhecidos.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Adiciona suporte a controllers (para o AuthController) e Swagger.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Configuração do Pipeline de Requisições (Middleware) ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Habilita a política de CORS definida acima.
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Mapeia os controllers locais do Gateway (ex: /auth/login).
app.MapControllers();

// Habilita o middleware do Ocelot para roteamento. Deve ser um dos últimos.
await app.UseOcelot();

app.Run();
