namespace Estoque.API.Models // <-- Ajuste seu namespace se for diferente
{
    public class Produto
    {
        public int Id { get; set; } // <-- ALTERADO DE GUID PARA INT
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal Preco { get; set; }
    }
}