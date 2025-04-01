namespace Coach.API.Services
{
    public interface IImageKitService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName, CancellationToken cancellationToken = default);
        Task<List<string>> UploadFilesAsync(IList<IFormFile> files, string folderName, CancellationToken cancellationToken = default);
        Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
    }
}