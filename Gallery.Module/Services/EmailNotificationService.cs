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

        public EmailNotificationService(
            IEmailService emailService,
            IContentManager contentManager,
            ISession session)
        {
            _emailService = emailService;
            _contentManager = contentManager;
            _session = session;
        }

        public async Task NotifyNewPhotoUploadedAsync(string photoId)
        {
            try
            {
                Console.WriteLine("🔔 Start sending notifications for PhotoId: " + photoId);

                // Get the photo content item
                var photo = await _contentManager.GetAsync(photoId, VersionOptions.Published);
                if (photo == null)
                {
                    Console.WriteLine("⚠️ Photo not found.");
                    return;
                }

                // Get the album ID from PhotoPart
                var albumId = photo.Content.PhotoPart?.AlbumContentItemId?.ToString();
                if (string.IsNullOrEmpty(albumId))
                {
                    Console.WriteLine("⚠️ Album ID missing.");
                    return;
                }

                // Get the album content item
                var album = await _contentManager.GetAsync(albumId, VersionOptions.Published);
                if (album == null)
                {
                    Console.WriteLine("⚠️ Album not found.");
                    return;
                }

                // Query for users directly using YesSql
                var users = await _session
                    .Query<User, UserIndex>()
                    .ListAsync();

                Console.WriteLine($"📧 Found {users.Count()} users to notify.");

                foreach (var user in users)
                {
                    // Get the email directly from the User object
                    var email = user.Email;
                    if (string.IsNullOrEmpty(email))
                    {
                        Console.WriteLine($"⚠️ User has no email.");
                        continue;
                    }
                    var link = $"https://localhost:5001/Gallery.Module/Gallery/Photo?photoId={photo.ContentItemId}";
                    var subject = $"📸 New photo uploaded: {photo.DisplayText}";
                    var body = $@"
                        <h2>New Photo Uploaded</h2>
                        <p>A new photo was uploaded to the album <strong>{album.DisplayText}</strong>.</p>
                        <p><strong>Photo:</strong> {photo.DisplayText}</p>
                        <p><a href='{link}'>Click here to view</a></p>;
";

                    var result = await _emailService.SendAsync(email, subject, body);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"❌ Failed to send to {email}:  - {error.Value}");
                        }

                        Console.WriteLine($"❌ Failed to send to {email}: {string.Join(", ", result.Errors)}");
                    }
                    else
                    {
                        Console.WriteLine($"✅ Email sent to {email}");
                    }
                }

                Console.WriteLine("✅ Notification finished.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("🚨 Exception while sending email: " + ex.Message);
                Console.WriteLine("🚨 Exception details: " + ex.StackTrace);
            }
        }
    }
}