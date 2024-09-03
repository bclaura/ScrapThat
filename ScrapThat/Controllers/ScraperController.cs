using ScrapThat.Services;
using Microsoft.AspNetCore.Mvc;

namespace ScrapThat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScraperController : ControllerBase
    {
        private readonly ScraperService _scraperService;

        public ScraperController(ScraperService scraperService)
        {
            _scraperService = scraperService;
        }

        [HttpPost("scrape-all")]
        public async Task<IActionResult> ScrapeAll()
        {
            try
            {
                await _scraperService.ScrapeProducts();
                return Ok("Scraping initated for all URLs");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}