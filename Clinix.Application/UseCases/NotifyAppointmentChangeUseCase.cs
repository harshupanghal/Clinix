using Clinix.Application.Services;
using Microsoft.AspNetCore.SignalR;

namespace Clinix.Application.UseCases;

public class NotifyAppointmentChangeUseCase
    {
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotifyAppointmentChangeUseCase(IHubContext<NotificationHub> hubContext)
        {
        _hubContext = hubContext;
        }

    public async Task NotifyAsync(string userId, string message)
        {
        await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message);
        }
    }
