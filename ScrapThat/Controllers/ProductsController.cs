using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrapThat.Data;
using ScrapThat.Models;

namespace ScrapThat.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(int page = 1, int pageSize = 60)
        {
            var products = await _context.Products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.Products.CountAsync(x => x.Currency == "Lei");

            if (products == null || products.Count == 0)
            {
                return NoContent();
            }

            return Ok(new {products, totalCount});
        }

        [HttpGet("laptops")]
        public async Task<IActionResult> GetLaptops(int page = 1, int pageSize = 60)
        {
            var products = await _context.Products
                .Where(x => x.CategoryName == "Laptop / Notebook")
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.Products.CountAsync(x => x.CategoryName == "Laptop / Notebook");

            if (products == null || products.Count == 0)
            {
                return NoContent();
            }

            return Ok(new { products, totalCount });
        }

        [HttpGet("phones")]
        public async Task<IActionResult> GetPhones(int page = 1, int pageSize = 60)
        {
            var products = await _context.Products
                .Where(x => x.CategoryName == "Telefoane Mobile")
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.Products.CountAsync(x => x.CategoryName == "Telefoane Mobile");

            if (products == null || products.Count == 0)
            {
                return NoContent();
            }
            return Ok(new {products, totalCount});
        }

        [HttpGet("tablets")]
        public async Task<IActionResult> GetTablets(int page = 1, int pageSize = 60)
        {
            var products = await _context.Products
                .Where(x => x.CategoryName == "Tablete")
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.Products.CountAsync(x => x.CategoryName == "Tablete");

            if (products == null || products.Count == 0)
            {
                return NoContent();
            }

            return Ok(new {products, totalCount});
        }

        [HttpGet("smartwatches")]
        public async Task<IActionResult> GetSmartwatches(int page = 1, int pageSize = 60)
        {
            var products = await _context.Products
                .Where(x => x.CategoryName == "Smartwatch")
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.Products.CountAsync(x => x.CategoryName == "Smartwatch");

            if (products == null || products.Count == 0)
            {
                return NoContent();
            }
            return Ok(new {products, totalCount});
        }

        [HttpGet("fitness-bracelets")]
        public async Task<IActionResult> GetFitnessBracelets(int page = 1, int pageSize = 60)
        {
            var products = await _context.Products
                .Where(x => x.CategoryName == "Bratari fitness")
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.Products.CountAsync(x => x.CategoryName == "Bratari fitness");

            if (products == null || products.Count == 0)
            {
                return NoContent();
            }
            return Ok(new {products, totalCount});
        }

        [HttpGet("wireless-headphones")]
        public async Task<IActionResult> GetWirelessHeadphones(int page = 1, int pageSize = 60)
        {
            var products = await _context.Products
                .Where(x => x.CategoryName == "Casti Wireless")
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.Products.CountAsync(x => x.CategoryName == "Casti Wireless");

            if (products == null || products.Count == 0)
            {
                return NoContent();
            }
            return Ok(new {products, totalCount});
        }

        [HttpGet("games")]
        public async Task<IActionResult> GetGames(string platform = "", int page = 1, int pageSize = 60)
        {
            var query = _context.Products.Where(x => x.CategoryName == "Jocuri Consola &amp; PC");
            
            if(!string.IsNullOrEmpty(platform))
            {
                query = query.Where(x => x.Platform == platform);
            }

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await query.CountAsync();

            if (products == null || products.Count == 0)
            {
                return NoContent();
            }
            return Ok(new {products, totalCount });
        }

        [HttpGet("manga")]
        public async Task<IActionResult> GetManga(int page = 1, int pageSize = 60)
        {
            var products = await _context.Products
                .Where(x => x.CategoryName == "Benzi desenate")
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.Products.CountAsync(x => x.CategoryName == "Benzi desenate");

            if (products == null || products.Count == 0)
            {
                return NoContent();
            }
            return Ok(new {products, totalCount});
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

            if(product == null)
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

            return Ok(new {products, totalCount});
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
