using System.ComponentModel.DataAnnotations;
using Gallery.Module.Models;
using Gallery.Module.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Media;

namespace Gallery.Module.Controllers
{
    public class GalleryController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IGalleryService _galleryService;
        private readonly IPhotoNotificationService _notificationService;
        private readonly IMediaFileStore _mediaFileStore;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<GalleryController> _htmlLocalizer;

        public GalleryController(
            IContentManager contentManager,
            IGalleryService galleryService,
            IPhotoNotificationService notificationService,
            IMediaFileStore mediaFileStore,
            INotifier notifier,
            IHtmlLocalizer<GalleryController> htmlLocalizer)
        {
            _contentManager = contentManager;
            _galleryService = galleryService;
            _notificationService = notificationService;
            _mediaFileStore = mediaFileStore;
            _notifier = notifier;
            _htmlLocalizer = htmlLocalizer; 
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
        [HttpGet] // Add this explicit attribute
        public async Task<IActionResult> Upload(string albumId = null)
        {
            var albums = await _galleryService.GetAlbumsAsync();
            var viewModel = new PhotoUploadViewModel
            {
                Albums = albums,
                AlbumId = albumId // Pre-select the album if provided
            };

            // Set a flag to indicate a specific album was selected
            ViewBag.SingleAlbumMode = !string.IsNullOrEmpty(albumId);

            return View(viewModel);
        }


        [HttpPost] 
        [Authorize]
        [ValidateAntiForgeryToken] 
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
    

            
            var photoContentItem = await _contentManager.NewAsync("Photo");
            
        
            photoContentItem.DisplayText = model.Title;
       
      
            var photoPart = photoContentItem.As<PhotoPart>();
         
            if (photoPart != null)
            { 
             
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
                       
                        ModelState.AddModelError("", $"Could not create directory: {ex.Message}");
                        model.Albums = await _galleryService.GetAlbumsAsync();
                        return View(model);
                    }
                    
          
                    using (var stream = model.ImageFile.OpenReadStream())
                    {
                        await _mediaFileStore.CreateFileFromStreamAsync(path, stream);
                    }
            
                    photoPart.ImageMediaPath = path;
                 
                }
                
    
                photoPart.AlbumContentItemId = model.AlbumId;
                photoContentItem.Apply(photoPart);
                Console.WriteLine("model.AlbumId: " + model.AlbumId);
                Console.WriteLine("testEmailCp");
                Console.WriteLine("Assigned AlbumContentItemId: " + photoPart.AlbumContentItemId);
                Console.WriteLine("model.AlbumId:" + model.AlbumId);
                Console.WriteLine("testEmailCp");
               
                if (!string.IsNullOrWhiteSpace(model.Tags))
                {
                    photoPart.Tags = new List<string>(
                        model.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim())
                    );
                }
                photoContentItem.Apply(photoPart);
                Console.WriteLine("Tags: " +photoPart.Tags[0]);
            }
          
     
            Console.WriteLine("testEmailCp");
            await _contentManager.CreateAsync(photoContentItem, VersionOptions.Published);
            
    
            Console.WriteLine("testEmailC");
            await _notificationService.NotifyNewPhotoUploadedAsync(photoContentItem.ContentItemId);

            Console.WriteLine("testEmailC");
      
            return RedirectToAction(nameof(Album), new { albumId = model.AlbumId });
        }
    }


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