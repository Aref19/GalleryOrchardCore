using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using Gallery.Module.Models;

namespace Gallery.Module;

public sealed class Startup : StartupBase
{
    
    
    
    public override void ConfigureServices(IServiceCollection services)
    {
        
        
     
        // Register content parts
        services.AddContentPart<Models.AlbumPart>();
        services.AddContentPart<Models.PhotoPart>();
    
        // Register migrations for setting up content types
        services.AddScoped<OrchardCore.Data.Migration.IDataMigration, Migrations.GalleryMigrations>();
    
        // Register services for your module
        services.AddScoped<Services.IGalleryService, Services.GalleryService>();
        services.AddScoped<Services.IPhotoNotificationService, Services.EmailNotificationService>();
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        // Add this route for the homepage
        routes.MapAreaControllerRoute(
            name: "Home",
            areaName: "Gallery.Module",
            pattern: "/",
            defaults: new { controller = "Gallery", action = "Index" }
        );

        // Keep your existing routes
        routes.MapAreaControllerRoute(
            name: "GalleryHome",
            areaName: "Gallery.Module",
            pattern: "Gallery",
            defaults: new { controller = "Gallery", action = "Index" }
        );

        routes.MapAreaControllerRoute(
            name: "GalleryUpload",
            areaName: "Gallery.Module",
            pattern: "Gallery/Upload",
            defaults: new { controller = "Gallery", action = "Upload" }
        );
    }
}
