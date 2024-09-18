using HtmlAgilityPack;
using ScrapThat.Data;
using ScrapThat.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Web;
using System.Text.RegularExpressions;
using Polly.Retry;
using Polly;
using System.Net;


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
                int pageNumber = 1;
                bool hasNextPage = true;

                while (hasNextPage)
                {
                    var links = $"{url}/p{pageNumber}/c";

                    //workaround to be able to compare first page with finalUrl
                    if (pageNumber == 1)
                    {
                        links = $"{url}/c";
                    }

                    var request = new HttpRequestMessage(HttpMethod.Get, links);
                    HttpResponseMessage response = null;

                    try
                    {
                        response = await _httpClient.SendAsync(request);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occured while sending the request: {ex.Message}");
                        throw;
                    }


                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Console.WriteLine("Captcha detected. Please solve it in the browser to continue");

                        bool captchaSolved = false;
                        int maxRetry = 500;
                        int retryInterval = 5;

                        for (int i = 0; i < maxRetry; i += retryInterval)
                        {
                            Console.Beep();
                            Console.WriteLine("Waiting for Captcha to be solved....");
                            await Task.Delay(retryInterval * 1000);

                            request.Dispose();

                            request = new HttpRequestMessage(HttpMethod.Get, links);

                            response = await _httpClient.SendAsync(request);

                            if (response.IsSuccessStatusCode)
                            {
                                captchaSolved = true;
                                Console.WriteLine("Captcha solved, resuming scraping");
                                break;
                            }
                        }

                        if (!captchaSolved)
                        {
                            Console.WriteLine("Captcha was not solved in 5 minutes. Stopping...");
                            break;
                        }
                    }


                    if (response.IsSuccessStatusCode)
                    {
                        var headRequest = new HttpRequestMessage(HttpMethod.Head, links);

                        var headResponse = await _httpClient.SendAsync(headRequest);

                        var finalUrl = headResponse.RequestMessage.RequestUri.ToString();

                        if (!finalUrl.Equals(links, StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }

                        var web = new HtmlWeb();
                        var doc = await web.LoadFromWebAsync(links);
                        var products = doc.DocumentNode.SelectNodes("//div[@class='card-item card-standard js-product-data js-card-clickable ']");

                        if (products == null || products.Count == 0)
                        {
                            hasNextPage = false;
                            break;
                        }

                        await ScrapeWebsiteAsync(links);

                        pageNumber++;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to retrieve page {pageNumber}. Status code: {response.StatusCode}");
                        break;
                    }
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
                        var image = string.Empty;
                        var websiteUrl = product.GetAttributeValue("data-url", string.Empty);
                        var categoryName = product.GetAttributeValue("data-category-name", string.Empty);

                        var titleNode = product.SelectSingleNode(".//h2[@class='card-v2-title-wrapper']//a");
                        var titleText = titleNode?.InnerText.Trim();

                        if (titleText != null && titleText.Contains("RESIGILAT", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (priceNode == null)
                        {
                            var otherPriceNode = product.SelectSingleNode(".//p[@class='product-new-price unfair-price']");
                            var otherIntegerPartNode = otherPriceNode.SelectSingleNode(".//text()[normalize-space()]");
                            var otherDecimalPartNode = otherPriceNode.SelectSingleNode(".//sup");

                            var priceText = HttpUtility.HtmlDecode(otherPriceNode.InnerText);
                            if (priceText.Contains("de la"))
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
                        if (imageNode != null)
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


                        existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
                        if (existingProduct == null)
                        {
                            existingProduct = new Product
                            {
                                ProductId = productId,
                                Price = price,
                                Name = name,
                                Currency = currency,
                                Image = image,
                                DateChecked = DateTime.Today.Date,
                                WebsiteUrl = websiteUrl,
                                CategoryName = categoryName
                                
                            };
                            _context.Products.Add(existingProduct);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            bool isUpdated = false;

                            if(existingProduct.Price != price)
                            {
                                existingProduct.Price = price;
                                isUpdated = true;
                            }
                            if(existingProduct.Image != image)
                            {
                                existingProduct.Image = image;
                                isUpdated = true;
                            }

                            existingProduct.DateChecked = DateTime.Today.Date;

                            if(isUpdated)
                            {
                                _context.Products.Update(existingProduct);
                            }
                            await _context.SaveChangesAsync();
                        }


                        var today = DateTime.Today.Date;

                        var existingPriceHistory = await _context.ProductPriceHistories
                                    .FirstOrDefaultAsync(p => p.ProductId == existingProduct.ProductId && p.DateChecked == today);

                        if (existingPriceHistory == null)
                        {
                            var productPriceHistory = new ProductPriceHistory
                            {
                               
                                ProductId = existingProduct.ProductId,
                                Price = price,
                                DateChecked = DateTime.Today.Date,
                                WebsiteUrl = websiteUrl
                            };

                            _context.ProductPriceHistories.Add(productPriceHistory);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            existingPriceHistory.Price = price;
                            await _context.SaveChangesAsync();
                        }
                    }
                }

            }
        }
    }
}
