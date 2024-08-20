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
                    if(pageNumber == 100)
                    {

                    }
                }
            }
        }

        public async Task ScrapeWebsiteAsync(string url)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            var count = 0;

            var products = doc.DocumentNode.SelectNodes("//div[@class='card-item card-standard js-product-data js-card-clickable ']");

            if (products != null)
            {
                foreach (var product in products)
                {
                    count++;

                    var productIdString = product.GetAttributeValue("data-product-id", string.Empty);
                    int productId;

                    if (int.TryParse(productIdString, out productId))
                    {
                        Product? existingProduct = null;
                        var name = product.GetAttributeValue("data-name", string.Empty);
                        var priceNode = product.SelectSingleNode(".//p[@class='product-new-price']");
                        double price = 0;
                        var image = string.Empty;

                        var titleNode = product.SelectSingleNode(".//h2[@class='card-v2-title-wrapper']//a");
                        var titleText = titleNode?.InnerText.Trim();

                        if(titleText != null && titleText.Contains("RESIGILAT", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (priceNode == null)
                        {
                            var otherPriceNode = product.SelectSingleNode(".//p[@class='product-new-price unfair-price']");
                            var otherIntegerPartNode = otherPriceNode.SelectSingleNode(".//text()[normalize-space()]");
                            var otherDecimalPartNode = otherPriceNode.SelectSingleNode(".//sup");

                            var priceText = HttpUtility.HtmlDecode(otherPriceNode.InnerText);
                            if(priceText.Contains("de la"))
                            {

                            }
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
                            if (priceString.Contains("de la"))
                            {

                            }

                            price = double.Parse(priceString, CultureInfo.InvariantCulture);
                        }


                        var currency = "Lei";

                        var imageNode = product.SelectSingleNode(".//div[contains(@class, 'img-component')]//img");
                        if(imageNode != null )
                        {
                            image = imageNode.GetAttributeValue("src", string.Empty);
                        }
                        else
                        {
                            var bundleImageNode = product.SelectSingleNode(".//div[contains(@class, 'bundle-image')]");
                            if (bundleImageNode != null)
                            {
                                var styleAttribute = bundleImageNode.GetAttributeValue("style", string.Empty);
                                var urlPattern = @"url\((?<url>[^)]+)\)";
                                var matches = Regex.Matches(styleAttribute, urlPattern);

                                foreach (Match match in matches)
                                {
                                    var imageUrl = match.Groups["url"].Value;
                                    image += imageUrl + "; ";
                                }
                                image = image.TrimEnd(';', ' ');
                            }
                        }
                        

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
