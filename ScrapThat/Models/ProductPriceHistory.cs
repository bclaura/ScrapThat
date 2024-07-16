namespace ScrapThat.Models
{
    public class ProductPriceHistory
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public double Price { get; set; }
        public DateTime DateChecked { get; set; }
        public Product Product { get; set; }
    }
}
