using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Shared.Services
{
    /// <summary>
    /// Implementação de IHealthCheck para verificar a saúde de um banco de dados via DbContext.
    /// É genérico e pode ser usado para qualquer DbContext do Entity Framework.
    /// </summary>
    /// <typeparam name="TContext">O tipo do DbContext a ser verificado.</typeparam>
    public class DatabaseHealthCheck<TContext> : IHealthCheck where TContext : DbContext
    {
        private readonly TContext _context;
        private readonly ILogger<DatabaseHealthCheck<TContext>> _logger;

        public DatabaseHealthCheck(TContext context, ILogger<DatabaseHealthCheck<TContext>> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executa uma consulta simples ("SELECT 1") para verificar se o banco de dados está acessível.
        /// </summary>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext healthCheckContext, CancellationToken cancellationToken = default)
        {
            try
            {
                // Tenta executar um comando SQL bruto que é rápido e não depende de tabelas.
                await _context.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
                _logger.LogInformation("Database health check for {DbContext} passed.", typeof(TContext).Name);
                return HealthCheckResult.Healthy("O banco de dados está acessível.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check for {DbContext} failed.", typeof(TContext).Name);
                return HealthCheckResult.Unhealthy("O banco de dados está inacessível.", ex);
            }
        }
    }

    /// <summary>
    /// Implementação de IHealthCheck para verificar a saúde do servidor RabbitMQ.
    /// </summary>
    public class RabbitMQHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMQHealthCheck> _logger;

        public RabbitMQHealthCheck(IConfiguration configuration, ILogger<RabbitMQHealthCheck> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Tenta estabelecer uma conexão e criar um canal com o RabbitMQ para verificar sua disponibilidade.
        /// </summary>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext healthCheckContext, CancellationToken cancellationToken = default)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
                    Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                    UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
                    Password = _configuration["RabbitMQ:Password"] ?? "guest"
                };

                // A tentativa de criar conexão e canal já valida a saúde do serviço.
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();
                
                _logger.LogInformation("RabbitMQ health check passed.");
                return Task.FromResult(HealthCheckResult.Healthy("O RabbitMQ está acessível."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ health check failed.");
                return Task.FromResult(HealthCheckResult.Unhealthy("O RabbitMQ está inacessível.", ex));
            }
        }
    }
}