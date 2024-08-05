using HtmlAgilityPack;
using ScrapThat.Data;
using ScrapThat.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Web;
using System.Text.RegularExpressions;

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
            int pageNumber = 1;
            bool hasNextPage = true;
            var urls = File.ReadAllLines("urls.txt");

            foreach (var url in urls)
            {
                while(hasNextPage)
                {
                    var links = $"{url}/p{pageNumber}/c";
                    var web = new HtmlWeb();
                    var doc = await web.LoadFromWebAsync(links);
                    var products = doc.DocumentNode.SelectNodes("//div[@class='card-item card-standard js-product-data js-card-clickable ']");
                    
                    if(products == null || products.Count == 0)
                    {
                        hasNextPage = false;
                        break;
                    }

                    await ScrapeWebsiteAsync(links);
                    pageNumber++;
                }
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
                        Product? existingProduct = null;
                        var name = product.GetAttributeValue("data-name", string.Empty);
                        var priceNode = product.SelectSingleNode(".//p[@class='product-new-price']");
                        double price = 0;
                        if(priceNode == null)
                        {
                            var otherPriceNode = product.SelectSingleNode(".//p[@class='product-new-price unfair-price']");
                            var otherIntegerPartNode = otherPriceNode.SelectSingleNode(".//text()[normalize-space()]");
                            var otherDecimalPartNode = otherPriceNode.SelectSingleNode(".//sup");

                            var priceText = HttpUtility.HtmlDecode(otherPriceNode.InnerText);
                            var match = Regex.Match(priceText, @"\d+(\.\d+)?");

                            var integerPart = match.Value.Replace(".", string.Empty);
                            var decimalPart = HttpUtility.HtmlDecode(otherDecimalPartNode?.InnerText.Trim());
                            var noCommadecimalPart = decimalPart.Replace(",", string.Empty).Trim();
                            var priceString = $"{integerPart}.{noCommadecimalPart}";
                            price = double.Parse(priceString, CultureInfo.InvariantCulture);

                        }
                        else
                        {
                            var integerPartNode = priceNode.SelectSingleNode(".//text()[normalize-space()]");
                            var decimalPartNode = priceNode.SelectSingleNode(".//sup");
                            var integerPart = HttpUtility.HtmlDecode(integerPartNode?.InnerText.Trim()).Replace(".", string.Empty).Trim(); ;
                            var decimalPart = HttpUtility.HtmlDecode(decimalPartNode?.InnerText.Trim());
                            var noCommadecimalPart = decimalPart.Replace(",", string.Empty).Trim();
                            var priceString = $"{integerPart}.{noCommadecimalPart}";
                            price = double.Parse(priceString, CultureInfo.InvariantCulture);
                        }


                        var currency = "Lei";

                        var imageNode = product.SelectSingleNode(".//div[contains(@class, 'img-component')]//img");
                        var image = imageNode.GetAttributeValue("src", string.Empty);

                        existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
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
