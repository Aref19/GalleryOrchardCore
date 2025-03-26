using OrchardCore.Email;
using System.Threading.Tasks;
using OrchardCore.Email;
namespace Gallery.Module.Services
{
    public class EmailNotificationService : IPhotoNotificationService
    {
        private readonly IEmailService _emailService;
        
        public EmailNotificationService(IEmailService emailService)
        {
            _emailService = emailService;
        }
        
        public async Task NotifyNewPhotoUploadedAsync(string photoId)
        {
            // Implement email notification logic here
            await Task.CompletedTask;
        }
    }
}