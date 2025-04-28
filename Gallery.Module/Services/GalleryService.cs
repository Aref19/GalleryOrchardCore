using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using YesSql;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using System.Linq;
using YesSql;
using OrchardCore.ContentManagement.Records;
namespace Gallery.Module.Services
{
    public class GalleryService : IGalleryService
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly IServiceProvider _serviceProvider;
        public GalleryService(
            IContentManager contentManager,
            IServiceProvider serviceProvider,ISession session)
        {
            _contentManager = contentManager;
            _serviceProvider = serviceProvider;
            _session = session;
        }

        public async Task<IEnumerable<ContentItem>> GetAlbumsAsync()
        {
            Console.WriteLine("======= ATTEMPTING TO RETRIEVE ALBUMS =======");

            try
            {
                var albums = await _session.Query<ContentItem>()
                    .With<ContentItemIndex>(x => x.ContentType == "Album" && x.Published)
                    .ListAsync();


                foreach (var album in albums)
                {
                    Console.WriteLine($"Found album: {album.DisplayText}");
                }

                Console.WriteLine("Total albums found: {albums.Count}");
                Console.WriteLine("======= END ALBUM RETRIEVAL =======");

                return albums;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"!!!! ERROR: {ex.Message}");
                return Enumerable.Empty<ContentItem>();
            }
        }

        public async Task<ContentItem> GetAlbumByIdAsync(string albumId)
        {
            return await _contentManager.GetAsync(albumId, VersionOptions.Published);
        }

        public async Task<IEnumerable<ContentItem>> GetPhotosByAlbumIdAsync(string albumId)
        {
            var photos = await _session
                .Query<ContentItem, ContentItemIndex>(index =>
                    index.ContentType == "Photo" && index.Published)
                .ListAsync();

            var photos1 = await _session
                .Query<ContentItem, ContentItemIndex>(index =>
                    index.ContentType == "Photo" && index.Published)
                .ListAsync();

            Console.WriteLine($"📦 Alle veröffentlichten Fotos: {photos.Count()}");

            foreach (var photo in photos)
            {
                Console.WriteLine($" Photo: {photo.DisplayText}, AlbumRef: {photo.Content.PhotoPart?.AlbumContentItemId}");
            }
            var results = photos
                .Where(photo => photo.Content.PhotoPart?.AlbumContentItemId == albumId)
                .ToList();

            Console.WriteLine($"Gefundene Fotos für Album {albumId}: {results.Count}");

            return results;
        }

        public async Task<ContentItem> GetPhotoByIdAsync(string photoId)
        {
            return await _contentManager.GetAsync(photoId, VersionOptions.Published);
        }
        
        
      
    }
}