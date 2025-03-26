using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Gallery Module",
    Author = "Aref Obaid",
    Website = "https://yourwebsite.com",
    Version = "1.0.0",
    Description = "A module for managing image galleries in Orchard Core",
    Category = "Content Management",
    Dependencies = new[] { 
        "OrchardCore.Contents", 
        "OrchardCore.Media", 
        "OrchardCore.Workflows", 
        "OrchardCore.Users", 
        "OrchardCore.Roles",
        "OrchardCore.Taxonomies"
    }
)]