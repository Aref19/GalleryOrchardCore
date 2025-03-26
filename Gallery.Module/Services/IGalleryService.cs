using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gallery.Module.Services
{
    public interface IGalleryService
    {
        Task<IEnumerable<ContentItem>> GetAlbumsAsync();
        Task<ContentItem> GetAlbumByIdAsync(string albumId);
        Task<IEnumerable<ContentItem>> GetPhotosByAlbumIdAsync(string albumId);
        Task<ContentItem> GetPhotoByIdAsync(string photoId);
    }
}