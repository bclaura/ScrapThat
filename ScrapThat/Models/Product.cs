namespace ScrapThat.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Currency { get; set; }
        public string Image { get; set; }
        public DateTime DateChecked { get; set; }

        public ICollection<ProductPriceHistory> PriceHistories { get; set; }


    }
}
