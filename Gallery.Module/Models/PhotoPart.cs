namespace Gallery.Module.Models;
using OrchardCore.ContentManagement;

public class PhotoPart : ContentPart
{
    public string ImageMediaPath { get; set; }
    public string AlbumContentItemId { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
}
