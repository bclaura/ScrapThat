using HtmlAgilityPack;
using ScrapThat.Data;
using ScrapThat.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Web;

namespace ScrapThat.Services
{
    public class ScraperService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public ScraperService(ApplicationDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task ScrapeProducts()
        {
            var urls = File.ReadAllLines("urls.txt");

            foreach (var url in urls)
            {
                await ScrapeWebsiteAsync(url);
            }
        }

        public async Task ScrapeWebsiteAsync(string url)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);

            var products = doc.DocumentNode.SelectNodes("//div[@class='card-item card-standard js-product-data js-card-clickable ']");

            if (products != null)
            {
                foreach (var product in products)
                {
                    var productIdString = product.GetAttributeValue("data-product-id", string.Empty);
                    int productId;

                    if (int.TryParse(productIdString, out productId))
                    {

                        var name = product.GetAttributeValue("data-name", string.Empty);
                        var priceNode = product.SelectSingleNode(".//p[@class='product-new-price']");
                        var integerPartNode = priceNode.SelectSingleNode(".//text()[normalize-space()]");
                        var decimalPartNode = priceNode.SelectSingleNode(".//sup");

                        var integerPart = HttpUtility.HtmlDecode(integerPartNode?.InnerText.Trim()).Replace(".", string.Empty).Trim(); ;
                        var decimalPart = HttpUtility.HtmlDecode(decimalPartNode?.InnerText.Trim());
                        var noCommadecimalPart = decimalPart.Replace(",", string.Empty).Trim();
                        var priceString = $"{integerPart}.{noCommadecimalPart}";
                        var price = double.Parse(priceString, CultureInfo.InvariantCulture);

                        var currency = "Lei";

                        var imageNode = product.SelectSingleNode(".//div[contains(@class, 'img-component')]//img");
                        var image = imageNode.GetAttributeValue("src", string.Empty);

                        var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
                        if (existingProduct == null)
                        {
                            existingProduct = new Product
                            {
                                Name = name,
                                Currency = currency,
                                Image = image,
                                DateChecked = DateTime.Now
                            };
                            _context.Products.Add(existingProduct);
                            await _context.SaveChangesAsync();
                        }

                        var today = DateTime.Today;

                        var existingPriceHistory = await _context.ProductPriceHistories
                                    .FirstOrDefaultAsync(p => p.ProductId == existingProduct.Id && p.DateChecked == today);

                        if (existingPriceHistory != null)
                        {
                            existingPriceHistory.Price = price;
                        }
                        else
                        {
                            var productPriceHistory = new ProductPriceHistory
                            {
                                ProductId = existingProduct.Id,
                                Price = price,
                                DateChecked = DateTime.Now
                            };

                            _context.ProductPriceHistories.Add(productPriceHistory);
                        }

                        await _context.SaveChangesAsync();
                    }
                }

            }
        }
    }
}
