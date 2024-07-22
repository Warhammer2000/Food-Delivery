using CloudinaryDotNet.Actions;
using CloudinaryDotNet;

namespace FoodDelivery.Services
{
    public class ImageService
    {
        private readonly Cloudinary _cloudinary;

        public ImageService(IConfiguration configuration)
        {
            var cloudinaryConfig = configuration.GetSection("Cloudinary");
            var account = new Account(
                cloudinaryConfig["CloudName"],
                cloudinaryConfig["ApiKey"],
                cloudinaryConfig["ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(string filePath)
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(filePath)
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.AbsoluteUri;
        }

        public async Task<string> UploadImageFromUrlAsync(string imageUrl)
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(imageUrl)
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.AbsoluteUri;
        }
    }
}
