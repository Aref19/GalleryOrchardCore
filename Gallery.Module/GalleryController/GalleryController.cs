using System.ComponentModel.DataAnnotations;
using Gallery.Module.Models;
using Gallery.Module.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.Media;

namespace Gallery.Module.Controllers
{
    public class GalleryController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IGalleryService _galleryService;
        private readonly IPhotoNotificationService _notificationService;
        private readonly IMediaFileStore _mediaFileStore;

        public GalleryController(
            IContentManager contentManager,
            IGalleryService galleryService,
            IPhotoNotificationService notificationService,
            IMediaFileStore mediaFileStore)
        {
            _contentManager = contentManager;
            _galleryService = galleryService;
            _notificationService = notificationService;
            _mediaFileStore = mediaFileStore;
        }

        // GET: /Gallery
        public async Task<IActionResult> Index()
        {
            var albums = await _galleryService.GetAlbumsAsync();
            return View(albums);
        }

        // GET: /Gallery/Album/{albumId}
        public async Task<IActionResult> Album(string albumId)
        {
            var album = await _galleryService.GetAlbumByIdAsync(albumId);
            if (album == null)
            {
                return NotFound();
            }

            var photos = await _galleryService.GetPhotosByAlbumIdAsync(albumId);
            
            var viewModel = new AlbumViewModel
            {
                Album = album,
                Photos = photos
            };

            return View(viewModel);
        }

        // GET: /Gallery/Photo/{photoId}
        public async Task<IActionResult> Photo(string photoId)
        {
            var photo = await _galleryService.GetPhotoByIdAsync(photoId);
            if (photo == null)
            {
                return NotFound();
            }

            return View(photo);
        }

        // GET: /Gallery/Upload
        [Authorize]
        public async Task<IActionResult> Upload()
        {
            var albums = await _galleryService.GetAlbumsAsync();
            var viewModel = new PhotoUploadViewModel
            {
                Albums = albums
            };

            return View(viewModel);
        }

        // POST: /Gallery/Upload
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Upload(PhotoUploadViewModel model)
        {
            Console.WriteLine("model.ImageFile " +model.ImageFile );
            
            
            foreach (var error in ModelState)
            {
                Console.WriteLine($"ModelState Key: {error.Key}");
                foreach (var err in error.Value.Errors)
                {
                    Console.WriteLine($" -> Error: {err.ErrorMessage}");
                }
            }
            ModelState.Remove("Albums");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("probleme" +model );
                model.Albums = await _galleryService.GetAlbumsAsync();
                return View(model);
            }

            // Handle file upload
            var photoContentItem = await _contentManager.NewAsync("Photo");
            
            // Update title
            photoContentItem.DisplayText = model.Title;
       
            // Get the PhotoPart and update its properties
            var photoPart = photoContentItem.As<PhotoPart>();
         
            if (photoPart != null)
            {
                // Save the uploaded image to media storage
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(model.ImageFile.FileName);
                    var path = $"/gallery/photos/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}_{fileName}";
                    
                    // Ensure the directory exists
                    try
                    {
                        var directoryPath = Path.GetDirectoryName(path);
                        await _mediaFileStore.TryCreateDirectoryAsync(directoryPath);
                    }
                    catch(Exception ex)
                    {
                        // Log the exception or handle it appropriately
                        ModelState.AddModelError("", $"Could not create directory: {ex.Message}");
                        model.Albums = await _galleryService.GetAlbumsAsync();
                        return View(model);
                    }
                    
                    // Save the file
                    using (var stream = model.ImageFile.OpenReadStream())
                    {
                        await _mediaFileStore.CreateFileFromStreamAsync(path, stream);
                    }
                    
                    // Store the path in the PhotoPart
                    photoPart.ImageMediaPath = path;
                 
                }
                
                // Set album reference
                photoPart.AlbumContentItemId = model.AlbumId;
                photoContentItem.Apply(photoPart);
                Console.WriteLine("model.AlbumId: " + model.AlbumId);
                Console.WriteLine("Assigned AlbumContentItemId: " + photoPart.AlbumContentItemId);
                Console.WriteLine("model.AlbumId:" + model.AlbumId);
                // Handle tags
                if (!string.IsNullOrWhiteSpace(model.Tags))
                {
                    photoPart.Tags = new List<string>(
                        model.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim())
                    );
                }
            }
            
            // Publish the content item
            await _contentManager.CreateAsync(photoContentItem, VersionOptions.Published);
            
            // Send notification to other users
            await _notificationService.NotifyNewPhotoUploadedAsync(photoContentItem.ContentItemId);
            
            // Redirect to the album page
            return RedirectToAction(nameof(Album), new { albumId = model.AlbumId });
        }
    }

    // View models
    public class AlbumViewModel
    {
        public ContentItem Album { get; set; }
        public IEnumerable<ContentItem> Photos { get; set; }
    }

    public class PhotoUploadViewModel
    {
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Please choose an image.")]
        public IFormFile ImageFile { get; set; }
        [Required(ErrorMessage = "Album selection is required.")]
        public string AlbumId { get; set; }
        public string Tags { get; set; }
        [BindNever]
        public IEnumerable<ContentItem> Albums { get; set; }
    }
    
  
}