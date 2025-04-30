using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

using System;
using System.Threading.Tasks;

namespace Gallery.Module.Migrations
{
    // Class that sets up the content structure for the gallery module
    public class GalleryMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        // Constructor starts the migration immediately
        public GalleryMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
            CreateAsync(); // This calls the migration at startup
        }

        // Creates all the content definitions needed for the gallery
        public async Task<int> CreateAsync()
        {
            // PhotoPart - Defines structure for photos with tags
            await _contentDefinitionManager.AlterPartDefinitionAsync("PhotoPart", part => part
                .WithDisplayName("PhotoPart")
                .WithField("Tags", field => field.OfType("TextField"))
                .Attachable()
            );

            // AlbumPart - Defines structure for albums with description
            await _contentDefinitionManager.AlterPartDefinitionAsync("AlbumPart", part => part
                .WithDisplayName("AlbumPart")
                .WithField("Description", field => field.OfType("TextField"))
                .Attachable()
            );
            
            // Album content type - Container for photos
            await _contentDefinitionManager.AlterTypeDefinitionAsync("Album", type => type
                .Creatable().Listable().Draftable().Versionable()
                .WithPart("TitlePart")
                .WithPart("AlbumPart")
            );

            // Photo content type - Individual photos that belong to albums
            await _contentDefinitionManager.AlterTypeDefinitionAsync("Photo", type => type
                .Creatable().Listable().Draftable().Versionable()
                .WithPart("TitlePart")
                .WithPart("PhotoPart")
            );

            return 1; 
        }
    }
}