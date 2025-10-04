// src/Estoque.API/Data/EstoqueDbContext.cs

using Estoque.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Estoque.API.Data
{
    public class EstoqueDbContext : DbContext
    {
        public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options)
            : base(options)
        {
        }

        public DbSet<Produto> Produtos { get; set; }

        // ADICIONE ESTE MÉTODO
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configura a propriedade 'Preco' da entidade 'Produto'.
            // Define o tipo de coluna SQL como DECIMAL(18, 2), que é um padrão seguro para dinheiro.
            modelBuilder.Entity<Produto>()
                .Property(p => p.Preco)
                .HasColumnType("decimal(18, 2)"); 
            
            base.OnModelCreating(modelBuilder);
        }
    }
}