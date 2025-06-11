using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;

namespace FurnitureAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Model2Controller : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public Model2Controller(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpPost("generateObj")]
        public async Task<IActionResult> Predict([FromForm] IFormFile image, [FromForm] string objectName)
        {
            if (image == null || string.IsNullOrEmpty(objectName))
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
                content.Add(new StringContent(objectName), "object_name");

                // Send request to model API
                var response = await _httpClient.PostAsync("https://reindeer-enormous-mildly.ngrok-free.app/room", content);

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, new { error = await response.Content.ReadAsStringAsync() });

                // Get image bytes and original content-type
                var imageResponse = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.MediaType;

                if (string.IsNullOrEmpty(contentType))
                    return StatusCode(500, new { error = "Model response missing valid Content-Type." });


                return File(imageResponse, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
