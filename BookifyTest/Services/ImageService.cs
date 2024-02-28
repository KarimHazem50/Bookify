using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace BookifyTest.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly List<string> _allowedExtension = new() { ".png", ".jpeg", ".jpg" };
        private readonly int _allowedMaxSize = 2097152;

        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<(bool isUploaded, string? errorMessage)> UploadAsync(IFormFile image, string imageName, string folderPath, bool hasThumbnail)
        {
            var extension = Path.GetExtension(image.FileName);
            if (!_allowedExtension.Contains(extension))
                return (isUploaded: false, errorMessage: Errors.NotAllowedExtension);

            if (image.Length > _allowedMaxSize)
                return (isUploaded: false, errorMessage: Errors.Maxsize);

            var path = Path.Combine($"{_webHostEnvironment.WebRootPath}{folderPath}", imageName);

            using var stream = File.Create(path);
            await image.CopyToAsync(stream);
            stream.Dispose();

            if (hasThumbnail)
            {
                var ThumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}{folderPath}/Thumb", imageName);

                using var loadedImage = Image.Load(image.OpenReadStream());
                var ratio = (float)loadedImage.Width / 175;
                var height = loadedImage.Height / ratio;
                loadedImage.Mutate(i => i.Resize(width: 175, height: (int)height));
                loadedImage.Save(ThumbPath);
            }

            return (isUploaded: true, errorMessage: null);
        }
        public void Delete(string ImageName, string imagePath, bool HasThumbnailPath)
        {
            var oldPath = Path.Combine($"{_webHostEnvironment.WebRootPath}{imagePath}/{ImageName}");
            if (File.Exists(oldPath))
                File.Delete(oldPath);

            if (HasThumbnailPath)
            {
                var oldThumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}{imagePath}/Thumb/{ImageName}");

                if (File.Exists(oldThumbPath))
                    File.Delete(oldThumbPath);
            }
        }

    }
}
