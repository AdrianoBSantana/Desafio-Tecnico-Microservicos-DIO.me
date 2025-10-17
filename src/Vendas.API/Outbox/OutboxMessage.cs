using System;

namespace Vendas.API.Outbox
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public DateTime OccurredOn { get; set; }
        public required string Type { get; set; }
        public required string Content { get; set; }
        public bool Processed { get; set; }
        public DateTime? ProcessedOn { get; set; }
    }
}