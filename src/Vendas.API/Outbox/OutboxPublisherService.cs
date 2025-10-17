using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vendas.API.Data;

namespace Vendas.API.Outbox
{
	// Background service that periodically publishes unprocessed outbox messages
	public class OutboxPublisherService : BackgroundService
	{
		private readonly VendasDbContext _db;
		private readonly IPublishEndpoint _publisher;
		private readonly ILogger<OutboxPublisherService> _logger;
		private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

		public OutboxPublisherService(VendasDbContext db, IPublishEndpoint publisher, ILogger<OutboxPublisherService> logger)
		{
			_db = db;
			_publisher = publisher;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("OutboxPublisherService starting");

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					await PublishPending(stoppingToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Erro ao publicar mensagens outbox");
				}

				await Task.Delay(_interval, stoppingToken);
			}

			_logger.LogInformation("OutboxPublisherService stopping");
		}

		private async Task PublishPending(CancellationToken cancellationToken)
		{
			var pending = await _db.OutboxMessages
				.Where(o => !o.Processed)
				.OrderBy(o => o.OccurredOn)
				.ToListAsync(cancellationToken);

			if (!pending.Any()) return;

			foreach (var msg in pending)
			{
				try
				{
					_logger.LogInformation("Publishing outbox message {Id} of type {Type}", msg.Id, msg.Type);

					// Attempt to resolve type by known name. For this project we expect VendaRealizadaEvent
					if (msg.Type == "VendaRealizadaEvent")
					{
						// deserialize into shared event type
						var evt = JsonSerializer.Deserialize<Shared.Events.VendaRealizadaEvent>(msg.Content);
						if (evt != null)
						{
							await _publisher.Publish(evt, cancellationToken);
						}
					}
					else
					{
						// Fallback: publish raw envelope
						await _publisher.Publish(new RawOutboxMessage(msg.Type, msg.Content), cancellationToken);
					}

					msg.Processed = true;
					msg.ProcessedOn = DateTime.UtcNow;
					await _db.SaveChangesAsync(cancellationToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Falha ao publicar mensagem outbox {Id}", msg.Id);
					// do not rethrow - continue with other messages
				}
			}
		}
	}

	// Simple envelope used when type is unknown to the publisher
	public record RawOutboxMessage(string Type, string Content);
}

