using System.ComponentModel.DataAnnotations;

namespace ScrapThat.Models
{
    public class ProductPriceHistory
    {
        [Key]
        public int Id { get; set; } //primary key
        public int ProductId { get; set; } //product id
        public double Price { get; set; }
        public DateTime DateChecked { get; set; }

        public string WebsiteUrl { get; set; }
    }
}
