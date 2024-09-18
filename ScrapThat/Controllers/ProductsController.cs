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
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            if (products == null || products.Count == 0)
            {
                return NoContent();
            }

            return Ok(products);
        }

        [HttpGet("laptops")]
        public async Task<ActionResult<IEnumerable<Product>>> GetLaptops()
        {
            var products = await _context.Products.Where(x => x.CategoryName == "Laptop / Notebook").ToListAsync();
            if (products == null || products.Count == 0)
            {
                return NoContent();
            }

            return Ok(products);
        }

        [HttpGet("phones")]
        public async Task<ActionResult<IEnumerable<Product>>> GetPhones()
        {
            var products = await _context.Products.Where(x => x.CategoryName == "Telefoane Mobile").ToListAsync();
            if (products == null || products.Count == 0)
            {
                return NoContent();
            }
            return Ok(products);
        }

        [HttpGet("tablets")]
        public async Task<ActionResult<IEnumerable<Product>>> GetTablets()
        {
            var products = await _context.Products.Where(x => x.CategoryName == "Tablete").ToListAsync();
            if (products == null || products.Count == 0)
            {
                return NoContent();
            }
            return Ok(products);
        }

        [HttpGet("smartwatches")]
        public async Task<ActionResult<IEnumerable<Product>>> GetSmartwatches()
        {
            var products = await _context.Products.Where(x => x.CategoryName == "Smartwatch").ToListAsync();
            if (products == null || products.Count == 0)
            {
                return NoContent();
            }
            return Ok(products);
        }

        [HttpGet("fitness-bracelets")]
        public async Task<ActionResult<IEnumerable<Product>>> GetFitnessBracelets()
        {
            var products = await _context.Products.Where(x => x.CategoryName == "Bratari fitness").ToListAsync();
            if (products == null || products.Count == 0)
            {
                return NoContent();
            }
            return Ok(products);
        }

        [HttpGet("wireless-headphones")]
        public async Task<ActionResult<IEnumerable<Product>>> GetWirelessHeadphones()
        {
            var products = await _context.Products.Where(x => x.CategoryName == "Casti Wireless").ToListAsync();
            if (products == null || products.Count == 0)
            {
                return NoContent();
            }
            return Ok(products);
        }

        [HttpGet("games")]
        public async Task<ActionResult<IEnumerable<Product>>> GetGames()
        {
            var products = await _context.Products.Where(x => x.CategoryName == "Jocuri Consola &amp; PC").ToListAsync();
            if (products == null || products.Count == 0)
            {
                return NoContent();
            }
            return Ok(products);
        }

        [HttpGet("manga")]
        public async Task<ActionResult<IEnumerable<Product>>> GetManga()
        {
            var products = await _context.Products.Where(x => x.CategoryName == "Benzi desenate").ToListAsync();
            if (products == null || products.Count == 0)
            {
                return NoContent();
            }
            return Ok(products);
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

    }
}
