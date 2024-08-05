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

        /*[HttpPost("scrape")]
        public async Task<IActionResult> Scrape([FromBody] string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var validatedUri))
            {
                return BadRequest("Invalid Url format");
            }

            try
            {
                await _scraperService.ScrapeWebsiteAsync(url);
                return Ok("Scraping initiated");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }*/

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