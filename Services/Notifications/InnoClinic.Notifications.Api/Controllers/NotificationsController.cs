using InnoClinic.Notifications.Application.Interfaces;
using InnoClinic.Notifications.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace InnoClinic.Notifications.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController(INotificationRepository repository) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] Notification request, CancellationToken ct)
        {
            repository.Add(request);
            await repository.SaveChangesAsync(ct);
            return Ok(new { Message = "Notification scheduled" });
        }
    }
}