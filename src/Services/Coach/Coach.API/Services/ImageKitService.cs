using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace Coach.API.Services
{
    public class ImageKitOptions
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public string UrlEndpoint { get; set; }
    }

    public class ImageKitService : IImageKitService
    {
        private readonly HttpClient _httpClient;
        private readonly ImageKitOptions _options;

        public ImageKitService(HttpClient httpClient, IOptions<ImageKitOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName, CancellationToken cancellationToken = default)
        {
            if (file == null) return "";

            try
            {
                // Create a unique filename
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";

                // Create multipart form data
                using var content = new MultipartFormDataContent();

                // Read file into memory stream
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms, cancellationToken);
                ms.Position = 0;

                // Set up the request with the file
                var fileContent = new ByteArrayContent(ms.ToArray());
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType ?? "application/octet-stream");

                // Add file and other required parameters
                content.Add(fileContent, "file", fileName);
                content.Add(new StringContent(fileName), "fileName");
                content.Add(new StringContent(folderName), "folder");
                content.Add(new StringContent("true"), "useUniqueFileName");

                // Add basic authentication
                var authString = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.PrivateKey}:"));

                // Set up the request
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);

                // Make the request to ImageKit API
                var response = await _httpClient.PostAsync("https://upload.imagekit.io/api/v1/files/upload", content, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                // Check for success
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to upload file to ImageKit: {responseContent}");
                }

                // Parse the response
                var result = JsonSerializer.Deserialize<ImageKitUploadResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Return the URL from the response
                return result.Url;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading to ImageKit: {ex.Message}", ex);
            }
        }

        public async Task<List<string>> UploadFilesAsync(IList<IFormFile> files, string folderName, CancellationToken cancellationToken = default)
        {
            var urls = new List<string>();
            if (files == null || files.Count == 0) return urls;

            foreach (var file in files)
            {
                var url = await UploadFileAsync(file, folderName, cancellationToken);
                if (!string.IsNullOrEmpty(url))
                    urls.Add(url);
            }
            return urls;
        }

        public async Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return false;

            try
            {
                // Extract fileId from URL or get it from metadata
                string fileId = ExtractFileIdFromUrl(fileUrl);

                if (string.IsNullOrEmpty(fileId))
                {
                    // If we can't extract fileId, we need to search for it
                    fileId = await GetFileIdByUrl(fileUrl, cancellationToken);
                }

                if (string.IsNullOrEmpty(fileId))
                {
                    // We couldn't find the file ID
                    return false;
                }

                // Add basic authentication
                var authString = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.PrivateKey}:"));
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);

                // Make the DELETE request to ImageKit API
                var response = await _httpClient.DeleteAsync($"https://api.imagekit.io/v1/files/{fileId}", cancellationToken);

                // Check for success
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error deleting file from ImageKit: {ex.Message}");
                return false;
            }
        }

        private string ExtractFileIdFromUrl(string url)
        {
            // ImageKit URLs typically have the file ID in them
            // Example: https://ik.imagekit.io/youraccountid/path/to/file/filename_fileId.jpg

            try
            {
                // This is a simplified approach - you might need to adjust based on your actual URL format
                if (url.Contains("/"))
                {
                    var parts = url.Split('/');
                    var filename = parts[parts.Length - 1];

                    // The filename might contain the fileId
                    if (filename.Contains("_"))
                    {
                        var filenameParts = filename.Split('_');
                        // The last part might be the fileId
                        return filenameParts[filenameParts.Length - 1].Split('.')[0];
                    }
                }
            }
            catch
            {
                // If parsing fails, return empty
            }

            return string.Empty;
        }

        private async Task<string> GetFileIdByUrl(string url, CancellationToken cancellationToken)
        {
            try
            {
                // Add basic authentication
                var authString = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.PrivateKey}:"));
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);

                // List files to find the one with matching URL
                var response = await _httpClient.GetAsync("https://api.imagekit.io/v1/files?limit=1000", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var files = JsonSerializer.Deserialize<List<ImageKitFileResponse>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Find file with matching URL
                    var file = files?.FirstOrDefault(f => f.Url == url);
                    return file?.FileId ?? string.Empty;
                }
            }
            catch
            {
                // If searching fails, return empty
            }

            return string.Empty;
        }

        private class ImageKitUploadResponse
        {
            public string FileId { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
            public string ThumbnailUrl { get; set; }
            public long Size { get; set; }
            public string FilePath { get; set; }
        }

        private class ImageKitFileResponse
        {
            public string FileId { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
            public string FilePath { get; set; }
        }
    }
}