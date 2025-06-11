using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace FurnitureAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FurnitureController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public FurnitureController(HttpClient httpClient)  // <- fixed constructor name
        {
            _httpClient = httpClient;
        }

        [HttpPost("detect")]
        public async Task<IActionResult> Predict([FromForm] IFormFile image, [FromForm] string room_type)
        {
            if (image == null || string.IsNullOrEmpty(room_type))
                return BadRequest("Image and object name are required.");

            try
            {
                // Read uploaded image into byte array
                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                var imageBytes = ms.ToArray();

                // Prepare request content
                var content = new MultipartFormDataContent();
                var imageContent = new ByteArrayContent(imageBytes);
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse(image.ContentType);
                content.Add(imageContent, "file", image.FileName);
                content.Add(new StringContent(room_type), "room_type");

                // Send request to model API
                var response = await _httpClient.PostAsync("https://native-tick-delicate.ngrok-free.app/detect", content);

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, new { error = await response.Content.ReadAsStringAsync() });

                // Extract headers
                response.Headers.TryGetValues("X-Detected-Items", out var detectedItems);
                response.Headers.TryGetValues("X-Missing-Items", out var missingItems);

                // Get image bytes and content type
                var imageResponse = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.MediaType;

                if (string.IsNullOrEmpty(contentType))
                    return StatusCode(500, new { error = "Model response missing valid Content-Type." });

                // Return image + custom headers
                //Response.Headers.Add("X-Detected-Items", detectedItems?.FirstOrDefault() ?? "N/A");
                //Response.Headers.Add("X-Missing-Items", missingItems?.FirstOrDefault() ?? "N/A");
                Response.Headers.Append("X-Detected-Items", detectedItems?.FirstOrDefault() ?? "N/A");
                Response.Headers.Append("X-Missing-Items", missingItems?.FirstOrDefault() ?? "N/A");

                return File(imageResponse, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
