﻿using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

using System;
using System.Threading.Tasks;

namespace Gallery.Module.Migrations
{
    public class GalleryMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public GalleryMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
            CreateAsync();
        }

        public async Task<int> CreateAsync()
        {
            // Define PhotoPart
            await _contentDefinitionManager.AlterPartDefinitionAsync("PhotoPart", part => part
                .WithDisplayName("PhotoPart")
                .WithField("Tags", field => field
                    .OfType("TextField")
                    .WithDisplayName("Tags")
                )
                .WithDescription("Tags for the photo")
                .Attachable()
            );

            // Define AlbumPart
            await _contentDefinitionManager.AlterPartDefinitionAsync("AlbumPart", part => part
                .WithDisplayName("AlbumPart")
                .Attachable()
                .WithField("Description", field => field
                    .OfType("TextField")
                    .WithDisplayName("Description")
                )
                .WithDescription("Description for the album")
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

            return 1; // Version 1 für die erste Installation
        }
    }
}