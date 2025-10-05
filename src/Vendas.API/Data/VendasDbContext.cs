// src/Vendas.API/Data/VendasDbContext.cs

using Vendas.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Vendas.API.Data
{
    public class VendasDbContext : DbContext
    {
        public VendasDbContext(DbContextOptions<VendasDbContext> options)
            : base(options)
        {
        }

        // Tabela para os Pedidos
        public DbSet<Pedido> Pedidos { get; set; }
        
        // Tabela para os Itens de Pedido
        public DbSet<ItemPedido> ItensPedido { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Garante que o Entity Framework trate a precisão monetária corretamente
            modelBuilder.Entity<Pedido>()
                .Property(p => p.ValorTotal)
                .HasColumnType("decimal(18, 2)");
            
            // Não precisamos definir HasColumnType aqui pois já definimos na própria classe ItemPedido

            base.OnModelCreating(modelBuilder);
        }
    }
}