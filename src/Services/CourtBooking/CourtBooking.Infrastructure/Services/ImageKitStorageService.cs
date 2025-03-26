using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace CourtBooking.Infrastructure.Services
{
    public class ImageKitOptions
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public string UrlEndpoint { get; set; }
    }

    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName);

        Task<List<string>> UploadFilesAsync(IList<IFormFile> files, string folderName);
    }

    public class ImageKitStorageService : IFileStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly ImageKitOptions _options;

        public ImageKitStorageService(HttpClient httpClient, IOptions<ImageKitOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
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
                await file.CopyToAsync(ms);
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
                var response = await _httpClient.PostAsync("https://upload.imagekit.io/api/v1/files/upload", content);
                var responseContent = await response.Content.ReadAsStringAsync();

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

        public async Task<List<string>> UploadFilesAsync(IList<IFormFile> files, string folderName)
        {
            var urls = new List<string>();
            if (files == null || files.Count == 0) return urls;

            foreach (var file in files)
            {
                var url = await UploadFileAsync(file, folderName);
                if (!string.IsNullOrEmpty(url))
                    urls.Add(url);
            }
            return urls;
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
    }
}