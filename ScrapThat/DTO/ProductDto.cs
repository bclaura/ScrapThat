namespace ScrapThat.DTO
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Currency { get; set; }
        public string Image { get; set; }
        public string WebsiteUrl { get; set; }
        public string CategoryName { get; set; }
        public DateTime DateChecked { get; set; }
        public string Platform { get; set; }
        public double OldestPrice { get; set; }
        public double PercentageChange { get; set; }
    }
}