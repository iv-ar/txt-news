namespace I2R.LightNews.Services.Hosted;

public class RadioIndexerService : IHostedService, IDisposable
{
    private readonly NrkRadioService _radio;
    private readonly ILogger<RadioIndexerService> _logger;
    private Timer _timer;

    public RadioIndexerService(NrkRadioService radio, ILogger<RadioIndexerService> logger) {
        _radio = radio;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("Radio Indexer Service running.");

        async void Callback(object state) {
            await _radio.CreateIndexAsync(cancellationToken);
        }

        _timer = new Timer(Callback, null, TimeSpan.Zero, TimeSpan.FromMinutes(20));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("Radio Indexer Service is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() {
        _timer?.Dispose();
    }
}
