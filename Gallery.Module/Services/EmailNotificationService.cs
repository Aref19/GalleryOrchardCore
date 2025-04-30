using Microsoft.AspNetCore.Identity;
using OrchardCore.ContentManagement;
using OrchardCore.Email;
using OrchardCore.Users;
using YesSql;
using OrchardCore.Users.Models;
using OrchardCore.Users.Indexes;

namespace Gallery.Module.Services
{
    public class EmailNotificationService : IPhotoNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly IContentManager _contentManager;
        private readonly ISession _session;

        // Constructor - initializes the service with necessary dependencies
        public EmailNotificationService(
            IEmailService emailService,        // For sending emails
            IContentManager contentManager,     // For content management
            ISession session)                   // For database access
        {
            _emailService = emailService;
            _contentManager = contentManager;
            _session = session;
        }

        // Method to notify users when a new photo is uploaded
        public async Task NotifyNewPhotoUploadedAsync(string photoId)
        {
            try
            {
                // Log start of notification process
                Console.WriteLine(" Start sending notifications for PhotoId: " + photoId);

                // Get the photo content item
                var photo = await _contentManager.GetAsync(photoId, VersionOptions.Published);
                if (photo == null)
                {
                    // Log and exit if photo not found
                    Console.WriteLine(" Photo not found.");
                    return;
                }

                // Get the album ID from PhotoPart
                var albumId = photo.Content.PhotoPart?.AlbumContentItemId?.ToString();
                if (string.IsNullOrEmpty(albumId))
                {
                    // Log and exit if album ID is missing
                    Console.WriteLine(" Album ID missing.");
                    return;
                }

                // Get the album content item
                var album = await _contentManager.GetAsync(albumId, VersionOptions.Published);
                if (album == null)
                {
                    // Log and exit if album not found
                    Console.WriteLine(" Album not found.");
                    return;
                }

                // Query for all users
                var users = await _session
                    .Query<User, UserIndex>()
                    .ListAsync();

                // Log number of users found
                Console.WriteLine($"📧 Found {users.Count()} users to notify.");

                // Loop through each user to send notification
                foreach (var user in users)
                {
                    // Get the user's email
                    var email = user.Email;
                    if (string.IsNullOrEmpty(email))
                    {
                        // Skip users without email
                        Console.WriteLine($"⚠️ User has no email.");
                        continue;
                    }
                    
                    // Create the email content
                    var link = $"https://localhost:5001/Gallery.Module/Gallery/Photo?photoId={photo.ContentItemId}";
                    var subject = $"📸 New photo uploaded: {photo.DisplayText}";
                    var body = $@"
                        <h2>New Photo Uploaded</h2>
                        <p>A new photo was uploaded to the album <strong>{album.DisplayText}</strong>.</p>
                        <p><strong>Photo:</strong> {photo.DisplayText}</p>
                        <p><a href='{link}'>Click here to view</a></p>";

                    // Send the email
                    var result = await _emailService.SendAsync(email, subject, body);

                    // Log the result
                    if (!result.Succeeded)
                    {
                        // Log email send failures
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($" Failed to send to {email}:  - {error.Value}");
                        }

                        Console.WriteLine($" Failed to send to {email}: {string.Join(", ", result.Errors)}");
                    }
                    else
                    {
                        // Log email send success
                        Console.WriteLine($" Email sent to {email}");
                    }
                }

                // Log completion of notification process
                Console.WriteLine(" Notification finished.");
            }
            catch (Exception ex)
            {
                // Log any exceptions during the process
                Console.WriteLine(" Exception while sending email: " + ex.Message);
                Console.WriteLine(" Exception details: " + ex.StackTrace);
            }
        }
    }
}