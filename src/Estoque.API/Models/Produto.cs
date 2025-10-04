namespace Estoque.API.Models
{
    public class Produto
    {
        // 1. Chave Primária (Primary Key)
        // Por convenção, o EF Core reconhece 'Id' ou '{NomeDaClasse}Id' como a chave primária.
        public Guid Id { get; set; }

        // 2. Informações Básicas do Produto
        public string Nome { get; set; }
        public string Descricao { get; set; }
        
        // 3. Preço e Estoque
        // Usamos 'decimal' para dinheiro para evitar erros de precisão que 'double' ou 'float' poderiam causar.
        public decimal Preco { get; set; }
        
        // Esta é a propriedade crítica para o Microserviço de Estoque.
        public int QuantidadeEmEstoque { get; set; }
    }
}