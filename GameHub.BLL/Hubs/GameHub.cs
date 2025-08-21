using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GameHub.BLL.Hubs;

public class GameHub : Hub
{
    private readonly ILogger<GameHub> _logger;

    public GameHub(ILogger<GameHub> logger)
    {
        _logger = logger;
    }

    // Simple notification methods like the working project
    public async Task NotifyGameUpdated(int gameId)
    {
        await Clients.All.SendAsync("GameUpdated", gameId);
    }

    public async Task NotifyGameDeleted(int gameId)  
    {
        await Clients.All.SendAsync("GameDeleted", gameId);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client {Context.ConnectionId} connected to GameHub");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client {Context.ConnectionId} disconnected from GameHub");
        await base.OnDisconnectedAsync(exception);
    }
}
