using B2Net;
using B2Net.Models;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Coach.API.Services
{
    public class BackblazeSettings
    {
        public string KeyId { get; set; }
        public string ApplicationKey { get; set; }
        public string BucketId { get; set; }
        public string BaseUrl { get; set; }
    }

    public interface IBackblazeService
    {
        Task<(string Url, string FileId)> UploadFileAsync(IFormFile file, string folderName, CancellationToken cancellationToken = default);

        Task DeleteFileAsync(string fileId, string fileName, CancellationToken cancellationToken = default);
    }

    public class BackblazeService : IBackblazeService
    {
        private readonly B2Client _b2Client;
        private readonly BackblazeSettings _settings;

        public BackblazeService(IOptions<BackblazeSettings> settings)
        {
            _settings = settings.Value;
            _b2Client = new B2Client(_settings.KeyId, _settings.ApplicationKey);
        }

        public async Task<(string Url, string FileId)> UploadFileAsync(IFormFile file, string folderName, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null", nameof(file));

            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{folderName}/{Guid.NewGuid()}{fileExtension}";

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream, cancellationToken);
            var fileData = stream.ToArray();

            var uploadUrl = await _b2Client.Files.GetUploadUrl(_settings.BucketId);

            var response = await _b2Client.Files.Upload(fileData, fileName, uploadUrl, "application/octet-stream");

            return ($"{_settings.BaseUrl}/file/{_settings.BucketId}/{response.FileName}", response.FileId);
        }

        public async Task DeleteFileAsync(string fileId, string fileName, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(fileId))
            {
                try
                {
                    await _b2Client.Files.Delete(fileId, fileName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting file {fileName}: {ex.Message}");
                }
            }
        }
    }
}