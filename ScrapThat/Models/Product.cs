using System.ComponentModel.DataAnnotations;

namespace ScrapThat.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; } //primary key
        public int ProductId { get; set; } //product id
        public string Name { get; set; }
        public double Price { get; set; }
        public string Currency { get; set; }
        public string Image { get; set; }
        public DateTime DateChecked { get; set; }
        public string WebsiteUrl { get; set; }
        public string CategoryName { get; set; }
        public string Platform { get; set; }

    }
}
