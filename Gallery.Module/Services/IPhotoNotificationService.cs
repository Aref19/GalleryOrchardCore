using System.Threading.Tasks;

namespace Gallery.Module.Services
{
    public interface IPhotoNotificationService
    {
        Task NotifyNewPhotoUploadedAsync(string photoId);
    }
}