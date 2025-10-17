using System;

namespace Estoque.API.Models
{
    public class ProcessedOrder
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public DateTime ProcessedOn { get; set; }
    }
}
