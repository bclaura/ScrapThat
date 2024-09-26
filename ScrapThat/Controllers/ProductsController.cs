using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ScrapThat.Data;
using ScrapThat.DTO;
using ScrapThat.Models;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ScrapThat.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private static string CurrentCachePrefix = "";
        private static HashSet<string> CachedKeys = new HashSet<string>();

        public ProductsController(ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        private void ClearCacheWithPrefix(string prefix)
        {
            var keysToRemove = CachedKeys.Where(key => key.StartsWith(prefix));
            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                CachedKeys.Remove(key);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(int page = 1, int pageSize = 60, int days = 7, string sort = "")
        {
            string cachePrefix = "AllCache_";
            string cacheKey = $"{cachePrefix}{days}_{sort}";

            if (CurrentCachePrefix != cachePrefix)
            {
                ClearCacheWithPrefix(CurrentCachePrefix);
                CurrentCachePrefix = cachePrefix;
            }
 
            if (!_memoryCache.TryGetValue(cacheKey, out List<ProductDto> cachedProducts))
            {
                var currentDate = DateTime.Today;
                var pastDate = currentDate.AddDays(-days);


                var query = from product in _context.Products
                            join priceHistory in _context.ProductPriceHistories
                            on product.ProductId equals priceHistory.ProductId into pGroup
                            let oldestPrice = pGroup
                            .Where(p => p.DateChecked >= pastDate)
                            .OrderBy(p => p.DateChecked)
                            .FirstOrDefault()
                            select new ProductDto
                            {
                                ProductId = product.ProductId,
                                Name = product.Name,
                                Price = product.Price,
                                Currency = product.Currency,
                                Image = product.Image,
                                WebsiteUrl = product.WebsiteUrl,
                                CategoryName = product.CategoryName,
                                DateChecked = product.DateChecked,
                                Platform = product.Platform,
                                OldestPrice = (oldestPrice != null) ? oldestPrice.Price : 0,
                                PercentageChange = (oldestPrice != null && oldestPrice.Price != 0) ? 100 * ((product.Price - oldestPrice.Price) / oldestPrice.Price) : 0
                            };

                var unsortedProducts = await query.ToListAsync();

                cachedProducts = sort switch
                {
                    "highestIncrease" => unsortedProducts.OrderByDescending(p => p.PercentageChange).ToList(),
                    "highestDecrease" => unsortedProducts.OrderBy(p => p.PercentageChange).ToList(),
                    "lowestPrice" => unsortedProducts.OrderBy(p => p.Price).ToList(),
                    "highestPrice" => unsortedProducts.OrderByDescending(p => p.Price).ToList(),
                    _ => unsortedProducts

                };

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _memoryCache.Set(cacheKey, cachedProducts, cacheOptions);
                CachedKeys.Add(cacheKey);
            }

            var paginatedProducts = cachedProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalCount = cachedProducts.Count();

            if (!paginatedProducts.Any())
            {
                return NoContent();
            }

            var stats = _memoryCache.GetCurrentStatistics();
            if (stats != null)
            {
                Console.WriteLine($"Current Memory Cache Size: {stats.CurrentEstimatedSize}");
            }

            return Ok(new { products = paginatedProducts, totalCount });
        }

        [HttpGet("laptops")]
        public async Task<IActionResult> GetLaptops(int page = 1, int pageSize = 60, int days = 7, string sort = "")
        {
            string cachePrefix = "LaptopsCache_";
            string cacheKey = $"{cachePrefix}{days}_{sort}";

            if(CurrentCachePrefix != cachePrefix)
            {
                ClearCacheWithPrefix(CurrentCachePrefix);
                CurrentCachePrefix = cachePrefix;
            }

            if (!_memoryCache.TryGetValue(cacheKey, out List<ProductDto> cachedProducts))
            {
                var currentDate = DateTime.Today;
                var pastDate = currentDate.AddDays(-days);


                var query = from product in _context.Products
                            where product.CategoryName == "Laptop / Notebook"
                            join priceHistory in _context.ProductPriceHistories
                            on product.ProductId equals priceHistory.ProductId into pGroup
                            let oldestPrice = pGroup
                            .Where(p => p.DateChecked >= pastDate)
                            .OrderBy(p => p.DateChecked)
                            .FirstOrDefault()
                            select new ProductDto
                            {
                                ProductId = product.ProductId,
                                Name = product.Name,
                                Price = product.Price,
                                Currency = product.Currency,
                                Image = product.Image,
                                WebsiteUrl = product.WebsiteUrl,
                                CategoryName = product.CategoryName,
                                DateChecked = product.DateChecked,
                                Platform = product.Platform,
                                OldestPrice = (oldestPrice != null) ? oldestPrice.Price : 0,
                                PercentageChange = (oldestPrice != null && oldestPrice.Price != 0) ? 100 * ((product.Price - oldestPrice.Price) / oldestPrice.Price) : 0
                            };

                var unsortedProducts = await query.ToListAsync();

                cachedProducts = sort switch
                {
                    "highestIncrease" => unsortedProducts.OrderByDescending(p => p.PercentageChange).ToList(),
                    "highestDecrease" => unsortedProducts.OrderBy(p => p.PercentageChange).ToList(),
                    "lowestPrice" => unsortedProducts.OrderBy(p => p.Price).ToList(),
                    "highestPrice" => unsortedProducts.OrderByDescending(p => p.Price).ToList(),
                    _ => unsortedProducts

                };

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _memoryCache.Set(cacheKey, cachedProducts, cacheOptions);
                CachedKeys.Add(cacheKey);
            }

            var paginatedProducts = cachedProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalCount = cachedProducts.Count();

            if (!paginatedProducts.Any())
            {
                return NoContent();
            }

            return Ok(new { products = paginatedProducts, totalCount });
        }

        [HttpGet("phones")]
        public async Task<IActionResult> GetPhones(int page = 1, int pageSize = 60, int days = 7, string sort = "")
        {
            string cachePrefix = "PhonesCache_";
            string cacheKey = $"{cachePrefix}{days}_{sort}";

            if (CurrentCachePrefix != cachePrefix)
            {
                ClearCacheWithPrefix(CurrentCachePrefix);
                CurrentCachePrefix = cachePrefix;
            }

            if (!_memoryCache.TryGetValue(cacheKey, out List<ProductDto> cachedProducts))
            {
                var currentDate = DateTime.Today;
                var pastDate = currentDate.AddDays(-days);


                var query = from product in _context.Products
                            where product.CategoryName == "Telefoane Mobile"
                            join priceHistory in _context.ProductPriceHistories
                            on product.ProductId equals priceHistory.ProductId into pGroup
                            let oldestPrice = pGroup
                            .Where(p => p.DateChecked >= pastDate)
                            .OrderBy(p => p.DateChecked)
                            .FirstOrDefault()
                            select new ProductDto
                            {
                                ProductId = product.ProductId,
                                Name = product.Name,
                                Price = product.Price,
                                Currency = product.Currency,
                                Image = product.Image,
                                WebsiteUrl = product.WebsiteUrl,
                                CategoryName = product.CategoryName,
                                DateChecked = product.DateChecked,
                                Platform = product.Platform,
                                OldestPrice = (oldestPrice != null) ? oldestPrice.Price : 0,
                                PercentageChange = (oldestPrice != null && oldestPrice.Price != 0) ? 100 * ((product.Price - oldestPrice.Price) / oldestPrice.Price) : 0
                            };

                var unsortedProducts = await query.ToListAsync();

                cachedProducts = sort switch
                {
                    "highestIncrease" => unsortedProducts.OrderByDescending(p => p.PercentageChange).ToList(),
                    "highestDecrease" => unsortedProducts.OrderBy(p => p.PercentageChange).ToList(),
                    "lowestPrice" => unsortedProducts.OrderBy(p => p.Price).ToList(),
                    "highestPrice" => unsortedProducts.OrderByDescending(p => p.Price).ToList(),
                    _ => unsortedProducts

                };

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _memoryCache.Set(cacheKey, cachedProducts, cacheOptions);
                CachedKeys.Add(cacheKey);
            }

            var paginatedProducts = cachedProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalCount = cachedProducts.Count();

            if (!paginatedProducts.Any())
            {
                return NoContent();
            }

            return Ok(new { products = paginatedProducts, totalCount });
        }

        [HttpGet("tablets")]
        public async Task<IActionResult> GetTablets(int page = 1, int pageSize = 60, int days = 7, string sort = "")
        {
            string cachePrefix = "TabletsCache_";
            string cacheKey = $"{cachePrefix}{days}_{sort}";

            if (CurrentCachePrefix != cachePrefix)
            {
                ClearCacheWithPrefix(CurrentCachePrefix);
                CurrentCachePrefix = cachePrefix;
            }

            if (!_memoryCache.TryGetValue(cacheKey, out List<ProductDto> cachedProducts))
            {
                var currentDate = DateTime.Today;
                var pastDate = currentDate.AddDays(-days);


                var query = from product in _context.Products
                            where product.CategoryName == "Tablete"
                            join priceHistory in _context.ProductPriceHistories
                            on product.ProductId equals priceHistory.ProductId into pGroup
                            let oldestPrice = pGroup
                            .Where(p => p.DateChecked >= pastDate)
                            .OrderBy(p => p.DateChecked)
                            .FirstOrDefault()
                            select new ProductDto
                            {
                                ProductId = product.ProductId,
                                Name = product.Name,
                                Price = product.Price,
                                Currency = product.Currency,
                                Image = product.Image,
                                WebsiteUrl = product.WebsiteUrl,
                                CategoryName = product.CategoryName,
                                DateChecked = product.DateChecked,
                                Platform = product.Platform,
                                OldestPrice = (oldestPrice != null) ? oldestPrice.Price : 0,
                                PercentageChange = (oldestPrice != null && oldestPrice.Price != 0) ? 100 * ((product.Price - oldestPrice.Price) / oldestPrice.Price) : 0
                            };

                var unsortedProducts = await query.ToListAsync();

                cachedProducts = sort switch
                {
                    "highestIncrease" => unsortedProducts.OrderByDescending(p => p.PercentageChange).ToList(),
                    "highestDecrease" => unsortedProducts.OrderBy(p => p.PercentageChange).ToList(),
                    "lowestPrice" => unsortedProducts.OrderBy(p => p.Price).ToList(),
                    "highestPrice" => unsortedProducts.OrderByDescending(p => p.Price).ToList(),
                    _ => unsortedProducts

                };

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _memoryCache.Set(cacheKey, cachedProducts, cacheOptions);
                CachedKeys.Add(cacheKey);
            }

            var paginatedProducts = cachedProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalCount = cachedProducts.Count();

            if (!paginatedProducts.Any())
            {
                return NoContent();
            }

            return Ok(new { products = paginatedProducts, totalCount });
        }

        [HttpGet("smartwatches")]
        public async Task<IActionResult> GetSmartwatches(int page = 1, int pageSize = 60, int days = 7, string sort = "")
        {
            string cachePrefix = "SmartwatchCache_";
            string cacheKey = $"{cachePrefix}{days}_{sort}";

            if (CurrentCachePrefix != cachePrefix)
            {
                ClearCacheWithPrefix(CurrentCachePrefix);
                CurrentCachePrefix = cachePrefix;
            }

            if (!_memoryCache.TryGetValue(cacheKey, out List<ProductDto> cachedProducts))
            {
                var currentDate = DateTime.Today;
                var pastDate = currentDate.AddDays(-days);


                var query = from product in _context.Products
                            where product.CategoryName == "Smartwatch"
                            join priceHistory in _context.ProductPriceHistories
                            on product.ProductId equals priceHistory.ProductId into pGroup
                            let oldestPrice = pGroup
                            .Where(p => p.DateChecked >= pastDate)
                            .OrderBy(p => p.DateChecked)
                            .FirstOrDefault()
                            select new ProductDto
                            {
                                ProductId = product.ProductId,
                                Name = product.Name,
                                Price = product.Price,
                                Currency = product.Currency,
                                Image = product.Image,
                                WebsiteUrl = product.WebsiteUrl,
                                CategoryName = product.CategoryName,
                                DateChecked = product.DateChecked,
                                Platform = product.Platform,
                                OldestPrice = (oldestPrice != null) ? oldestPrice.Price : 0,
                                PercentageChange = (oldestPrice != null && oldestPrice.Price != 0) ? 100 * ((product.Price - oldestPrice.Price) / oldestPrice.Price) : 0
                            };

                var unsortedProducts = await query.ToListAsync();

                cachedProducts = sort switch
                {
                    "highestIncrease" => unsortedProducts.OrderByDescending(p => p.PercentageChange).ToList(),
                    "highestDecrease" => unsortedProducts.OrderBy(p => p.PercentageChange).ToList(),
                    "lowestPrice" => unsortedProducts.OrderBy(p => p.Price).ToList(),
                    "highestPrice" => unsortedProducts.OrderByDescending(p => p.Price).ToList(),
                    _ => unsortedProducts

                };

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _memoryCache.Set(cacheKey, cachedProducts, cacheOptions);
                CachedKeys.Add(cacheKey);
            }

            var paginatedProducts = cachedProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalCount = cachedProducts.Count();

            if (!paginatedProducts.Any())
            {
                return NoContent();
            }

            return Ok(new { products = paginatedProducts, totalCount });
        }

        [HttpGet("fitness-bracelets")]
        public async Task<IActionResult> GetFitnessBracelets(int page = 1, int pageSize = 60, int days = 7, string sort = "")
        {
            string cachePrefix = "FitnessCache_";
            string cacheKey = $"{cachePrefix}{days}_{sort}";

            if (CurrentCachePrefix != cachePrefix)
            {
                ClearCacheWithPrefix(CurrentCachePrefix);
                CurrentCachePrefix = cachePrefix;
            }

            if (!_memoryCache.TryGetValue(cacheKey, out List<ProductDto> cachedProducts))
            {
                var currentDate = DateTime.Today;
                var pastDate = currentDate.AddDays(-days);


                var query = from product in _context.Products
                            where product.CategoryName == "Bratari fitness"
                            join priceHistory in _context.ProductPriceHistories
                            on product.ProductId equals priceHistory.ProductId into pGroup
                            let oldestPrice = pGroup
                            .Where(p => p.DateChecked >= pastDate)
                            .OrderBy(p => p.DateChecked)
                            .FirstOrDefault()
                            select new ProductDto
                            {
                                ProductId = product.ProductId,
                                Name = product.Name,
                                Price = product.Price,
                                Currency = product.Currency,
                                Image = product.Image,
                                WebsiteUrl = product.WebsiteUrl,
                                CategoryName = product.CategoryName,
                                DateChecked = product.DateChecked,
                                Platform = product.Platform,
                                OldestPrice = (oldestPrice != null) ? oldestPrice.Price : 0,
                                PercentageChange = (oldestPrice != null && oldestPrice.Price != 0) ? 100 * ((product.Price - oldestPrice.Price) / oldestPrice.Price) : 0
                            };

                var unsortedProducts = await query.ToListAsync();

                cachedProducts = sort switch
                {
                    "highestIncrease" => unsortedProducts.OrderByDescending(p => p.PercentageChange).ToList(),
                    "highestDecrease" => unsortedProducts.OrderBy(p => p.PercentageChange).ToList(),
                    "lowestPrice" => unsortedProducts.OrderBy(p => p.Price).ToList(),
                    "highestPrice" => unsortedProducts.OrderByDescending(p => p.Price).ToList(),
                    _ => unsortedProducts

                };

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _memoryCache.Set(cacheKey, cachedProducts, cacheOptions);
                CachedKeys.Add(cacheKey);
            }

            var paginatedProducts = cachedProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalCount = cachedProducts.Count();

            if (!paginatedProducts.Any())
            {
                return NoContent();
            }

            return Ok(new { products = paginatedProducts, totalCount });
        }

        [HttpGet("wireless-headphones")]
        public async Task<IActionResult> GetWirelessHeadphones(int page = 1, int pageSize = 60, int days = 7, string sort = "")
        {
            string cachePrefix = "WirelessHeadphonesCache_";
            string cacheKey = $"{cachePrefix}{days}_{sort}";

            if (CurrentCachePrefix != cachePrefix)
            {
                ClearCacheWithPrefix(CurrentCachePrefix);
                CurrentCachePrefix = cachePrefix;
            }

            if (!_memoryCache.TryGetValue(cacheKey, out List<ProductDto> cachedProducts))
            {
                var currentDate = DateTime.Today;
                var pastDate = currentDate.AddDays(-days);


                var query = from product in _context.Products
                            where product.CategoryName == "Casti Wireless"
                            join priceHistory in _context.ProductPriceHistories
                            on product.ProductId equals priceHistory.ProductId into pGroup
                            let oldestPrice = pGroup
                            .Where(p => p.DateChecked >= pastDate)
                            .OrderBy(p => p.DateChecked)
                            .FirstOrDefault()
                            select new ProductDto
                            {
                                ProductId = product.ProductId,
                                Name = product.Name,
                                Price = product.Price,
                                Currency = product.Currency,
                                Image = product.Image,
                                WebsiteUrl = product.WebsiteUrl,
                                CategoryName = product.CategoryName,
                                DateChecked = product.DateChecked,
                                Platform = product.Platform,
                                OldestPrice = (oldestPrice != null) ? oldestPrice.Price : 0,
                                PercentageChange = (oldestPrice != null && oldestPrice.Price != 0) ? 100 * ((product.Price - oldestPrice.Price) / oldestPrice.Price) : 0
                            };

                var unsortedProducts = await query.ToListAsync();

                cachedProducts = sort switch
                {
                    "highestIncrease" => unsortedProducts.OrderByDescending(p => p.PercentageChange).ToList(),
                    "highestDecrease" => unsortedProducts.OrderBy(p => p.PercentageChange).ToList(),
                    "lowestPrice" => unsortedProducts.OrderBy(p => p.Price).ToList(),
                    "highestPrice" => unsortedProducts.OrderByDescending(p => p.Price).ToList(),
                    _ => unsortedProducts

                };

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _memoryCache.Set(cacheKey, cachedProducts, cacheOptions);
                CachedKeys.Add(cacheKey);
            }

            var paginatedProducts = cachedProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalCount = cachedProducts.Count();

            if (!paginatedProducts.Any())
            {
                return NoContent();
            }

            return Ok(new { products = paginatedProducts, totalCount });
        }

        [HttpGet("games")]
        public async Task<IActionResult> GetGames(string platform = "", int page = 1, int pageSize = 60, int days = 7, string sort = "")
        {
            string cachePrefix = "GamesCache_";
            string cacheKey = $"{cachePrefix}{days}_{sort}";

            if (CurrentCachePrefix != cachePrefix)
            {
                ClearCacheWithPrefix(CurrentCachePrefix);
                CurrentCachePrefix = cachePrefix;
            }

            if (!_memoryCache.TryGetValue(cacheKey, out List<ProductDto> cachedProducts))
            {
                var currentDate = DateTime.Today;
                var pastDate = currentDate.AddDays(-days);


                var query = from product in _context.Products
                            where product.CategoryName == "Jocuri Consola &amp; PC" && (string.IsNullOrEmpty(platform) || product.Platform == platform)
                            join priceHistory in _context.ProductPriceHistories
                            on product.ProductId equals priceHistory.ProductId into pGroup
                            let oldestPrice = pGroup
                            .Where(p => p.DateChecked >= pastDate)
                            .OrderBy(p => p.DateChecked)
                            .FirstOrDefault()
                            select new ProductDto
                            {
                                ProductId = product.ProductId,
                                Name = product.Name,
                                Price = product.Price,
                                Currency = product.Currency,
                                Image = product.Image,
                                WebsiteUrl = product.WebsiteUrl,
                                CategoryName = product.CategoryName,
                                DateChecked = product.DateChecked,
                                Platform = product.Platform,
                                OldestPrice = (oldestPrice != null) ? oldestPrice.Price : 0,
                                PercentageChange = (oldestPrice != null && oldestPrice.Price != 0) ? 100 * ((product.Price - oldestPrice.Price) / oldestPrice.Price) : 0
                            };

                var unsortedProducts = await query.ToListAsync();

                cachedProducts = sort switch
                {
                    "highestIncrease" => unsortedProducts.OrderByDescending(p => p.PercentageChange).ToList(),
                    "highestDecrease" => unsortedProducts.OrderBy(p => p.PercentageChange).ToList(),
                    "lowestPrice" => unsortedProducts.OrderBy(p => p.Price).ToList(),
                    "highestPrice" => unsortedProducts.OrderByDescending(p => p.Price).ToList(),
                    _ => unsortedProducts

                };

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _memoryCache.Set(cacheKey, cachedProducts, cacheOptions);
                CachedKeys.Add(cacheKey);
            }

            var paginatedProducts = cachedProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalCount = cachedProducts.Count();

            if (!paginatedProducts.Any())
            {
                return NoContent();
            }

            return Ok(new { products = paginatedProducts, totalCount });
        }


        [HttpGet("manga")]
        public async Task<IActionResult> GetManga(int page = 1, int pageSize = 60, int days = 7, string sort = "")
        {
            string cachePrefix = "MangaCache_";
            string cacheKey = $"{cachePrefix}{days}_{sort}";

            if (CurrentCachePrefix != cachePrefix)
            {
                ClearCacheWithPrefix(CurrentCachePrefix);
                CurrentCachePrefix = cachePrefix;
            }

            if (!_memoryCache.TryGetValue(cacheKey, out List<ProductDto> cachedProducts))
            {
                var currentDate = DateTime.Today;
                var pastDate = currentDate.AddDays(-days);


                var query = from product in _context.Products
                            where product.CategoryName == "Benzi desenate"
                            join priceHistory in _context.ProductPriceHistories
                            on product.ProductId equals priceHistory.ProductId into pGroup
                            let oldestPrice = pGroup
                            .Where(p => p.DateChecked >= pastDate)
                            .OrderBy(p => p.DateChecked)
                            .FirstOrDefault()
                            select new ProductDto
                            {
                                ProductId = product.ProductId,
                                Name = product.Name,
                                Price = product.Price,
                                Currency = product.Currency,
                                Image = product.Image,
                                WebsiteUrl = product.WebsiteUrl,
                                CategoryName = product.CategoryName,
                                DateChecked = product.DateChecked,
                                Platform = product.Platform,
                                OldestPrice = (oldestPrice != null) ? oldestPrice.Price : 0,
                                PercentageChange = (oldestPrice != null && oldestPrice.Price != 0) ? 100 * ((product.Price - oldestPrice.Price) / oldestPrice.Price) : 0
                            };

                var unsortedProducts = await query.ToListAsync();

                cachedProducts = sort switch
                {
                    "highestIncrease" => unsortedProducts.OrderByDescending(p => p.PercentageChange).ToList(),
                    "highestDecrease" => unsortedProducts.OrderBy(p => p.PercentageChange).ToList(),
                    "lowestPrice" => unsortedProducts.OrderBy(p => p.Price).ToList(),
                    "highestPrice" => unsortedProducts.OrderByDescending(p => p.Price).ToList(),
                    _ => unsortedProducts

                };

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _memoryCache.Set(cacheKey, cachedProducts, cacheOptions);
                CachedKeys.Add(cacheKey);
            }

            var paginatedProducts = cachedProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalCount = cachedProducts.Count();

            if (!paginatedProducts.Any())
            {
                return NoContent();
            }

            return Ok(new { products = paginatedProducts, totalCount });
        }

        [HttpGet("pricehistory/{productId}")]
        public async Task<ActionResult<IEnumerable<ProductPriceHistory>>> GetPriceHistory(int productId)
        {
            var priceHistory = await _context.ProductPriceHistories.Where(x => x.ProductId == productId)
                                                                   .OrderBy(x => x.DateChecked)
                                                                   .ToListAsync();
            if (priceHistory == null || !priceHistory.Any())
            {
                return NotFound();
            }
            return Ok(priceHistory);
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<Product>> GetProductDetail(int productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == productId);

            if (product == null)
            {
                return NoContent();
            }
            return Ok(product);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts(string query, int page = 1, int pageSize = 60)
        {
            var products = await _context.Products.Where(p => p.Name.Contains(query))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.Products.CountAsync(p => p.Name.Contains(query));

            return Ok(new { products, totalCount });
        }

        //for future reference
        /*[HttpDelete("delete-games")]
        public async Task<IActionResult> DeleteGamesWithPriceHistory()
        {
            var games = await _context.Products.Where(x => x.CategoryName == "Jocuri Consola &amp; PC").ToListAsync();

            var gameProductIds = games.Select(y => y.ProductId).ToList();

            var priceHistories = await _context.ProductPriceHistories.Where(z => gameProductIds.Contains(z.ProductId)).ToListAsync();

            _context.ProductPriceHistories.RemoveRange(priceHistories);

            _context.Products.RemoveRange(games);

            await _context.SaveChangesAsync();

            return Ok("Deleted all games and their price history.");
        }*/

    }
}
