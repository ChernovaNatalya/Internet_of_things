namespace server;

public sealed class TimerService(ILogger<TimerService> logger, IServiceScopeFactory serviceScopeFactory) : IHostedService, IAsyncDisposable
{
    private readonly Task _completedTask = Task.CompletedTask;
    private Timer? _timer;

    public Task StartAsync(CancellationToken stoppingToken)
    {
         logger.LogInformation("{Service} is running.", nameof(TimerService));
         _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));

        return _completedTask;
    }

    private async void DoWork(object? state)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var httpFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();
        using var httpClient = httpFactory!.CreateClient();
        var roomService = scope.ServiceProvider.GetService<IRoomService>();
        foreach (var room in roomService!.GetRooms())
        {
            var result = await httpClient.GetFromJsonAsync<DeviceData>("http://localhost:5120");
            var storage = scope.ServiceProvider.GetService<IDeviceDataStorage>();
            storage!.SaveData(room.Id, DateTime.Now, result!);
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "{Service} is stopping.", nameof(TimerService));

        _timer?.Change(Timeout.Infinite, 0);

        return _completedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_timer is IAsyncDisposable timer)
        {
            await timer.DisposeAsync();
        }

        _timer = null;
    }
}

public record DeviceData(float Temperature, float Humidity, float Light);
