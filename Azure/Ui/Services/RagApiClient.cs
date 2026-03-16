namespace Ui.Services
{
    public class RagApiClient
    {
        private readonly HttpClient _http;

        public RagApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> Ask(string question)
        {
            // Build the query string with proper URL encoding
            var url = $"query?q={Uri.EscapeDataString(question)}";

            // Send GET request
            var response = await _http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                // Extract your custom "validationResult.ErrorMessage"
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorBody ?? response.ReasonPhrase);
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> Upload(Stream fileStream, string fileName)
        {
            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(fileStream);
            content.Add(fileContent, "file", fileName);

            // Add it to the top-level request headers
            _http.DefaultRequestHeaders.Remove("X-Filename"); // Clear old value if reused
            _http.DefaultRequestHeaders.Add("X-Filename", fileName);

            var response = await _http.PostAsync("upload", content);

            if (!response.IsSuccessStatusCode)
            {
                // Extract your custom "validationResult.ErrorMessage"
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorBody ?? response.ReasonPhrase);
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
