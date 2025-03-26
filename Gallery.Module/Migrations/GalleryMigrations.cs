using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using System.Threading.Tasks;
namespace Gallery.Module.Migrations
{
    public class GalleryMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public GalleryMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public async Task<int> CreateAsync()
        {
            // Define AlbumPart
            await _contentDefinitionManager.AlterPartDefinitionAsync("AlbumPart", part =>
                part.Attachable()
            );

            // Define PhotoPart
            await _contentDefinitionManager.AlterPartDefinitionAsync("PhotoPart", part =>
                part.Attachable()
            );

            // Create Album content type
            await _contentDefinitionManager.AlterTypeDefinitionAsync("Album", type => type
                .DisplayedAs("Album")
                .Creatable()
                .Listable()
                .Draftable()
                .Versionable()
                .WithPart("TitlePart")
                .WithPart("AlbumPart")
            );

            // Create Photo content type
            await _contentDefinitionManager.AlterTypeDefinitionAsync("Photo", type => type
                .DisplayedAs("Photo")
                .Creatable()
                .Listable()
                .Draftable()
                .Versionable()
                .WithPart("TitlePart")
                .WithPart("PhotoPart")
            );

            return 1;
        }
    }

}