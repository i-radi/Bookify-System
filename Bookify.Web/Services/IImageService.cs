namespace Bookify.Web.Services
{
    public interface IImageService
    {
        Task<(bool isUploaded, string? errorMessage)> UploadAsync(IFormFile image, string imageName, string folderPath, bool hasThumbnail);
        void Delete(string imagePath, string? imageThumbnailPath = null);
    }
}