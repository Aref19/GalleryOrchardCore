using OrchardCore.ContentManagement;
using YesSql;
using OrchardCore.ContentManagement.Records;
namespace Gallery.Module.Services
{
    public class GalleryService : IGalleryService
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly IServiceProvider _serviceProvider;

        // Constructor - initializes the service with required dependencies
        public GalleryService(
            IContentManager contentManager,     // Manages content operations
            IServiceProvider serviceProvider,   // Provides access to services
            ISession session)                   // Manages database operations
        {
            _contentManager = contentManager;
            _serviceProvider = serviceProvider;
            _session = session;
        }

        // Retrieves all published albums from the database
        public async Task<IEnumerable<ContentItem>> GetAlbumsAsync()
        {
            Console.WriteLine("======= ATTEMPTING TO RETRIEVE ALBUMS =======");

            try
            {
                // Query the database for all published album content items
                var albums = await _session.Query<ContentItem>()
                    .With<ContentItemIndex>(x => x.ContentType == "Album" && x.Published)
                    .ListAsync();

                // Log each found album for debugging
                foreach (var album in albums)
                {
                    Console.WriteLine($"Found album: {album.DisplayText}");
                }

                // Log the total count of albums found
                Console.WriteLine("Total albums found: {albums.Count}");
                Console.WriteLine("======= END ALBUM RETRIEVAL =======");

                return albums;
            }
            catch (Exception ex)
            {
                // Log any exceptions and return an empty collection
                Console.WriteLine($"!!!! ERROR: {ex.Message}");
                return Enumerable.Empty<ContentItem>();
            }
        }

        // Retrieves a specific album by its ID
        public async Task<ContentItem> GetAlbumByIdAsync(string albumId)
        {
            // Use ContentManager to get the album with the specified ID
            return await _contentManager.GetAsync(albumId, VersionOptions.Published);
        }

        // Retrieves all photos that belong to a specific album
        public async Task<IEnumerable<ContentItem>> GetPhotosByAlbumIdAsync(string albumId)
        {
            // Query the database for all published photo content items
            var photos = await _session
                .Query<ContentItem, ContentItemIndex>(index =>
                    index.ContentType == "Photo" && index.Published)
                .ListAsync();

            // Duplicate query for debugging purposes
            var photos1 = await _session
                .Query<ContentItem, ContentItemIndex>(index =>
                    index.ContentType == "Photo" && index.Published)
                .ListAsync();

            // Log the total count of all published photos
            Console.WriteLine($"📦 All published photos: {photos.Count()}");

            // Log each photo and its associated album reference
            foreach (var photo in photos)
            {
                Console.WriteLine($" Photo: {photo.DisplayText}, AlbumRef: {photo.Content.PhotoPart?.AlbumContentItemId}");
            }

            // Filter photos to only include those belonging to the specified album
            var results = photos
                .Where(photo => photo.Content.PhotoPart?.AlbumContentItemId == albumId)
                .ToList();

            // Log the count of photos found for this album
            Console.WriteLine($"Photos found for album {albumId}: {results.Count}");

            return results;
        }

        // Retrieves a specific photo by its ID
        public async Task<ContentItem> GetPhotoByIdAsync(string photoId)
        {
            // Use ContentManager to get the photo with the specified ID
            return await _contentManager.GetAsync(photoId, VersionOptions.Published);
        }
    }
}