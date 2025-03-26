using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace CourtBooking.Infrastructure.Services
{
    public class B2FileStorageOptions
    {
        public string KeyId { get; set; }
        public string KeyName { get; set; }
        public string ApplicationKey { get; set; }
        public string BucketId { get; set; }
        public string BaseUrl { get; set; }
    }

    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName);
        Task<List<string>> UploadFilesAsync(IList<IFormFile> files, string folderName);
    }

    public class B2FileStorageService : IFileStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly B2FileStorageOptions _options;
        private string _authToken;
        private string _uploadUrl;
        private string _uploadAuthToken;

        public B2FileStorageService(HttpClient httpClient, IOptions<B2FileStorageOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null) return "";

            // Make sure we have valid auth tokens
            await EnsureAuthorizedAsync();

            // Create a unique filename
            var fileName = $"{folderName}/{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";

            // Calculate SHA1 hash
            string sha1;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                ms.Position = 0;
                using (var sha1Provider = SHA1.Create())
                {
                    sha1 = BitConverter.ToString(sha1Provider.ComputeHash(ms)).Replace("-", "").ToLower();
                }
                ms.Position = 0;

                // Create a new HTTP client specifically for this upload
                using var uploadClient = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, _uploadUrl);

                // Add headers properly without validation to maintain exact format
                request.Headers.TryAddWithoutValidation("Authorization", _uploadAuthToken);
                request.Headers.TryAddWithoutValidation("X-Bz-File-Name", Uri.EscapeDataString(fileName));
                request.Headers.TryAddWithoutValidation("X-Bz-Content-Sha1", sha1);

                // IMPORTANT: Set Content-Type explicitly on the content, not as a header
                var content = new StreamContent(ms);
                content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
                request.Content = content;

                // Send the upload request
                var response = await uploadClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to upload file: {responseContent}");
                }

                var result = JsonSerializer.Deserialize<B2UploadFileResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Return the full URL to the uploaded file
                return $"{_options.BaseUrl}/file/{result.BucketName}/{result.FileName}";
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

        private async Task EnsureAuthorizedAsync()
        {
            // If we already have auth info, don't re-authorize
            if (!string.IsNullOrEmpty(_authToken) && !string.IsNullOrEmpty(_uploadUrl))
                return;

            // Authorize with B2
            var authString = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.KeyId}:{_options.ApplicationKey}"));

            using var authClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.backblazeb2.com/b2api/v2/b2_authorize_account");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authString);

            var response = await authClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to authorize with B2: {content}");
            }

            var authResponse = JsonSerializer.Deserialize<B2AuthResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            _authToken = authResponse.AuthorizationToken;

            // Get upload URL for our bucket
            using var uploadUrlClient = new HttpClient();
            var uploadUrlRequest = new HttpRequestMessage(HttpMethod.Post, $"{authResponse.ApiUrl}/b2api/v2/b2_get_upload_url");

            // Use TryAddWithoutValidation to bypass header validation
            uploadUrlRequest.Headers.TryAddWithoutValidation("Authorization", authResponse.AuthorizationToken);
            uploadUrlRequest.Content = new StringContent($"{{\"bucketId\":\"{_options.BucketId}\"}}", Encoding.UTF8, "application/json");

            var uploadUrlResponse = await uploadUrlClient.SendAsync(uploadUrlRequest);
            var uploadUrlContent = await uploadUrlResponse.Content.ReadAsStringAsync();

            if (!uploadUrlResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get upload URL: {uploadUrlContent}");
            }

            var uploadUrlResult = JsonSerializer.Deserialize<B2UploadUrlResponse>(uploadUrlContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            _uploadUrl = uploadUrlResult.UploadUrl;
            _uploadAuthToken = uploadUrlResult.AuthorizationToken;
        }

        private class B2AuthResponse
        {
            public string AuthorizationToken { get; set; }
            public string ApiUrl { get; set; }
            public string DownloadUrl { get; set; }
        }

        private class B2UploadUrlResponse
        {
            public string UploadUrl { get; set; }
            public string AuthorizationToken { get; set; }
        }

        private class B2UploadFileResponse
        {
            public string FileId { get; set; }
            public string FileName { get; set; }
            public string BucketId { get; set; }
            public string BucketName { get; set; }
        }
    }
}